using UnityEngine;

/// <summary>
/// Datos específicos para el Summoner, que extiende la lógica base de EnemyData.
/// </summary>
[System.Serializable]
public class SummonerData : EnemyData
{
    [Header("Summoner Settings")] [Tooltip("Prefab del enemigo básico que se va a invocar")]
    public GameObject minionPrefab;

    [Tooltip("Número de minions que se invocarán en cada 'summon'.")]
    public int numberOfMinions = 3;

    [Tooltip("Tiempo de reutilización (cooldown) entre invocaciones.")]
    public float summonCooldown = 5f;

    [Tooltip("Rango alrededor del Summoner donde aparecerán los minions")]
    public float summonRange = 2f;
}
