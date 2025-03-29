using UnityEngine;

/// <summary>
/// Estado de muerte para el Summoner. Se asemeja a EnemyDeadState.
/// </summary>
public class SummonerDeadState : SummonerBaseState
{
    public SummonerDeadState(SummonerStateManager manager, SummonerData data)
        : base(manager, data) { }

    public override void Enter()
    {
        Debug.Log("Summoner entró en estado: DEAD");

        summonerData.isDead = true;
        manager.StopAgent();

        summonerData.currentDeathTime = summonerData.deathDuration;

        if (summonerData.animator != null)
        {
            summonerData.animator.ResetTrigger("Summon");
            summonerData.animator.SetBool("IsMoving", false);
            summonerData.animator.SetTrigger("Dead");
        }
    }

    public override void Update()
    {
        // Reducimos el tiempo hasta que se destruya (si shouldDestroyOnDeath)
        if (!summonerData.shouldDestroyOnDeath) return;

        summonerData.currentDeathTime -= Time.deltaTime;
        if (summonerData.currentDeathTime <= 0f)
        {
            Debug.Log("Destruyendo Summoner muerto.");
            GameObject.Destroy(manager.gameObject);
        }
    }

    public override void Exit()
    {
        Debug.Log("Summoner saliendo de estado: DEAD");
        // Normalmente no sales de DeadState, pero aquí por consistencia
    }
}