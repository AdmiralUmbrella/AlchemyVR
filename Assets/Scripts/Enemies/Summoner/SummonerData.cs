using UnityEngine;

/// <summary>
/// Datos espec�ficos para el Summoner, que extiende la l�gica base de EnemyData.
/// </summary>
[System.Serializable]
public class SummonerData : EnemyData
{
    [Header("Summoner Settings")] [Tooltip("Prefab del enemigo b�sico que se va a invocar")]
    public GameObject minionPrefab;

    [Tooltip("N�mero de minions que se invocar�n en cada 'summon'.")]
    public int numberOfMinions = 3;

    [Tooltip("Tiempo de reutilizaci�n (cooldown) entre invocaciones.")]
    public float summonCooldown = 5f;

    [Tooltip("Rango alrededor del Summoner donde aparecer�n los minions")]
    public float summonRange = 2f;
}
