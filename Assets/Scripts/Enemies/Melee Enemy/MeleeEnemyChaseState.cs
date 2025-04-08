using UnityEngine;

public class MeleeEnemyChaseState : BaseState<MeleeEnemyStates>
{
    private MeleeEnemyAI manager;
    private MeleeEnemyData enemyData;
    private float pathUpdateTimer;

    public MeleeEnemyChaseState(MeleeEnemyStates stateKey, MeleeEnemyAI manager, MeleeEnemyData enemyData) : base(stateKey)
    {
        this.manager = manager;
        this.enemyData = enemyData;
    }

    public override void EnterState()
    {
        Debug.Log("Enemigo entr� en estado: CHASE (Nuevo)");
        // Reanudar el movimiento
        if (enemyData.agent != null)
        {
            enemyData.agent.isStopped = false;
        }
        pathUpdateTimer = 0f;
        if (enemyData.animator != null)
        {
            enemyData.animator.SetBool("IsMoving", true);
        }
        UpdateChaseTarget();
    }

    public override void ExitState()
    {
        Debug.Log("Enemigo saliendo de estado: CHASE (Nuevo)");
        if (enemyData.animator != null)
        {
            enemyData.animator.SetBool("IsMoving", false);
        }
    }

    public override void UpdateState()
    {
        pathUpdateTimer -= Time.deltaTime;
        if (pathUpdateTimer <= 0f)
        {
            pathUpdateTimer = enemyData.pathUpdateInterval;
            UpdateChaseTarget();
        }
    }

    public override MeleeEnemyStates GetNextState()
    {
        if (enemyData.playerTransform == null)
        {
            return MeleeEnemyStates.Idle;
        }

        float distanceToPlayer = Vector3.Distance(
            enemyData.agent.transform.position,
            enemyData.playerTransform.position
        );

        if (distanceToPlayer > enemyData.stopChaseDistance)
        {
            Debug.Log("Objetivo muy lejos, volviendo a IDLE (Nuevo)");
            return MeleeEnemyStates.Idle;
        }

        if (distanceToPlayer <= enemyData.attackRange && enemyData.attackCooldownTimer <= 0)
        {
            return MeleeEnemyStates.Attack;
        }

        return MeleeEnemyStates.Chase;
    }

    public override void OnTriggerEnter(Collider other) { }

    public override void OnTriggerStay(Collider other) { }

    public override void OnTriggerExit(Collider other) { }

    private void UpdateChaseTarget()
    {
        if (enemyData.agent != null && enemyData.playerTransform != null)
        {
            enemyData.agent.SetDestination(enemyData.playerTransform.position);
            Debug.DrawLine(
                enemyData.agent.transform.position,
                enemyData.playerTransform.position,
                Color.red,
                enemyData.pathUpdateInterval
            );
        }
    }
}
