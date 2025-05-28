using UnityEngine;
using UnityEngine.AI;

public class SummonerPatrolState : BaseState<SummonerState>
{
    private SummonerAI manager;
    private SummonerData summonerData;

    private float pathUpdateTimer;

    public SummonerPatrolState(SummonerAI manager, SummonerData data)
        : base(SummonerState.Patrol)
    {
        this.manager = manager;
        this.summonerData = data;
    }

    public override void EnterState()
    {
        Debug.Log("Summoner entró en estado: PATROL");
        manager.ResumeAgent(); // Aseguramos que el NavMeshAgent pueda moverse
        if (summonerData.animator != null)
        {
            summonerData.animator.SetTrigger("Move");
        }

        pathUpdateTimer = 0f;
        UpdateWaypointDestination();
    }

    public override void UpdateState()
    {
        pathUpdateTimer -= Time.deltaTime;
        if (pathUpdateTimer <= 0f)
        {
            pathUpdateTimer = summonerData.pathUpdateInterval;
            UpdateWaypointDestination();
        }

        CheckArrivalToWaypoint();
    }

    public override SummonerState GetNextState()
    {
        // Si hay objetivo en rango, nos vamos a CHASE
        if (manager.CheckForTargetsInRange(summonerData.detectionRange))
        {
            return SummonerState.Chase;
        }

        // Sino, seguir en Patrol
        return SummonerState.Patrol;
    }

    public override void ExitState()
    {
        Debug.Log("Summoner saliendo de estado: PATROL");
        if (summonerData.animator != null)
        {
            summonerData.animator.ResetTrigger("Move");
        }
    }

    public override void OnTriggerEnter(Collider other) { }
    public override void OnTriggerStay(Collider other) { }
    public override void OnTriggerExit(Collider other) { }

    private void UpdateWaypointDestination()
    {
        // Validar si hay waypoints configurados
        if (summonerData.waypoints == null || summonerData.waypoints.Length == 0)
        {
            // Sin waypoints, pasar a Idle
            manager.TransitionToState(SummonerState.Idle);
            return;
        }

        // Asignar destino en NavMeshAgent
        NavMeshAgent navAgent = manager.GetComponent<NavMeshAgent>();
        if (navAgent != null)
        {
            Transform wp = summonerData.waypoints[summonerData.currentWaypointIndex];
            if (wp != null)
            {
                navAgent.SetDestination(wp.position);
            }
        }
    }

    private void CheckArrivalToWaypoint()
    {
        NavMeshAgent navAgent = manager.GetComponent<NavMeshAgent>();
        if (navAgent == null) return;

        Transform wp = summonerData.waypoints[summonerData.currentWaypointIndex];
        if (wp == null) return; // En caso de que haya un hueco en el array.

        float distance = Vector3.Distance(navAgent.transform.position, wp.position);
        if (distance <= summonerData.waypointArriveThreshold)
        {
            summonerData.currentWaypointIndex++;
            if (summonerData.currentWaypointIndex >= summonerData.waypoints.Length)
            {
                summonerData.currentWaypointIndex = 0;
            }
            UpdateWaypointDestination();
        }
    }
}
