using UnityEngine;

[System.Serializable]
public class SummonerData : EnemyData
{
    [Header("Summon Settings")]
    public GameObject basicEnemyPrefab;
    public float summonInterval = 5f;
    public int maxSummonedEnemies = 5;
    public float summonRadius = 2f;
}

