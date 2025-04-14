using UnityEngine;

public class EnemySpawner : MonoBehaviour
{
    public int EnemiesAlive { get; private set; } = 0;

    // Método responsable de instanciar al enemigo con información adicional
    public void SpawnEnemy(GameObject enemyPrefab, Vector3 spawnPosition, WaveConfig waveConfig)
    {
        GameObject newEnemy = Instantiate(enemyPrefab, spawnPosition, Quaternion.identity);
        EnemiesAlive++;

        // Suscribirte al evento de destrucción
        var meleeAI = newEnemy.GetComponent<MeleeEnemyAI>();
        if (meleeAI != null)
        {
            meleeAI.OnEnemyDestroyed += HandleMeleeEnemyDestroyed;
        }

        var casterAI = newEnemy.GetComponent<CasterEnemyAI>();
        if (casterAI != null)
        {
            casterAI.OnEnemyDestroyed += HandleCasterEnemyDestroyed;
        }

        var summonerAI = newEnemy.GetComponent<SummonerAI>();
        if (summonerAI != null)
        {
            summonerAI.OnEnemyDestroyed += HandleSummonerEnemyDestroyed;
        }
        // Si necesitas ajustar multiplicadores de salud, velocidad, etc., aquí lo harías
    }

    private void HandleMeleeEnemyDestroyed(MeleeEnemyAI meleeEnemy)
    {
        EnemiesAlive--;
        if (EnemiesAlive < 0) EnemiesAlive = 0;

        meleeEnemy.OnEnemyDestroyed -= HandleMeleeEnemyDestroyed;
    }

    private void HandleCasterEnemyDestroyed(CasterEnemyAI casterEnemy)
    {
        EnemiesAlive--;
        if (EnemiesAlive < 0) EnemiesAlive = 0;

        casterEnemy.OnEnemyDestroyed -= HandleCasterEnemyDestroyed;
    }

    private void HandleSummonerEnemyDestroyed(SummonerAI summonerEnemy)
    {
        EnemiesAlive--;
        if (EnemiesAlive < 0) EnemiesAlive = 0;

        summonerEnemy.OnEnemyDestroyed -= HandleSummonerEnemyDestroyed;
    }
}