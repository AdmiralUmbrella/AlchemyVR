using UnityEngine;

public class EnemySummonState : EnemyBaseState
{
    // Referencia local al SummonerData para acceder a campos espec�ficos
    private SummonerData summonerData;

    //Temporizador para controlar la ventana de invocaci�n
    private float summonTimer;

    /// <summary>
    /// Constructor que recibe el manager (FSM) y la data especializada.
    /// </summary>
    public EnemySummonState(EnemyStateManager manager, SummonerData data) : base(manager, data)
    {
        summonerData = data;
    }

    /// <summary>
    /// Se llama al entrar en el estado de invocaci�n.
    /// </summary>
    public override void Enter()
    {
        Debug.Log("Enemigo entr� en estado: SUMMON");

        // Detenemos el movimiento del NavMeshAgent mientras invoca
        manager.StopAgent();

        // Reseteamos el temporizador al cooldown de invocaci�n
        summonTimer = summonerData.summonCooldown;

        //Disparar la animaci�n del Summoner
        if (summonerData.animator != null)
        {
            summonerData.animator.SetTrigger("Summon");
        }

        //Invocar a los minions ahora o en un retardo; aqu� se hace de inmediato
        SummonMinions();
    }

    /// <summary>
    /// Se llama cada frame mientras el Summoner est� en ese estado
    /// </summary>
    public override void Update()
    {
        // Bajamos el tiempo del timer
        summonTimer -= Time.deltaTime;

        // Al terminar el tiempo de invocaci�n (la animaci�n),
        // elegimos a qu� estado volver, por ejemplo, chase o idle.
        if (summonTimer <= 0f)
        {
            // Si hay un objetivo cercano, pasamos a Chase
            if (manager.IsPlayerInRange(summonerData.detectionRange))
            {
                manager.ChangeState(manager.ChaseState);
            }
            else
            {
                // Si no hay nadie, volvemos a Idle
                manager.ChangeState(manager.IdleState);
            }
        }
    }

    /// <summary>
    /// Se llama al salir de este estado
    /// </summary>
    public override void Exit()
    {
        Debug.Log("Enemigo saliendo del estado: SUMMON");
    }

    /// <summary>
    /// Funci�n que instancia a los minions b�sicos.
    /// </summary>
    private void SummonMinions()
    {
        // Iteramos seg�n el n�mero de minions que queremos invocar
        for (int i = 0; i < summonerData.numberOfMinions; i++)
        {
            // Calculamos una posici�n aleatoria alrededor del Summoner
            Vector3 offset = new Vector3(
                Random.Range(-summonerData.summonRange, summonerData.summonRange),
                0f,
                Random.Range(-summonerData.summonRange, summonerData.summonRange)
            );

            // Sumamos esa posici�n aleatoria a la posici�n actual del Summoner
            Vector3 spawnPosition = manager.transform.position + offset;

            // Instanciamos el prefab del minion
            GameObject minion = GameObject.Instantiate(
                summonerData.minionPrefab,
                spawnPosition,
                Quaternion.identity);

            var minionManager = minion.GetComponent<EnemyStateManager>();
            minionManager.enemyData.playerTransform = summonerData.playerTransform;
        }
    }
}
