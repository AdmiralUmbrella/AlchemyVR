using UnityEngine;

public class MeleeEnemyChaseState : BaseState<MeleeEnemyStates>
{
    private MeleeEnemyAI manager;
    private MeleeEnemyData enemyData;
    private float pathUpdateTimer;

    public MeleeEnemyChaseState(
        MeleeEnemyStates stateKey,
        MeleeEnemyAI manager,
        MeleeEnemyData enemyData
    ) : base(stateKey)
    {
        this.manager = manager;
        this.enemyData = enemyData;
    }

    public override void EnterState()
    {
        Debug.Log("Enemigo entró en estado: CHASE (Nuevo)");
        if (enemyData.agent != null)
        {
            enemyData.agent.isStopped = false;
        }
        pathUpdateTimer = 0f;
        if (enemyData.animator != null)
        {
            enemyData.animator.ResetTrigger("Attack");
            enemyData.animator.ResetTrigger("Idle");
            enemyData.animator.SetTrigger("Move");
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
        // 1) Si la torre está destruida o la referencia es nula, volver a Patrulla.
        if (enemyData.tower == null)
        {
            return MeleeEnemyStates.Patrol;
        }

        // 2) Verificar si la torre está en rango de ataque:
        if (manager.IsTowerWithinAttackRange(enemyData.attackRange))
        {
            return MeleeEnemyStates.Attack;
        }

        // 3) Si no hay playerTransform o no hay torre, vuelve a Idle (o Patrulla).
        if (enemyData.playerTransform == null)
        {
            return MeleeEnemyStates.Patrol; 
            // O MeleeEnemyStates.Idle, depende de tu diseño.
        }

        // 4) Si el target se alejó demasiado, también volver a Patrulla o Idle.
        float distanceToTarget = Vector3.Distance(
            enemyData.agent.transform.position,
            enemyData.playerTransform.position
        );
        if (distanceToTarget > enemyData.stopChaseDistance)
        {
            Debug.Log("Objetivo muy lejos, volviendo a PATROL (modificado)");
            return MeleeEnemyStates.Patrol;
        }

        // 5) Si estamos en rango para atacar y el cooldown está listo
        if (distanceToTarget <= enemyData.attackRange && enemyData.attackCooldownTimer <= 0)
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
