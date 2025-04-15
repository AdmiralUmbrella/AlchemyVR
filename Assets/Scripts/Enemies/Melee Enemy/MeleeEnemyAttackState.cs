using UnityEngine;

public class MeleeEnemyAttackState : BaseState<MeleeEnemyStates>
{
    private MeleeEnemyAI manager;
    private MeleeEnemyData enemyData;

    public MeleeEnemyAttackState(
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
            enemyData.animator.ResetTrigger("Hit");
            enemyData.animator.SetBool("IsMoving", false);
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

        // En algún punto de la animación, se aplica daño (una sola vez).
        if (
            !enemyData.hasDealtDamage &&
            enemyData.currentAttackTime <= (enemyData.attackDuration - enemyData.damageDelay)
        )
        {
            DealDamage();
            enemyData.hasDealtDamage = true;
        }
    }

    public override MeleeEnemyStates GetNextState()
    {
        // 1) Si la torre está destruida (o la referencia es nula), volver a Patrulla.
        if (enemyData.tower == null)
        {
            return MeleeEnemyStates.Patrol;
        }

        // 2) Finaliza la animación de ataque
        if (enemyData.currentAttackTime <= 0f)
        {
            float distanceToPlayer = Vector3.Distance(
                enemyData.agent.transform.position,
                enemyData.playerTransform.position
            );
            // Si sigue en rango y el cooldown está listo, ataca de nuevo.
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

        // Mientras no se cumpla el tiempo de ataque, permanecer en Attack.
        return MeleeEnemyStates.Attack;
    }

    public override void OnTriggerEnter(Collider other) { }
    public override void OnTriggerStay(Collider other) { }
    public override void OnTriggerExit(Collider other) { }

    private void LookAtTarget()
    {
        if (enemyData.playerTransform == null) return;

        Vector3 directionToPlayer = enemyData.playerTransform.position -
                                    enemyData.agent.transform.position;
        directionToPlayer.y = 0;
        enemyData.agent.transform.rotation = Quaternion.LookRotation(directionToPlayer);
    }

    private void DealDamage()
    {
        if (enemyData.playerTransform == null) return;

        // Se busca la torre con OverlapSphere para confirmar que está en rango
        Collider[] hits = Physics.OverlapSphere(
            enemyData.agent.transform.position,
            enemyData.attackRange
        );
        foreach (Collider col in hits)
        {
            if (col.CompareTag("Tower"))
            {
                enemyData.tower.TakeDamage(enemyData.attackDamage);
                Debug.Log("Daño aplicado al objetivo (Torre).");
                return;
            }
        }
    }
}
