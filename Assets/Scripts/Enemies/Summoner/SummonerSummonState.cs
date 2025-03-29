using UnityEngine;

/// <summary>
/// Estado donde el Summoner "ataca" invocando enemigos,
/// igual que un AttackState en Enemy, pero sin da�o f�sico.
/// </summary>
public class SummonerSummonState : SummonerBaseState
{
    private float animationDuration = 2f; // Duraci�n de la anim de invocaci�n
    private float currentTime;
    private bool hasInvokedThisCycle;

    public SummonerSummonState(SummonerStateManager manager, SummonerData data)
        : base(manager, data) { }

    public override void Enter()
    {
        Debug.Log("Summoner entr� en estado: SUMMON (Invocaci�n)");

        manager.StopAgent();

        currentTime = animationDuration;
        hasInvokedThisCycle = false;

        if (summonerData.animator != null)
        {
            // Usamos "Attack" como trigger, para parecerse a EnemyAttackState
            summonerData.animator.SetTrigger("Summon");
        }
    }

    public override void Update()
    {
        currentTime -= Time.deltaTime;

        // Similar a damageDelay: en la mitad de la anim, invocamos
        if (!hasInvokedThisCycle && currentTime <= animationDuration * 0.5f)
        {
            TrySummon();
            hasInvokedThisCycle = true;
        }

        // Al terminar la "animaci�n"
        if (currentTime <= 0f)
        {
            // Revisamos si puede seguir invocando o cambiar a Chase/Idle
            float dist = Mathf.Infinity;
            if (summonerData.targetTransform != null)
            {
                dist = Vector3.Distance(manager.transform.position,
                                        summonerData.targetTransform.position);
            }

            bool inRange = (dist <= summonerData.detectionRange);
            bool canSummonAgain = (summonerData.currentSummonTimer <= 0f);

            // Si sigue cerca y no hay cooldown, repetimos invocaci�n
            if (inRange && canSummonAgain)
            {
                manager.ChangeState(manager.SummonState);
            }
            else
            {
                // Si est� en rango medio, volver a Chase
                if (dist < summonerData.stopChaseDistance)
                {
                    manager.ChangeState(manager.ChaseState);
                }
                else
                {
                    manager.ChangeState(manager.IdleState);
                }
            }
        }
    }

    public override void Exit()
    {
        Debug.Log("Summoner saliendo de estado: SUMMON (Invocaci�n)");
    }

    private void TrySummon()
    {
        // L�mite de minions y cooldown
        if (summonerData.currentSummonTimer <= 0f &&
            summonerData.summonedEnemies.Count < summonerData.maxSummonedEnemies)
        {
            Vector3 spawnPos = manager.transform.position + manager.transform.forward * 2f;
            GameObject newEnemy = GameObject.Instantiate(summonerData.basicEnemyPrefab, spawnPos, Quaternion.identity);

            summonerData.summonedEnemies.Add(newEnemy);

            summonerData.currentSummonTimer = summonerData.summonCooldown;

            Debug.Log("Summoner invoc� un enemigo!");
        }
    }
}
