using UnityEngine;
using UnityEngine.AI;

/// <summary>
/// Estado Chase del CasterEnemy. El enemigo persigue al objetivo y cambia al estado Casting cuando está dentro del rango de ataque.
/// </summary>
public class CasterEnemyChaseState : BaseState<CasterEnemyState>
{
    private CasterEnemyAI manager;
    private CasterEnemyData enemyData;
    private float pathUpdateTimer;

    /// <summary>
    /// Constructor del estado Chase.
    /// </summary>
    public CasterEnemyChaseState(CasterEnemyState stateKey, CasterEnemyAI manager, CasterEnemyData enemyData) : base(stateKey)
    {
        this.manager = manager;
        this.enemyData = enemyData;
    }

    public override void EnterState()
    {
        Debug.Log("CasterEnemy entró en estado: CHASE");
        if (enemyData.agent != null)
        {
            enemyData.agent.isStopped = false;
        }
        if (enemyData.animator != null)
        {
            enemyData.animator.SetBool("IsMoving", true);
        }
        pathUpdateTimer = 0f;
        UpdateChaseTarget();
    }

    public override void UpdateState()
    {
        pathUpdateTimer -= Time.deltaTime;
        if (pathUpdateTimer <= 0f)
        {
            pathUpdateTimer = 0.2f; // Actualización frecuente de la posición
            UpdateChaseTarget();
        }
    }

    public override CasterEnemyState GetNextState()
    {
        if (enemyData.targetTransform == null)
        {
            // Objetivo perdió referencia → Patrulla
            return CasterEnemyState.Patrol;
        }

        float distanceToTarget = Vector3.Distance(
            manager.transform.position,
            enemyData.targetTransform.position
        );

        // Si está en rango, a Casting
        if (distanceToTarget <= enemyData.attackRange)
        {
            return CasterEnemyState.Casting;
        }

        // Si se alejó demasiado (puedes configurar un stopChaseDistance):
        if (distanceToTarget > enemyData.detectionRange * 1.5f)
        {
            // Volver a patrullar
            return CasterEnemyState.Patrol;
        }

        return CasterEnemyState.Chase;
    }

    public override void ExitState()
    {
        Debug.Log("CasterEnemy saliendo de estado: CHASE");
        if (enemyData.animator != null)
        {
            enemyData.animator.SetBool("IsMoving", false);
        }
    }

    public override void OnTriggerEnter(Collider other) { }
    public override void OnTriggerStay(Collider other) { }
    public override void OnTriggerExit(Collider other) { }

    /// <summary>
    /// Actualiza la posición de destino del NavMeshAgent según el objetivo actual.
    /// </summary>
    private void UpdateChaseTarget()
    {
        if (enemyData.agent != null && enemyData.targetTransform != null)
        {
            enemyData.agent.SetDestination(enemyData.targetTransform.position);
        }
    }
}
