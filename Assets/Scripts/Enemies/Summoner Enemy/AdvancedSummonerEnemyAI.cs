using UnityEngine;
using UnityEngine.AI;
using System.Collections;
using System;
using System.Linq;
using System.Text.RegularExpressions;
using System.Collections.Generic;
using UnityEngine.Serialization;

/// <summary>
/// Summoner enemy: patrols way-points (tag “Waypoint”), detects towers, chases
/// to summon range and spawns minions.  When the hard cap of minions is
/// reached it idles until one dies, then resumes summoning to refill the cap.
/// Compatible with IEnemy / IStunnable.
/// </summary>
[RequireComponent(typeof(NavMeshAgent), typeof(Animator))]
public class AdvancedSummonerEnemyAI : MonoBehaviour, IEnemy, IStunnable
{
    enum State { Idle, Patrol, Chase, Summon, Stunned, Dead }

    [Header("Navigation")]
    [Tooltip("If empty, auto-filled with every GameObject tagged “Waypoint”")]
    [SerializeField] Transform[] waypoints;
    [SerializeField] float waypointTolerance = 0.3f;
    [SerializeField] float pathRefreshRate   = 0.25f;
    [Tooltip("Cuánto puede desviarse aleatoriamente cada agente al patrullar")]
    [SerializeField] float waypointJitter     = 0.5f;

    [Header("Detection")]
    [SerializeField] float detectionRadius = 15f;
    [SerializeField, Range(0,180)] float fovAngle = 90f;
    [SerializeField] float forgetDistance = 25f;
    [SerializeField] LayerMask towerMask;

    [Header("Summon")]
    [SerializeField] float summonRange      = 8f;
    [SerializeField] GameObject minionPrefab;
    [SerializeField] int   maxMinions       = 3;
    [SerializeField] float summonCooldown   = 5f;
    [SerializeField] float summonWindUp     = 1f;
    [SerializeField] float summonClipTime   = 2f;
    [SerializeField] float summonFireDelay  = 1f;
    [SerializeField] float spawnOffset      = 1.5f;

    [Header("Animator")]
    [SerializeField] string moveState   = "Move";
    [SerializeField] string summonState = "SummonLoop";

    [Header("Health / VFX")]
    public int   maxHealth      = 50;
    [SerializeField] GameObject deathVFX;
    [SerializeField] float corpseLifetime = 4f;

    [Header("Stun")]
    [SerializeField] bool  canBeStunned   = true;
    [SerializeField, Range(0,1)] float stunResistance = 0f;

    State state;
    NavMeshAgent agent;
    Animator anim;
    int moveHash, summonHash;
    Transform tower;
    int nextWP;
    Coroutine chaseLoop;
    float cooldown;
    float idleCheckTimer;
    [HideInInspector] public   int currentHealth;
    float savedSpeed;
    bool  rotating;

    readonly List<GameObject> minions = new();

    public static event Action<AdvancedSummonerEnemyAI> OnSummonerKilled;

    void Awake()
    {
        agent      = GetComponent<NavMeshAgent>();
        anim       = GetComponent<Animator>();
        moveHash   = Animator.StringToHash(moveState);
        summonHash = Animator.StringToHash(summonState);
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
            .OrderBy(o => {
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

        CleanMinionList();
        cooldown = Mathf.Max(0, cooldown - Time.deltaTime);

        switch (state)
        {
            case State.Idle:
                idleCheckTimer -= Time.deltaTime;
                if (idleCheckTimer <= 0f)
                {
                    idleCheckTimer = .25f;
                    if (ValidTower() &&
                        Vector3.Distance(transform.position, tower.position) <= summonRange &&
                        minions.Count < maxMinions &&
                        cooldown <= 0f)
                    {
                        ChangeState(State.Summon);
                        return;
                    }
                }
                break;

            case State.Patrol:
                PatrolTick();
                TryDetectTower();
                break;

            case State.Chase:
                ChaseTick();
                break;

            case State.Summon:
                RotateTowards(tower);
                ValidateTower();
                break;
        }
    }

    void ChangeState(State s)
    {
        if (state == s) return;
        if (state == State.Chase && chaseLoop != null) StopCoroutine(chaseLoop);
        if (state == State.Stunned) anim.speed = savedSpeed;
        state = s;
        switch (s)
        {
            case State.Idle:
                StopMove();
                break;
            case State.Patrol:
                StartMove();
                MoveToWP();
                break;
            case State.Chase:
                StartMove();
                chaseLoop = StartCoroutine(ChasePath());
                break;
            case State.Summon:
                StopMove();
                StartCoroutine(SummonLoop());
                break;
            case State.Stunned:
                StopMove();
                anim.speed = 0;
                break;
            case State.Dead:
                Die();
                break;
        }
    }

    bool HasWaypoints => waypoints != null && waypoints.Length > 0;
    
    void MoveToWP()
    {
        Vector3 target  = waypoints[nextWP].position;
        Vector3 jitter  = UnityEngine.Random.insideUnitSphere * waypointJitter;
        jitter.y = 0f;
        agent.SetDestination(target + jitter);
    }

    void PatrolTick()
    {
        if (!agent.pathPending && agent.remainingDistance <= waypointTolerance)
        {
            nextWP = (nextWP + 1) % waypoints.Length;
            MoveToWP();
        }
    }

    void TryDetectTower()
    {
        foreach (var col in Physics.OverlapSphere(transform.position, detectionRadius, towerMask))
        {
            if (!col.TryGetComponent<TowerAI>(out var tw)) continue;
            var dir = (tw.transform.position - transform.position).normalized;
            if (Vector3.Angle(transform.forward, dir) <= fovAngle * 0.5f)
            {
                tower = tw.transform;
                ChangeState(State.Chase);
                return;
            }
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
        if (!ValidTower())
        {
            tower = null;
            ChangeState(State.Patrol);
            return;
        }
        float d = Vector3.Distance(transform.position, tower.position);
        agent.isStopped = d <= summonRange * 0.9f;
        if (d > forgetDistance)
        {
            tower = null;
            ChangeState(State.Patrol);
        }
        else if (d <= summonRange && cooldown <= 0f && minions.Count < maxMinions)
        {
            ChangeState(State.Summon);
        }
    }

    IEnumerator SummonLoop()
    {
        while (true)
        {
            if (!ValidTower())
            {
                tower = null;
                ChangeState(State.Patrol);
                yield break;
            }

            float dist = Vector3.Distance(transform.position, tower.position);
            if (dist > summonRange + 0.5f)
            {
                ChangeState(State.Chase);
                yield break;
            }

            // wind-up
            anim.CrossFadeInFixedTime(summonHash, .05f);
            anim.speed = 0;
            rotating = true;
            yield return new WaitForSeconds(summonWindUp);

            // play Summon clip & delay
            anim.speed = 1;
            yield return new WaitForSeconds(summonFireDelay);

            if (minions.Count < maxMinions && cooldown <= 0f && ValidTower())
            {
                SpawnMinion();
                cooldown = summonCooldown;
            }

            yield return new WaitForSeconds(summonClipTime - summonFireDelay);
            rotating = false;

            // cap reached → idle
            if (minions.Count >= maxMinions)
            {
                ChangeState(State.Idle);
                yield break;
            }

            // cooldown loop
            while (cooldown > 0f)
            {
                if (!ValidTower())
                {
                    tower = null;
                    ChangeState(State.Patrol);
                    yield break;
                }
                dist = Vector3.Distance(transform.position, tower.position);
                if (dist > summonRange + 0.5f)
                {
                    ChangeState(State.Chase);
                    yield break;
                }
                cooldown -= Time.deltaTime;
                yield return null;
            }
        }
    }

    void SpawnMinion()
    {
        if (!minionPrefab) return;
        Vector3 pos = transform.position + transform.forward * spawnOffset;
        Quaternion rot = Quaternion.LookRotation((tower.position - transform.position).normalized);
        var go = Instantiate(minionPrefab, pos, rot);
        minions.Add(go);
    }

    void CleanMinionList()
    {
        for (int i = minions.Count - 1; i >= 0; i--)
            if (minions[i] == null)
                minions.RemoveAt(i);
    }

    bool ValidTower() =>
        tower &&
        tower.TryGetComponent<TowerAI>(out var tw) &&
        tw.isActiveAndEnabled;

    void ValidateTower()
    {
        if (!ValidTower())
        {
            tower = null;
            ChangeState(State.Patrol);
        }
    }

    void RotateTowards(Transform t)
    {
        if (!rotating || !t) return;
        Vector3 dir = t.position - transform.position; dir.y = 0;
        if (dir.sqrMagnitude > .001f)
            transform.rotation = Quaternion.Slerp(
                transform.rotation,
                Quaternion.LookRotation(dir),
                8f * Time.deltaTime
            );
    }

    void StopMove()  { agent.isStopped = true;  anim.SetBool("Moving", false); }
    void StartMove(){ agent.isStopped = false; anim.SetBool("Moving", true); anim.CrossFadeInFixedTime(moveHash, .05f); }

    void Die()
    {
        foreach (var m in minions)
            if (m) Destroy(m);

        if (deathVFX)
            Instantiate(deathVFX, transform.position, Quaternion.identity).SetActive(true);

        OnSummonerKilled?.Invoke(this);
        Destroy(gameObject, corpseLifetime);
    }

    public void ChangeState(object newState)
    {
    }

    public void TakeDamage(int dmg, Vector3 hit, string src)
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
        if (state == State.Stunned)
            ChangeState(prev == State.Summon ? State.Chase : prev);
    }

    public Transform EnemyTransform => transform;

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow; Gizmos.DrawWireSphere(transform.position, detectionRadius);
        Gizmos.color = Color.magenta; Gizmos.DrawWireSphere(transform.position, summonRange);
        Gizmos.color = new Color(1,1,0,.2f);
        Vector3 l = Quaternion.Euler(0, -fovAngle*.5f, 0)*transform.forward;
        Vector3 r = Quaternion.Euler(0,  fovAngle*.5f, 0)*transform.forward;
        Gizmos.DrawRay(transform.position, l*detectionRadius);
        Gizmos.DrawRay(transform.position, r*detectionRadius);
    }
}
