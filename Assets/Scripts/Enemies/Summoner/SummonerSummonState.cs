using UnityEngine;
using System.Collections.Generic;

public class SummonerSummonState : EnemyBaseState
{
    private SummonerData summonerData;
    private float summonTimer;
    private List<GameObject> summonedEnemies;

    public SummonerSummonState(EnemyStateManager manager, SummonerData summonerData)
        : base(manager, summonerData)
    {
        this.summonerData = summonerData;
        summonedEnemies = new List<GameObject>();
        summonTimer = summonerData.summonInterval;
    }

    public override void Enter()
    {
        Debug.Log("Summoner entró en estado: SUMMON");
        manager.StopAgent();
        summonTimer = 0f; // Invocación inmediata al entrar al estado

        if (summonerData.animator != null)
            summonerData.animator.SetTrigger("Summon");
    }



    public override void Update()
    {
        summonTimer -= Time.deltaTime;

        if (summonTimer <= 0f)
        {
            CleanupSummonedEnemies();

            if (summonedEnemies.Count < summonerData.maxSummonedEnemies)
            {
                SummonEnemy();
            }
        }

        summonTimer = summonerData.summonInterval;

        if (!manager.CheckForTargetsInRange(summonerData.stopChaseDistance))
        {
            manager.ChangeState(manager.IdleState);
        }
    }

    public override void Exit()
    {
        Debug.Log("Summoner saliendo de estado: SUMMON");
    }

    private void SummonEnemy()
    {
        if (summonerData.basicEnemyPrefab == null)
        {
            Debug.LogWarning("No se asignó prefab para invocar enemigos.");
            return;
        }

        Vector3 spawnPosition = manager.transform.position +
                                Random.insideUnitSphere * summonerData.summonRadius;
        spawnPosition.y = manager.transform.position.y;

        GameObject newEnemy = GameObject.Instantiate(summonerData.basicEnemyPrefab, spawnPosition, Quaternion.identity);

        EnemyStateManager newEnemyManager = newEnemy.GetComponent<EnemyStateManager>();
        if (newEnemyManager != null)
        {
            newEnemyManager.enemyData.playerTransform = summonerData.playerTransform;
        }

        summonedEnemies.Add(newEnemy);
        Debug.Log("Enemigo básico invocado.");
    }

    private void CleanupSummonedEnemies()
    {
        summonedEnemies.RemoveAll(enemy => enemy == null);
    }
}
