using UnityEngine;

/// <summary>
/// Clase abstracta base para todos los estados del Summoner,
/// equivalente a EnemyBaseState.
/// </summary>
public abstract class SummonerBaseState
{
    protected SummonerStateManager manager;
    protected SummonerData summonerData;

    public SummonerBaseState(SummonerStateManager manager, SummonerData summonerData)
    {
        this.manager = manager;
        this.summonerData = summonerData;
    }

    // Mismos métodos abstractos que en EnemyBaseState
    public abstract void Enter();
    public abstract void Update();
    public abstract void Exit();
}