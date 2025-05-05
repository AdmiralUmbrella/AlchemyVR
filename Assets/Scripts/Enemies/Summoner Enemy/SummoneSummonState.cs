using UnityEngine;

public class SummonerSummonState : BaseState<SummonerState>
{
    private SummonerAI manager;
    private SummonerData summonerData;
    private float animationDuration = 2.7f;
    private float currentTime;
    private bool hasInvokedThisCycle;
    private SummonerState nextState;

    public SummonerSummonState(SummonerAI manager, SummonerData data)
        : base(SummonerState.Summon)
    {
        this.manager = manager;
        this.summonerData = data;
        nextState = SummonerState.Summon;
    }

    public override void EnterState()
    {
        Debug.Log("Summoner entró en estado: SUMMON (Invocación)");
        manager.StopAgent();
        currentTime = animationDuration;
        hasInvokedThisCycle = false;
        if (summonerData.animator != null)
        {
            summonerData.animator.SetTrigger("Summon");
        }

        nextState = SummonerState.Summon;
    }

    public override void UpdateState()
    {
        currentTime -= Time.deltaTime;

        if (!hasInvokedThisCycle && currentTime <= animationDuration * 0.5f)
        {
            TrySummon();
            hasInvokedThisCycle = true;
        }

        // Cuando se termine el ciclo de animación...
        if (currentTime <= 0f)
        {
            float dist = summonerData.targetTransform != null
                ? Vector3.Distance(manager.transform.position, summonerData.targetTransform.position)
                : Mathf.Infinity;
            bool inRange = (dist <= summonerData.detectionRange);
            bool canSummonAgain = (summonerData.currentSummonTimer <= 0f);

            if (inRange && canSummonAgain)
            {
                // Reiniciamos el ciclo para que se invoque nuevamente
                currentTime = animationDuration;
                hasInvokedThisCycle = false;
                nextState = SummonerState.Summon;
            }
            else
            {
                nextState = (dist < summonerData.stopChaseDistance) ? SummonerState.Patrol : SummonerState.Idle;
            }
        }
    }

    public override void ExitState()
    {
        Debug.Log("Summoner saliendo de estado: SUMMON (Invocación)");
        summonerData.animator.ResetTrigger("Summon");
    }

    public override SummonerState GetNextState()
    {
        return nextState;
    }

    public override void OnTriggerEnter(Collider other)
    {
    }

    public override void OnTriggerStay(Collider other)
    {
    }

    public override void OnTriggerExit(Collider other)
    {
    }

    private void TrySummon()
    {
        if (summonerData.currentSummonTimer <= 0f &&
            summonerData.summonedEnemies.Count < summonerData.maxSummonedEnemies)
        {
            Vector3 spawnPos = manager.transform.position + manager.transform.forward * 2f;
            GameObject newEnemy = GameObject.Instantiate(summonerData.basicEnemyPrefab, spawnPos, Quaternion.identity);

            summonerData.summonedEnemies.Add(newEnemy);

            summonerData.currentSummonTimer = summonerData.summonCooldown;

            Debug.Log("Summoner invocó un enemigo!");
        }
    }
}