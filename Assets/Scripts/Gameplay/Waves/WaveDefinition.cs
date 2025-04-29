using UnityEngine;

/// <summary>
/// Data-driven description of a single wave.
/// Edit in Inspector, duplicate to create new waves.
/// </summary>
[CreateAssetMenu(menuName = "Alchemy/Waves/Wave Definition")]
public class WaveDefinition : ScriptableObject
{
    [Header("Basic")]
    [Tooltip("Wave number (purely informational).")]
    public int waveNumber = 1;

    [Header("Spawn Rules")]
    [Tooltip("Entries that describe which enemies and how many of each to spawn.")]
    public WaveSpawnEntry[] enemies;

    [Tooltip("Seconds to wait before spawning starts (count-down for the player).")]
    public float warmupTime = 5f;

    [Tooltip("Seconds between each enemy spawn.")]
    public float spawnInterval = 1.5f;
}

[System.Serializable]
public struct WaveSpawnEntry
{
    [Tooltip("Prefab of the enemy (Melee, Summoner, Caster, etc.).")]
    public GameObject enemyPrefab;

    [Tooltip("How many of this prefab to spawn.")]
    public int count;
}