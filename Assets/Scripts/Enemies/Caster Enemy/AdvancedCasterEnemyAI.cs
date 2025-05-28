using UnityEngine;
using UnityEngine.AI;
using System.Collections;
using System;
using System.Linq;
using System.Text.RegularExpressions;
using UnityEngine.Serialization;

/// <summary>
/// Ranged enemy that patrols way-points tagged “Waypoint”, detects towers,
/// stops at attack range and fires a pooled beam while playing an animation.
/// Compatible with IEnemy / IStunnable.
/// </summary>
[RequireComponent(typeof(NavMeshAgent), typeof(Animator))]
public class AdvancedCasterEnemyAI : MonoBehaviour, IEnemy, IStunnable
{
    enum State { Idle, Patrol, Chase, Cast, Stunned, Dead }

    /*───────────── Inspector ─────────────*/
    [Header("Navigation")]
    [Tooltip("Leave empty: auto-filled with all GameObjects tagged ‘Waypoint’")]
    [SerializeField] Transform[] waypoints;
    [SerializeField] float waypointTolerance = 0.3f;
    [SerializeField] float pathRefreshRate   = 0.25f;
    [Tooltip("Radio de dispersión al pillarse un waypoint")]
    [SerializeField] float waypointJitter     = 0.5f;

    [Header("Detection")]
    [SerializeField] float detectionRadius = 15f;
    [SerializeField, Range(0,180)] float fovAngle = 90f;
    [SerializeField] float forgetDistance = 25f;
    [SerializeField] LayerMask towerMask;

    [Header("Combat")]
    [SerializeField] float attackRange  = 10f;
    [SerializeField] float windUp       = 0.6f;     // anim frozen
    [SerializeField] float castClipTime = 2.066f;   // full CastLoop at Animator speed
    [SerializeField] float fireDelay    = 0.8f;     // time inside CastLoop to shoot
    [SerializeField] float cooldownTime = 2.5f;
    [SerializeField] int   damage       = 20;
    [SerializeField] GameObject beamPrefab;
    [SerializeField] string beamPoolKey = "CasterBeam";

    [Header("Animator")]
    [SerializeField] string moveState = "Move";
    [SerializeField] string castState = "CastLoop";

    [Header("Health / VFX")]
    public int   maxHealth      = 60;
    [SerializeField] GameObject deathVFX;
    [SerializeField] float corpseLifeTime = 4f;

    [Header("Stun")]
    [SerializeField] bool  canBeStunned   = true;
    [SerializeField, Range(0,1)] float stunResistance = 0f;

    /*──────────── Runtime vars ───────────*/
    State   state;
    NavMeshAgent agent;
    Animator anim;
    int moveHash, castHash;
    Transform tower;
    int nextWP;
    Coroutine chaseLoop;
    float cooldown;
    [HideInInspector] public int   currentHealth;
    float savedSpeed;
    bool  rotating;

    public static event Action<AdvancedCasterEnemyAI> OnCasterKilled;

    /*──────────── Setup ───────────*/
    void Awake()
    {
        agent      = GetComponent<NavMeshAgent>();
        anim       = GetComponent<Animator>();
        moveHash   = Animator.StringToHash(moveState);
        castHash   = Animator.StringToHash(castState);
        currentHealth     = maxHealth;
        savedSpeed = anim.speed;
        AutoFillWaypoints();
    }

    void AutoFillWaypoints()
    {
        if (waypoints != null && waypoints.Length > 0) return;
        var objs = GameObject.FindGameObjectsWithTag("Waypoint");
        if (objs.Length == 0) return;

        waypoints = objs
            .OrderBy(o =>
            {
                var m = Regex.Match(o.name, "(\\d+)");
                return m.Success ? int.Parse(m.Value) : int.MaxValue;
            })
            .ThenBy(o => o.name)
            .Select(o => o.transform)
            .ToArray();
    }

    void OnEnable() => ChangeState(HasWaypoints ? State.Patrol : State.Idle);

    void Update()
    {
        if (state is State.Dead or State.Stunned) return;

        cooldown = Mathf.Max(0, cooldown - Time.deltaTime);

        switch (state)
        {
            case State.Idle:   TryDetectTower(); break;
            case State.Patrol: PatrolTick(); TryDetectTower(); break;
            case State.Chase:  ChaseTick(); break;
            case State.Cast:   RotateTowards(tower); ValidateTower(); break;
        }
    }

    /*──────────── FSM ───────────*/
    void ChangeState(State s)
    {
        if (state == s) return;

        if (state == State.Chase && chaseLoop != null) StopCoroutine(chaseLoop);
        if (state == State.Stunned) anim.speed = savedSpeed;

        state = s;

        switch (state)
        {
            case State.Idle:    StopMove();                                   break;
            case State.Patrol:  StartMove(); MoveToWP();                      break;
            case State.Chase:   StartMove(); chaseLoop = StartCoroutine(ChasePath()); break;
            case State.Cast:    StopMove();  StartCoroutine(CastLoop());      break;
            case State.Stunned: StopMove();  anim.speed = 0;                  break;
            case State.Dead:    Die();                                        break;
        }
    }

    /*──────── Navigation / Patrol ────────*/
    bool HasWaypoints => waypoints != null && waypoints.Length > 0;
    
    void MoveToWP()
    {
        var basePos = waypoints[nextWP].position;
        // genera un jitter horizontal
        Vector3 jitter = UnityEngine.Random.insideUnitSphere * waypointJitter;
        jitter.y = 0f;
        agent.SetDestination(basePos + jitter);
    }

    void PatrolTick()
    {
        if (!agent.pathPending && agent.remainingDistance <= waypointTolerance)
        {
            nextWP = (nextWP + 1) % waypoints.Length;
            MoveToWP();
        }
    }

    /*──────── Detection / Chase ─────────*/
    void TryDetectTower()
    {
        foreach (var col in Physics.OverlapSphere(transform.position, detectionRadius, towerMask))
        {
            if (!col.TryGetComponent<TowerAI>(out var tw)) continue;
            var dir = (tw.transform.position - transform.position).normalized;
            if (Vector3.Angle(transform.forward, dir) <= fovAngle * 0.5f)
            { tower = tw.transform; ChangeState(State.Chase); return; }
        }
    }

    IEnumerator ChasePath()
    {
        var wait = new WaitForSeconds(pathRefreshRate);
        while (state == State.Chase)
        {
            if (tower) agent.SetDestination(tower.position);
            yield return wait;
        }
    }

    void ChaseTick()
    {
        if (!ValidTower()) { tower = null; ChangeState(State.Patrol); return; }

        float d = Vector3.Distance(transform.position, tower.position);
        agent.isStopped = d <= attackRange * 0.9f;

        if (d > forgetDistance)               { tower = null; ChangeState(State.Patrol); }
        else if (d <= attackRange && cooldown <= 0f) ChangeState(State.Cast);
    }

    /*───────────── Cast / Beam ───────────*/
    IEnumerator CastLoop()
    {
        while (true)
        {
            if (!ValidTower()) { tower = null; ChangeState(State.Patrol); yield break; }
            float dist = Vector3.Distance(transform.position, tower.position);
            if (dist > attackRange + 0.5f) { ChangeState(State.Chase); yield break; }

            // 1) wind-up (anim frozen)
            anim.CrossFadeInFixedTime(castHash, .05f);
            anim.speed = 0; rotating = true;
            yield return new WaitForSeconds(windUp);

            // 2) play CastLoop clip, fire mid-way
            anim.speed = 1;
            yield return new WaitForSeconds(fireDelay);
            if (ValidTower() && Vector3.Distance(transform.position, tower.position) <= attackRange + 0.5f)
            {
                FireBeam();
                tower.GetComponent<TowerAI>().TakeDamage(damage);
            }
            yield return new WaitForSeconds(castClipTime - fireDelay);

            // 3) cooldown
            rotating = false; cooldown = cooldownTime;
            while (cooldown > 0)
            {
                if (!ValidTower()) { tower = null; ChangeState(State.Patrol); yield break; }
                dist = Vector3.Distance(transform.position, tower.position);
                if (dist > attackRange + 0.5f) { ChangeState(State.Chase); yield break; }
                cooldown -= Time.deltaTime; yield return null;
            }
        }
    }

    void FireBeam()
    {
        if (!beamPrefab) return;
        Vector3 pos = transform.position + transform.forward * 0.5f;
        Quaternion rot = Quaternion.LookRotation((tower.position - transform.position).normalized);
        PoolManager.Instance.Spawn(beamPoolKey, beamPrefab, pos, rot);
    }

    /*──────────── Utilities / Damage ────────*/
    bool ValidTower() =>
        tower &&
        tower.TryGetComponent<TowerAI>(out var tw) &&
        tw.isActiveAndEnabled;

    void ValidateTower()
    {
        if (!ValidTower()) { tower = null; ChangeState(State.Patrol); }
    }

    void RotateTowards(Transform t)
    {
        if (!rotating || !t) return;
        Vector3 dir = t.position - transform.position; dir.y = 0;
        if (dir.sqrMagnitude > .001f)
            transform.rotation =
                Quaternion.Slerp(transform.rotation,
                                 Quaternion.LookRotation(dir),
                                 8f * Time.deltaTime);
    }

    void StopMove()  { agent.isStopped = true;  anim.SetBool("Moving", false); }
    void StartMove() { agent.isStopped = false; anim.SetBool("Moving", true); anim.CrossFadeInFixedTime(moveHash, .05f); }

    void Die()
    {
        if (deathVFX) Instantiate(deathVFX, transform.position, Quaternion.identity).SetActive(true);
        OnCasterKilled?.Invoke(this);
        Destroy(gameObject, corpseLifeTime);
    }

    public void ChangeState(object newState)
    {
    }

    public void TakeDamage(int dmg, Vector3 hitPos, string src)
    {
        if (state == State.Dead) return;
        currentHealth -= dmg;
        if (currentHealth <= 0) ChangeState(State.Dead);
        else             ApplyStun(.25f);
    }

    public void ApplyStun(float secs)
    {
        if (!canBeStunned || UnityEngine.Random.value < stunResistance || state == State.Dead) return;
        StartCoroutine(StunRoutine(secs));
    }
    IEnumerator StunRoutine(float t)
    {
        State prev = state;
        ChangeState(State.Stunned);
        yield return new WaitForSeconds(t);
        if (state == State.Stunned) ChangeState(prev == State.Cast ? State.Chase : prev);
    }

    public Transform EnemyTransform => transform;

    /*────────────── Gizmos ──────────────*/
    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow; Gizmos.DrawWireSphere(transform.position, detectionRadius);
        Gizmos.color = Color.cyan;   Gizmos.DrawWireSphere(transform.position, attackRange);
        Gizmos.color = new Color(1,1,0,.2f);
        Vector3 l = Quaternion.Euler(0, -fovAngle*.5f, 0)*transform.forward;
        Vector3 r = Quaternion.Euler(0,  fovAngle*.5f, 0)*transform.forward;
        Gizmos.DrawRay(transform.position, l*detectionRadius);
        Gizmos.DrawRay(transform.position, r*detectionRadius);
    }
}
