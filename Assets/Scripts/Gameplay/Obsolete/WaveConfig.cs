using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "NewWave", menuName = "WaveManager/WaveConfig")]
public class WaveConfig : ScriptableObject
{
    [Header("Información General de la Oleada")]
    [SerializeField] private int waveNumber;
    [SerializeField] private List<GameObject> permittedEnemyPrefabs;

    [Header("Parámetros de Spawn")]
    [SerializeField] private int totalEnemies = 5;
    [SerializeField] private float spawnInterval = 1.5f;
    
    
    // Propiedades de solo lectura
    public int WaveNumber => waveNumber;
    public List<GameObject> PermittedEnemyPrefabs => permittedEnemyPrefabs;
    public int TotalEnemies => totalEnemies;
    public float SpawnInterval => spawnInterval;
}