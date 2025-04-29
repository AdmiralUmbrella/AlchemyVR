using UnityEngine;
using System.Collections;

public class WaveManagerOLD : MonoBehaviour
{
    [Header("Otras referencias")] [SerializeField]
    private Transform[] spawnPoints; // Lugares donde se generan los enemigos

    [SerializeField] private EnemySpawner enemySpawner; // Componente o script que se encarga del spawn real de enemigos

    [Header("Configuraciones de Oleadas")] 
    [SerializeField]
    private WaveConfig[] waveConfigs; // Array de configuraciones para las oleadas

    [SerializeField] 
    private bool loopAfterLastWave = false; // Opcional: si deseas seguir iterando una vez termines las oleadas definidas
    
    
    public int currentWaveIndex = 0;
    private bool isSpawningWave = false;

    void Start()
    {
        // Inicia la primera oleada al arrancar el juego, o cuando lo desees
        StartCoroutine(StartWaveRoutine());
    }

    // Método principal que inicia la siguiente oleada.
    private IEnumerator StartWaveRoutine()
    {
        if (currentWaveIndex >= waveConfigs.Length)
        {
            // Si se supera la última oleada...
            if (loopAfterLastWave)
            {
                // Vuelve a la primera (o modifica la lógica con un patrón incremental si quieres)
                currentWaveIndex = 0;
            }
            else
            {
                Debug.Log("Todas las oleadas han finalizado.");
                yield break; // Detener corrutina si no deseas más oleadas
            }
        }

        isSpawningWave = true;
        WaveConfig currentWave = waveConfigs[currentWaveIndex];

        // Empieza a spawnear los enemigos de la wave actual
        yield return StartCoroutine(SpawnEnemiesInWave(currentWave));

        // Espera a que todos los enemigos sean destruidos antes de avanzar
        // Esto requiere que lleves un conteo de enemigos activos (puedes manejarlo dentro de EnemySpawner)
        while (enemySpawner.EnemiesAlive > 0)
        {
            Debug.Log($"[WaveMgr] EnemiesAlive = {enemySpawner.EnemiesAlive}");
            yield return null;
        }

        yield return new WaitForSeconds(2f); // Pequeña pausa antes de la próxima oleada

        // Continúa con la siguiente oleada
        currentWaveIndex++;
        isSpawningWave = false;
        StartCoroutine(StartWaveRoutine());
    }

    // Corrutina para spawnear enemigos según la configuración de la oleada
    private IEnumerator SpawnEnemiesInWave(WaveConfig waveConfig)
    {
        int totalToSpawn = waveConfig.TotalEnemies;

        for (int i = 0; i < totalToSpawn; i++)
        {
            // Elige un enemigo aleatorio de la lista de prefabs permitidos
            var enemyPrefab = waveConfig.PermittedEnemyPrefabs[Random.Range(0, waveConfig.PermittedEnemyPrefabs.Count)];

            // Elige un punto de spawn aleatorio
            Transform spawnPoint = spawnPoints[Random.Range(0, spawnPoints.Length)];

            // Invoca el spawn
            enemySpawner.SpawnEnemy(enemyPrefab, spawnPoint.position, waveConfig);

            // Espera el tiempo de intervalo antes de spawnear el siguiente enemigo
            yield return new WaitForSeconds(waveConfig.SpawnInterval);
        }
    }
}