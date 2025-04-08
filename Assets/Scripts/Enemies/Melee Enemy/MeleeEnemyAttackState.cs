using UnityEngine;

public class MeleeEnemyAttackState : BaseState<MeleeEnemyStates>
{
    private MeleeEnemyAI manager;
    private MeleeEnemyData enemyData;

    public MeleeEnemyAttackState(MeleeEnemyStates stateKey, MeleeEnemyAI manager, MeleeEnemyData enemyData) : base(stateKey)
    {
        this.manager = manager;
        this.enemyData = enemyData;
    }

    public override void EnterState()
    {
        Debug.Log("Enemigo entr� en estado: ATTACK (Nuevo)");
        // Detener al agente durante el ataque
        if (enemyData.agent != null)
        {
            enemyData.agent.isStopped = true;
        }
        enemyData.currentAttackTime = enemyData.attackDuration;
        enemyData.hasDealtDamage = false;
        LookAtTarget();
        if (enemyData.animator != null)
        {
            enemyData.animator.SetTrigger("Attack");
        }
    }

    public override void ExitState()
    {
        Debug.Log("Enemigo saliendo de estado: ATTACK (Nuevo)");
        enemyData.attackCooldownTimer = enemyData.attackCooldown;
    }

    public override void UpdateState()
    {
        enemyData.currentAttackTime -= Time.deltaTime;
        if (!enemyData.hasDealtDamage &&
            enemyData.currentAttackTime <= (enemyData.attackDuration - enemyData.damageDelay))
        {
            DealDamage();
            enemyData.hasDealtDamage = true;
        }
    }

    public override MeleeEnemyStates GetNextState()
    {
        if (enemyData.currentAttackTime <= 0f)
        {
            float distanceToPlayer = Vector3.Distance(
                enemyData.agent.transform.position,
                enemyData.playerTransform.position
            );
            if (distanceToPlayer <= enemyData.attackRange)
            {
                if (enemyData.attackCooldownTimer <= 0)
                {
                    return MeleeEnemyStates.Attack;
                }
                else
                {
                    return MeleeEnemyStates.Chase;
                }
            }
            else
            {
                return MeleeEnemyStates.Chase;
            }
        }
        return MeleeEnemyStates.Attack;
    }

    public override void OnTriggerEnter(Collider other) { }

    public override void OnTriggerStay(Collider other) { }

    public override void OnTriggerExit(Collider other) { }

    private void LookAtTarget()
    {
        if (enemyData.playerTransform == null) return;

        Vector3 directionToPlayer = enemyData.playerTransform.position - enemyData.agent.transform.position;
        directionToPlayer.y = 0;
        enemyData.agent.transform.rotation = Quaternion.LookRotation(directionToPlayer);
    }

    private void DealDamage()
    {
        if (enemyData.playerTransform == null) return;

        if (Vector3.Distance(
                enemyData.agent.transform.position,
                enemyData.playerTransform.position
            ) <= enemyData.attackRange)
        {
            // Aqu� aplicar�as el da�o al jugador o torre
            Debug.Log("Da�o aplicado al objetivo.");
        }
    }
}
