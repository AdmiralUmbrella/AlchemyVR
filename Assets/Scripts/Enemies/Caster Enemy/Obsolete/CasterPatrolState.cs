using UnityEngine;

public class CasterEnemyPatrolState : BaseState<CasterEnemyState>
{
    private CasterEnemyAI manager;
    private CasterEnemyData enemyData;

    private float pathUpdateTimer;

    public CasterEnemyPatrolState(
        CasterEnemyState stateKey,
        CasterEnemyAI manager,
        CasterEnemyData enemyData
    ) : base(stateKey)
    {
        this.manager = manager;
        this.enemyData = enemyData;
    }

    public override void EnterState()
    {
        Debug.Log("CasterEnemy entró en estado: PATROL");
        if (enemyData.agent != null)
        {
            enemyData.agent.isStopped = false;
            enemyData.agent.speed = enemyData.moveSpeed;
        }
        if (enemyData.animator != null)
        {
            enemyData.animator.SetTrigger("Move");
        }

        pathUpdateTimer = 0f;
        SetNextWaypointDestination();
    }

    public override void UpdateState()
    { }

    public override CasterEnemyState GetNextState()
    {
        // Si detectamos un objetivo (torre), vamos a CHASE
        if (manager.CheckForTargetsInRange(enemyData.detectionRange))
        {
            return CasterEnemyState.Chase;
        }

        // Si no hay objetivo en rango, quedarnos en Patrol
        return CasterEnemyState.Patrol;
    }

    public override void ExitState()
    {
        Debug.Log("CasterEnemy saliendo de estado: PATROL");
        if (enemyData.animator != null)
        {
            enemyData.animator.SetBool("IsMoving", false);
        }
    }

    public override void OnTriggerEnter(Collider other)
    {
        // Detectamos sólo triggers de waypoints
        if (!other.CompareTag("Waypoint")) return;

        // Comprobamos que sea el waypoint actual
        Transform target = enemyData.waypoints[enemyData.currentWaypointIndex];
        if (other.transform == target)
        {
            // Avanzamos índice (y reseteamos al llegar al final, o detenemos)
            enemyData.currentWaypointIndex++;
            if (enemyData.currentWaypointIndex >= enemyData.waypoints.Length)
            {
                // Opcional: reiniciar o parar
                enemyData.currentWaypointIndex = 0;
            }
            SetNextWaypointDestination();
        }
    }
    public override void OnTriggerStay(Collider other) { }
    public override void OnTriggerExit(Collider other) { }



    private void SetNextWaypointDestination()
    {
        if (enemyData.waypoints == null || enemyData.waypoints.Length == 0)
        {
            manager.TransitionToState(CasterEnemyState.Idle);
            return;
        }
        Transform wp = enemyData.waypoints[enemyData.currentWaypointIndex];
        if (wp != null && enemyData.agent != null)
        {
            enemyData.agent.SetDestination(wp.position);
        }
    }
}
