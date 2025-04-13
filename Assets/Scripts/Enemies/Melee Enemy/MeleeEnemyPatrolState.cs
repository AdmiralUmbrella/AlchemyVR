using UnityEngine;

public class MeleeEnemyPatrolState : BaseState<MeleeEnemyStates>
{
    private MeleeEnemyAI manager;
    private MeleeEnemyData enemyData;

    public MeleeEnemyPatrolState(MeleeEnemyStates stateKey, MeleeEnemyAI manager, MeleeEnemyData enemyData)
        : base(stateKey)
    {
        this.manager = manager;
        this.enemyData = enemyData;
    }

    public override void EnterState()
    {
        Debug.Log("Entrando en estado PATROL");
        
        if (enemyData.agent != null)
        {
            enemyData.agent.isStopped = false;
            enemyData.agent.speed = enemyData.moveSpeed;
        }

        // Configurar animación
        if (enemyData.animator != null)
        {
            enemyData.animator.SetBool("IsMoving", true);
        }
        
        // Actualizar destino del waypoint
        UpdateWaypointDestination();
    }

    public override void ExitState()
    {
        Debug.Log("Saliendo de estado PATROL");
        if (enemyData.animator != null)
        {
            enemyData.animator.SetBool("IsMoving", false);
        }
    }

    public override void UpdateState()
    {
        UpdateWaypointDestination();
    }

    public override MeleeEnemyStates GetNextState()
    {
        // 1) Si el enemigo detecta una torre en rango, desviarse:
        if (manager.CheckForTargetsInRange(enemyData.detectionRange))
        {
            // Se transiciona a CHASE (y luego ATTACK).
            return MeleeEnemyStates.Chase;
        }

        // 2) Si no hay torre, seguir en Patrol
        return MeleeEnemyStates.Patrol;
    }

    public override void OnTriggerEnter(Collider other) { }
    public override void OnTriggerStay(Collider other) { }
    public override void OnTriggerExit(Collider other) { }

    private void UpdateWaypointDestination()
    {
        // Si no hay waypoints configurados, caer de nuevo a Idle, por ejemplo
        if (enemyData.waypoints == null || enemyData.waypoints.Length == 0)
        {
            manager.TransitionToState(MeleeEnemyStates.Idle);
            return;
        }
        
        CheckArrivalToWaypoint();
        
        Transform targetWaypoint = enemyData.waypoints[enemyData.currentWaypointIndex];
        if (targetWaypoint != null && enemyData.agent != null)
        {
            enemyData.agent.SetDestination(targetWaypoint.position);
        }
    }

    private void CheckArrivalToWaypoint()
    {
        //if (enemyData.agent == null) return;

        float distance = Vector3.Distance(
            enemyData.agent.transform.position,
            enemyData.waypoints[enemyData.currentWaypointIndex].position
        );

        if (distance <= enemyData.waypointArriveThreshold)
        {
            // Pasar al siguiente waypoint
            enemyData.currentWaypointIndex++;
            if (enemyData.currentWaypointIndex >= enemyData.waypoints.Length)
            {
                enemyData.agent.isStopped = true;
            }
        }
    }
}
