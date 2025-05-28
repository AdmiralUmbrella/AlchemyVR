using UnityEngine;
using UnityEngine.AI;
using System.Collections;
using System;
using System.Linq;
using System.Text.RegularExpressions;
using UnityEngine.Serialization;

/// <summary>
/// Melee enemy that patrols way-points tagged “Waypoint”, detects towers,
/// stops at attack range and delivers periodic melee hits. Compatible with
/// IEnemy / IStunnable (same interface the Caster uses).
/// </summary>
[RequireComponent(typeof(NavMeshAgent), typeof(Animator))]
public class AdvancedMeleeEnemyAI : MonoBehaviour, IEnemy, IStunnable
{
    enum State
    {
        Idle,
        Patrol,
        Chase,
        Attack,
        Stunned,
        Dead
    }

    /*──────── Inspector ────────*/
    [Header("Navigation")]
    [Tooltip("If left empty, auto-filled with every GameObject tagged “Waypoint”.")]
    [SerializeField] Transform[] waypoints;
    [SerializeField] float waypointTolerance = 0.3f;
    [SerializeField] float pathRefreshRate = 0.25f;
    [Tooltip("Radio aleatorio para dispersar llegadas al waypoint")]
    [SerializeField] float waypointJitter     = 0.5f;

    [Header("Detection")] [SerializeField] float detectionRadius = 12f;
    [SerializeField, Range(0, 180)] float fovAngle = 110f;
    [SerializeField] float forgetDistance = 20f;
    [SerializeField] LayerMask targetMask;

    [Header("Combat")] [SerializeField] float attackRange = 2f;
    [SerializeField] int attackDamage = 15;
    [SerializeField] float attackCooldown = 1.4f;
    [SerializeField] float damageDelay = 0.35f;
    [SerializeField] string attackState = "Attack";
    [SerializeField] string moveState = "Move";

    [Header("Health / VFX")] public int maxHealth = 60;
    [SerializeField] GameObject deathVFX;
    [SerializeField] float corpseLifetime = 4f;

    [Header("Stun")] [SerializeField] bool canBeStunned = true;
    [SerializeField, Range(0, 1)] float stunResistance = 0f;

    /*──────── Runtime ────────*/
    State state;
    NavMeshAgent agent;
    Animator anim;
    int moveHash, attackHash;
    Transform tower;
    int nextWP;
    Coroutine chaseLoop;
    float nextAttack;
    [HideInInspector] public int currentHealth;
    float savedSpeed;
    bool rotating;

    public static event Action<AdvancedMeleeEnemyAI> OnEnemyKilled;

    /*──────── Setup ────────*/
    void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
        anim = GetComponent<Animator>();
        moveHash = Animator.StringToHash(moveState);
        attackHash = Animator.StringToHash(attackState);
        currentHealth = maxHealth;
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

        switch (state)
        {
            case State.Patrol:
                PatrolTick();
                TryDetectTower();
                break;
            case State.Chase: ChaseTick(); break;
            case State.Attack: RotateTowards(tower); break;
        }
    }

    /*──────── FSM ────────*/
    void ChangeState(State s)
    {
        if (state == s) return;

        if (state == State.Chase && chaseLoop != null) StopCoroutine(chaseLoop);
        if (state == State.Stunned) anim.speed = savedSpeed;

        state = s;

        switch (state)
        {
            case State.Idle: StopMove(); break;
            case State.Patrol:
                StartMove();
                MoveToWP();
                break;
            case State.Chase:
                StartMove();
                chaseLoop = StartCoroutine(ChasePath());
                break;
            case State.Attack:
                StopMove();
                StartCoroutine(AttackRoutine());
                break;
            case State.Stunned:
                StopMove();
                anim.speed = 0;
                break;
            case State.Dead: Die(); break;
        }
    }

    /*──────── Navigation ────────*/
    bool HasWaypoints => waypoints != null && waypoints.Length > 0;

    void MoveToWP()
    {
        Vector3 basePos = waypoints[nextWP].position;
        Vector3 jitter  = UnityEngine.Random.insideUnitSphere * waypointJitter;
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

    /*──────── Detection / Chase ────────*/
    void TryDetectTower()
    {
        foreach (var col in Physics.OverlapSphere(transform.position, detectionRadius, targetMask))
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
        var wait = new WaitForSeconds(0.25f);
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
        if (d > forgetDistance)
        {
            tower = null;
            ChangeState(State.Patrol);
        }
        else if (d <= attackRange && Time.time >= nextAttack) ChangeState(State.Attack);
    }

    /*──────── Attack ────────*/
    IEnumerator AttackRoutine()
    {
        if (!ValidTower())
        {
            ChangeState(State.Patrol);
            yield break;
        }

        nextAttack = Time.time + attackCooldown;
        anim.CrossFadeInFixedTime(attackHash, 0.05f);
        rotating = true;

        yield return new WaitForSeconds(damageDelay);

        if (ValidTower() && Vector3.Distance(transform.position, tower.position) <= attackRange + .5f)
            tower.GetComponent<TowerAI>().TakeDamage(attackDamage);

        rotating = false;

        float wait = Mathf.Max(0, nextAttack - Time.time);
        if (wait > 0) yield return new WaitForSeconds(wait);

        if (ValidTower() && Vector3.Distance(transform.position, tower.position) <= attackRange)
            ChangeState(State.Chase);
        else
        {
            tower = null;
            ChangeState(State.Patrol);
        }
    }

    /*──────── Utilities ────────*/
    bool ValidTower() =>
        tower &&
        tower.TryGetComponent<TowerAI>(out var tw) &&
        tw.isActiveAndEnabled;

    void RotateTowards(Transform t)
    {
        if (!rotating || !t) return;
        Vector3 dir = t.position - transform.position;
        dir.y = 0;
        if (dir.sqrMagnitude > .001f)
            transform.rotation =
                Quaternion.Slerp(transform.rotation,
                    Quaternion.LookRotation(dir),
                    8f * Time.deltaTime);
    }

    void StopMove()
    {
        agent.isStopped = true;
        anim.SetBool("Moving", false);
    }

    void StartMove()
    {
        agent.isStopped = false;
        anim.SetBool("Moving", true);
        anim.CrossFadeInFixedTime(moveHash, .05f);
    }

    void Die()
    {
        if (deathVFX) Instantiate(deathVFX, transform.position, Quaternion.identity).SetActive(true);
        OnEnemyKilled?.Invoke(this);
        Destroy(gameObject, corpseLifetime);
    }

    /*──────── Damage / Stun / Interface ────────*/
    public void ChangeState(object newState)
    {
    }

    public void TakeDamage(int dmg, Vector3 hit, string src)
    {
        if (state == State.Dead) return;
        currentHealth -= dmg;
        if (currentHealth <= 0) ChangeState(State.Dead);
        else ApplyStun(.25f);
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
        if (state == State.Stunned) ChangeState(prev == State.Attack ? State.Chase : prev);
    }

    public Transform EnemyTransform => transform;

    /*──────── Gizmos ────────*/
    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, detectionRadius);
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, attackRange);
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(transform.position, forgetDistance);
        Gizmos.color = new Color(1, 1, 0, .2f);
        Vector3 l = Quaternion.Euler(0, -fovAngle * .5f, 0) * transform.forward;
        Vector3 r = Quaternion.Euler(0, fovAngle * .5f, 0) * transform.forward;
        Gizmos.DrawRay(transform.position, l * detectionRadius);
        Gizmos.DrawRay(transform.position, r * detectionRadius);
    }
}

public interface IStunnable
{
    void ApplyStun(float duration);
}