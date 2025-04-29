using UnityEngine;

/// <summary>
/// Estado Idle del CasterEnemy. El enemigo permanece quieto y revisa periódicamente si hay objetivos en rango.
/// </summary>
public class CasterEnemyIdleState : BaseState<CasterEnemyState>
{
    private CasterEnemyAI manager;
    private CasterEnemyData enemyData;
    private float checkTimer;

    /// <summary>
    /// Constructor del estado Idle.
    /// </summary>
    public CasterEnemyIdleState(CasterEnemyState stateKey, CasterEnemyAI manager, CasterEnemyData enemyData) : base(stateKey)
    {
        this.manager = manager;
        this.enemyData = enemyData;
    }

    public override void EnterState()
    {
        Debug.Log("CasterEnemy entró en estado: IDLE");
        if (enemyData.agent != null)
        {
            enemyData.agent.isStopped = true;
            enemyData.agent.velocity = Vector3.zero;
        }
        if (enemyData.animator != null)
        {
            enemyData.animator.SetTrigger("Idle");
        }
        checkTimer = 0f;
    }

    public override void UpdateState()
    {
        checkTimer -= Time.deltaTime;
        if (checkTimer <= 0f)
        {
            // Revisar cada 0.5 segundos
            checkTimer = 0.5f;
        }
    }

    public override CasterEnemyState GetNextState()
    {
        // Si detecta torre:
        if (manager.CheckForTargetsInRange(enemyData.detectionRange))
        {
            return CasterEnemyState.Chase;
        }

        // Si NO detecta torre y hay waypoints, ve a Patrol
        if (enemyData.waypoints != null && enemyData.waypoints.Length > 0)
        {
            return CasterEnemyState.Patrol;
        }

        // Si no hay waypoints, permanecer en Idle
        return CasterEnemyState.Idle;
    }

    public override void ExitState()
    {
        Debug.Log("CasterEnemy saliendo de estado: IDLE");
    }

    public override void OnTriggerEnter(Collider other) { }
    public override void OnTriggerStay(Collider other) { }
    public override void OnTriggerExit(Collider other) { }
}