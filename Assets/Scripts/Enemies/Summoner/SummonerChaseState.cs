using UnityEngine;
using UnityEngine.AI;

/// <summary>
/// Estado de persecución. El Summoner se mueve hasta estar en rango de invocar.
/// Igual que EnemyChaseState.
/// </summary>
public class SummonerChaseState : SummonerBaseState
{
    private float pathUpdateTimer;

    public SummonerChaseState(SummonerStateManager manager, SummonerData data)
        : base(manager, data) { }

    public override void Enter()
    {
        Debug.Log("Summoner entró en estado: CHASE");

        manager.ResumeAgent();

        pathUpdateTimer = 0f;

        if (summonerData.animator != null)
        {
            summonerData.animator.SetBool("IsMoving", true);
        }

        UpdateChasePath();
    }

    public override void Update()
    {
        if (summonerData.targetTransform == null || summonerData.isDead)
        {
            manager.ChangeState(manager.IdleState);
            return;
        }

        pathUpdateTimer -= Time.deltaTime;
        if (pathUpdateTimer <= 0f)
        {
            pathUpdateTimer = summonerData.pathUpdateInterval;
            UpdateChasePath();
        }

        float dist = Vector3.Distance(manager.transform.position,
            summonerData.targetTransform.position);

        // 1) Si se va demasiado lejos, vuelve a Idle
        if (dist > summonerData.stopChaseDistance)
        {
            manager.ChangeState(manager.IdleState);
            return;
        }

        // 2) Si ya está dentro de summonRange, pasa a Summon
        if (dist <= summonerData.summonRange)
        {
            manager.ChangeState(manager.SummonState);
            return;
        }

        // De lo contrario, sigue en Chase
    }

    public override void Exit()
    {
        Debug.Log("Summoner saliendo de estado: CHASE");

        if (summonerData.animator != null)
        {
            summonerData.animator.SetBool("IsMoving", false);
        }
    }

    private void UpdateChasePath()
    {
        NavMeshAgent navAgent = manager.GetComponent<NavMeshAgent>();
        if (navAgent != null && summonerData.targetTransform != null)
        {
            navAgent.SetDestination(summonerData.targetTransform.position);
        }
    }
}
