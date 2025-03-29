using UnityEngine;

/// <summary>
/// Estado donde el Summoner está quieto y revisa periódicamente
/// si hay un objetivo en rango. Igual que EnemyIdleState.
/// </summary>
public class SummonerIdleState : SummonerBaseState
{
    private float checkTimer;

    public SummonerIdleState(SummonerStateManager manager, SummonerData data)
        : base(manager, data) { }

    public override void Enter()
    {
        Debug.Log("Summoner entró en estado: IDLE");

        manager.StopAgent();

        if (summonerData.animator != null)
        {
            summonerData.animator.SetBool("IsMoving", false);
        }

        checkTimer = 0f;
    }

    public override void Update()
    {
        checkTimer -= Time.deltaTime;

        if (checkTimer <= 0f)
        {
            checkTimer = summonerData.idleCheckInterval;

            bool foundTarget = manager.CheckForTargetsInRange(summonerData.detectionRange);
            if (foundTarget)
            {
                manager.ChangeState(manager.ChaseState);
            }
        }
    }

    public override void Exit()
    {
        Debug.Log("Summoner saliendo de estado: IDLE");
    }
}