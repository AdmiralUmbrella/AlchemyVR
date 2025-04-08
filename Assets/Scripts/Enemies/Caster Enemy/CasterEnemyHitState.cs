using UnityEngine;

/// <summary>
/// Estado Hit del CasterEnemy. El enemigo queda aturdido al recibir daño y, una vez terminado el tiempo de stun, vuelve a Chase o Idle.
/// </summary>
public class CasterEnemyHitState : BaseState<CasterEnemyState>
{
    private CasterEnemyAI manager;
    private CasterEnemyData enemyData;

    /// <summary>
    /// Constructor del estado Hit.
    /// </summary>
    public CasterEnemyHitState(CasterEnemyState stateKey, CasterEnemyAI manager, CasterEnemyData enemyData) : base(stateKey)
    {
        this.manager = manager;
        this.enemyData = enemyData;
    }

    public override void EnterState()
    {
        Debug.Log("CasterEnemy entró en estado: HIT");
        enemyData.isStunned = true;
        enemyData.currentHitStunTime = enemyData.hitStunDuration;
        if (enemyData.agent != null)
        {
            enemyData.agent.isStopped = true;
            enemyData.agent.velocity = Vector3.zero;
        }
        if (enemyData.animator != null)
        {
            enemyData.animator.SetTrigger("Hit");
        }
    }

    public override void UpdateState()
    {
        enemyData.currentHitStunTime -= Time.deltaTime;
        // Aquí podrías aplicar knockback si lo deseas usando enemyData.knockbackDirection
    }

    public override CasterEnemyState GetNextState()
    {
        if (enemyData.currentHitStunTime <= 0f)
        {
            if (manager.CheckForTargetsInRange(enemyData.detectionRange))
                return CasterEnemyState.Chase;
            return CasterEnemyState.Idle;
        }
        return CasterEnemyState.Hit;
    }

    public override void ExitState()
    {
        Debug.Log("CasterEnemy saliendo de estado: HIT");
        enemyData.isStunned = false;
        if (enemyData.agent != null)
        {
            enemyData.agent.isStopped = false;
        }
    }

    public override void OnTriggerEnter(Collider other) { }
    public override void OnTriggerStay(Collider other) { }
    public override void OnTriggerExit(Collider other) { }
}
