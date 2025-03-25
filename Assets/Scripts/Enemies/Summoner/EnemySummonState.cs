using UnityEngine;

public class EnemySummonState : EnemyBaseState
{
    // Referencia local al SummonerData para acceder a campos específicos
    private SummonerData summonerData;

    //Temporizador para controlar la ventana de invocación
    private float summonTimer;

    /// <summary>
    /// Constructor que recibe el manager (FSM) y la data especializada.
    /// </summary>
    public EnemySummonState(EnemyStateManager manager, SummonerData data) : base(manager, data)
    {
        summonerData = data;
    }

    /// <summary>
    /// Se llama al entrar en el estado de invocación.
    /// </summary>
    public override void Enter()
    {
        Debug.Log("Enemigo entró en estado: SUMMON");

        // Detenemos el movimiento del NavMeshAgent mientras invoca
        manager.StopAgent();

        // Reseteamos el temporizador al cooldown de invocación
        summonTimer = summonerData.summonCooldown;

        //Disparar la animación del Summoner
        if (summonerData.animator != null)
        {
            summonerData.animator.SetTrigger("Summon");
        }

        //Invocar a los minions ahora o en un retardo; aquí se hace de inmediato
        SummonMinions();
    }

    /// <summary>
    /// Se llama cada frame mientras el Summoner está en ese estado
    /// </summary>
    public override void Update()
    {
        // Bajamos el tiempo del timer
        summonTimer -= Time.deltaTime;

        // Al terminar el tiempo de invocación (la animación),
        // elegimos a qué estado volver, por ejemplo, chase o idle.
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
    /// Función que instancia a los minions básicos.
    /// </summary>
    private void SummonMinions()
    {
        // Iteramos según el número de minions que queremos invocar
        for (int i = 0; i < summonerData.numberOfMinions; i++)
        {
            // Calculamos una posición aleatoria alrededor del Summoner
            Vector3 offset = new Vector3(
                Random.Range(-summonerData.summonRange, summonerData.summonRange),
                0f,
                Random.Range(-summonerData.summonRange, summonerData.summonRange)
            );

            // Sumamos esa posición aleatoria a la posición actual del Summoner
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
