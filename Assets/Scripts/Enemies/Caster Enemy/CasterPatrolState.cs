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
            enemyData.animator.SetBool("IsMoving", true);
        }

        pathUpdateTimer = 0f;
        UpdateWaypointDestination(); 
    }

    public override void UpdateState()
    {
        pathUpdateTimer -= Time.deltaTime;
        if (pathUpdateTimer <= 0f)
        {
            pathUpdateTimer = enemyData.pathUpdateInterval;
            UpdateWaypointDestination();
        }

        CheckArrivalToWaypoint();
    }

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

    public override void OnTriggerEnter(Collider other) { }
    public override void OnTriggerStay(Collider other) { }
    public override void OnTriggerExit(Collider other) { }

    private void UpdateWaypointDestination()
    {
        // Validar si hay waypoints
        if (enemyData.waypoints == null || enemyData.waypoints.Length == 0)
        {
            // Sin waypoints, quedarnos en Idle
            manager.TransitionToState(CasterEnemyState.Idle);
            return;
        }

        Transform currentWaypoint = enemyData.waypoints[enemyData.currentWaypointIndex];
        if (currentWaypoint != null && enemyData.agent != null)
        {
            enemyData.agent.SetDestination(currentWaypoint.position);
        }
    }

    private void CheckArrivalToWaypoint()
    {
        if (enemyData.agent == null) return;

        // Distancia al waypoint actual
        Transform wp = enemyData.waypoints[enemyData.currentWaypointIndex];
        float distance = Vector3.Distance(enemyData.agent.transform.position, wp.position);

        if (distance <= enemyData.waypointArriveThreshold)
        {
            // Mover al siguiente waypoint (o volver al 0 si ya estamos al final)
            enemyData.currentWaypointIndex++;
            if (enemyData.currentWaypointIndex >= enemyData.waypoints.Length)
            {
                enemyData.agent.isStopped = true;
            }
            UpdateWaypointDestination();
        }
    }
}
