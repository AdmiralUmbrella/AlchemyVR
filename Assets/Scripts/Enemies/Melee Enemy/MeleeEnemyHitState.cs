using UnityEngine;

public class MeleeEnemyHitState : BaseState<MeleeEnemyStates>
{
    private MeleeEnemyAI manager;
    private MeleeEnemyData enemyData;

    public MeleeEnemyHitState(MeleeEnemyStates stateKey, MeleeEnemyAI manager, MeleeEnemyData enemyData) : base(stateKey)
    {
        this.manager = manager;
        this.enemyData = enemyData;
    }

    public override void EnterState()
    {
        Debug.Log("Enemigo entr� en estado: HIT (Nuevo)");
        enemyData.isStunned = true;
        enemyData.currentHitStunTime = enemyData.hitStunDuration;
        if (enemyData.agent != null)
        {
            enemyData.agent.isStopped = true;
            enemyData.agent.velocity = Vector3.zero;
        }
        if (enemyData.animator != null)
        {
            enemyData.animator.ResetTrigger("Attack");
            enemyData.animator.SetBool("IsMoving", false);
            enemyData.animator.SetTrigger("Hit");
        }
    }

    public override void ExitState()
    {
        Debug.Log("Enemigo saliendo de estado: HIT (Nuevo)");
        enemyData.isStunned = false;
        if (enemyData.agent != null)
        {
            enemyData.agent.isStopped = false;
        }
    }
    

    public override void UpdateState()
    {
        enemyData.currentHitStunTime -= Time.deltaTime;
        if (enemyData.currentHitStunTime > enemyData.hitStunDuration * 0.5f)
        {
            ApplyKnockback();
        }
    }

    public override MeleeEnemyStates GetNextState()
    {
        if (enemyData.currentHitStunTime <= 0f)
        {
            if (manager.CheckForTargetsInRange(enemyData.detectionRange))
            {
                return MeleeEnemyStates.Chase;
            }
            else
            {
                return MeleeEnemyStates.Idle;
            }
        }
        return MeleeEnemyStates.Hit;
    }

    public override void OnTriggerEnter(Collider other) { }

    public override void OnTriggerStay(Collider other) { }

    public override void OnTriggerExit(Collider other) { }

    private void ApplyKnockback()
    {
        if (enemyData.agent != null && enemyData.knockbackDirection != Vector3.zero)
        {
            Vector3 knockback = enemyData.knockbackDirection * enemyData.knockbackForce;
            enemyData.agent.Move(knockback * Time.deltaTime);
        }
    }
}
