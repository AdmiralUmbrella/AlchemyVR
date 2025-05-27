using UnityEngine;

public class MeleeEnemyPatrolState : BaseState<MeleeEnemyStates>
{
    private MeleeEnemyAI manager;
    private MeleeEnemyData enemyData;

    public MeleeEnemyPatrolState(
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
        Debug.Log("Entrando en estado PATROL");
        if (enemyData.agent != null)
        {
            enemyData.agent.isStopped = false;
            enemyData.agent.speed = enemyData.moveSpeed;
        }
        if (enemyData.animator != null)
        {
            enemyData.animator.ResetTrigger("Attack");
            enemyData.animator.ResetTrigger("Idle");
            enemyData.animator.SetTrigger("Move");
        }
        // Fijamos el primer destino
        SetNextWaypointDestination();
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
        // ¡No hacemos más chequeos de distancia aquí!
        // El avance ocurre en OnTriggerEnter
    }

    public override MeleeEnemyStates GetNextState()
    {
        // Si detecta torre/jugador, cambia a Chase
        if (manager.CheckForTargetsInRange(enemyData.detectionRange))
        {
            return MeleeEnemyStates.Chase;
        }
        return MeleeEnemyStates.Patrol;
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
            manager.TransitionToState(MeleeEnemyStates.Idle);
            return;
        }
        Transform wp = enemyData.waypoints[enemyData.currentWaypointIndex];
        if (wp != null && enemyData.agent != null)
        {
            enemyData.agent.SetDestination(wp.position);
        }
    }
}
