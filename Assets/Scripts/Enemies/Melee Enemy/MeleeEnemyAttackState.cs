using UnityEngine;

public class MeleeEnemyAttackState : BaseState<MeleeEnemyStates>
{
    private MeleeEnemyAI manager;
    private MeleeEnemyData enemyData;
    private TowerAI tower;

    public MeleeEnemyAttackState(MeleeEnemyStates stateKey, MeleeEnemyAI manager, MeleeEnemyData enemyData,
        TowerAI tower) : base(stateKey)
    {
        this.manager = manager;
        this.enemyData = enemyData;
        this.tower = tower;
    }

    public override void EnterState()
    {
        Debug.Log("Enemigo entró en estado: ATTACK (Nuevo)");
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
            // Se asume que si se termina el ataque, se reevalúa la distancia mediante la lógica del Chase.
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

        // Usamos OverlapSphere para confirmar que el collider de la torre está en el rango de ataque.
        Collider[] hits = Physics.OverlapSphere(enemyData.agent.transform.position, enemyData.attackRange);
        foreach (Collider col in hits)
        {
            if (col.CompareTag("Tower"))
            {
                tower.TakeDamage(enemyData.attackDamage);
                Debug.Log("Daño aplicado al objetivo.");
                return;
            }
        }
    }
}
