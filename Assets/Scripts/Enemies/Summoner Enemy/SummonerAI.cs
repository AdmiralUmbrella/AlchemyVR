using UnityEngine;

public class SummonerAI : StateManager<SummonerState>, IEnemy
{
    [Header("Datos del Summoner (asignar en Inspector)")]
    public SummonerData summonerData;

    public TowerState tower;
    
    private UnityEngine.AI.NavMeshAgent agent;

    private void Awake()
    {
        // Configurar NavMeshAgent
        agent = GetComponent<UnityEngine.AI.NavMeshAgent>();
        if (agent != null)
        {
            agent.speed = summonerData.moveSpeed;
        }

        summonerData.currentHealth = summonerData.maxHealth;

        // Registrar los estados en el diccionario (utilizando la máquina de estados reusable)
        States.Add(SummonerState.Idle, new SummonerIdleState(this, summonerData));
        States.Add(SummonerState.Patrol, new SummonerPatrolState(this, summonerData));
        States.Add(SummonerState.Chase, new SummonerChaseState(this, summonerData));
        States.Add(SummonerState.Summon, new SummonerSummonState(this, summonerData));
        States.Add(SummonerState.Hit, new SummonerHitState(this, summonerData));
        States.Add(SummonerState.Dead, new SummonerDeadState(this, summonerData));

        // Asignar el Animator si aún no se ha hecho
        if (summonerData.animator == null && summonerData.modelRoot != null)
        {
            summonerData.animator = summonerData.modelRoot.GetComponent<Animator>();
        }

        if (summonerData.animator == null)
        {
            summonerData.animator = GetComponentInChildren<Animator>();
        }

        // Estado inicial
        CurrentState = States[SummonerState.Idle];
    }

    private void Start()
    {
        CurrentState.EnterState();
    }

    private void Update()
    {
        // Reducir cooldown de invocación si aplica
        if (summonerData.currentSummonTimer > 0f && !summonerData.isDead)
        {
            summonerData.currentSummonTimer -= Time.deltaTime;
        }

        // Actualización del estado a través del StateManager genérico
        SummonerState nextStateKey = CurrentState.GetNextState();
        if (!IsTransitioningState && nextStateKey.Equals(CurrentState.StateKey))
        {
            CurrentState.UpdateState();
        }
        else if (!IsTransitioningState)
        {
            TransitionToState(nextStateKey);
        }
    }

    #region Métodos Auxiliares

    public void StopAgent()
    {
        if (agent != null)
        {
            agent.isStopped = true;
            agent.velocity = Vector3.zero;
        }
    }

    public void ResumeAgent()
    {
        if (agent != null)
        {
            agent.isStopped = false;
        }
    }

    public bool CheckForTargetsInRange(float range)
    {
        // Primero se buscan torres
        GameObject[] towers = GameObject.FindGameObjectsWithTag("Tower");
        Transform nearestTower = null;
        float nearestDist = Mathf.Infinity;
        foreach (var t in towers)
        {
            float dist = Vector3.Distance(transform.position, t.transform.position);
            if (dist <= range && dist < nearestDist)
            {
                nearestDist = dist;
                nearestTower = t.transform;
            }
        }

        if (nearestTower != null)
        {
            summonerData.targetTransform = nearestTower;
            return true;
        }

        // Si no, se busca el Player
        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null)
        {
            float distPlayer = Vector3.Distance(transform.position, playerObj.transform.position);
            if (distPlayer <= range)
            {
                summonerData.targetTransform = playerObj.transform;
                return true;
            }
        }

        return false;
    }
    
    public bool CheckForTowerInRange(float range)
    {
        GameObject[] towers = GameObject.FindGameObjectsWithTag("Tower");
        Transform nearestTower = null;
        float nearestDist = Mathf.Infinity;

        foreach (var t in towers)
        {
            float dist = Vector3.Distance(transform.position, t.transform.position);
            if (dist <= range && dist < nearestDist)
            {
                nearestDist = dist;
                nearestTower = t.transform;
            }
        }

        // Devuelve true si encontró al menos una torre
        return (nearestTower != null);
    }
    
    public void DestroyAllSummonedEnemies()
    {
        // Recorre la lista y destruye los objetos
        foreach (GameObject summoned in summonerData.summonedEnemies)
        {
            if (summoned != null)
            {
                Destroy(summoned);
            }
        }
        // Limpia la lista
        summonerData.summonedEnemies.Clear();
        Debug.Log("¡Se han destruido todos los enemigos invocados!");
    }

    #endregion

    #region Implementación IEnemy

    /// <summary>
    /// Permite cambiar de estado cuando se recibe un efecto (por ejemplo, "Hit") desde una poción.
    /// </summary>
    /// <param name="newState">Puede ser un string o un SummonerState.</param>
    public void ChangeState(object newState)
    {
        if (newState is string str)
        {
            // Se interpreta el string para definir el estado a cambiar
            if (str == "Hit")
                TransitionToState(SummonerState.Hit);
            else if (str == "Idle")
                TransitionToState(SummonerState.Idle);
            else if (str == "Chase")
                TransitionToState(SummonerState.Chase);
            else if (str == "Summon")
                TransitionToState(SummonerState.Summon);
            else if (str == "Dead")
                TransitionToState(SummonerState.Dead);
        }
        else if (newState is SummonerState state)
        {
            TransitionToState(state);
        }
    }

    /// <summary>
    /// Permite que el Summoner reciba daño a través de efectos de poción.
    /// </summary>
    /// <param name="damage">Cantidad de daño recibido.</param>
    /// <param name="hitPosition">Posición de impacto.</param>
    /// <param name="damageSource">Tipo o nombre de la poción que inflige el daño.</param>
    public void TakeDamage(int damage, Vector3 hitPosition, string damageSource)
    {
        if (summonerData.isDead || summonerData.isStunned) return;

        float finalDamage = damage;
        switch (damageSource)
        {
            case "Pyro":
                finalDamage *= (1f - summonerData.pyroResistance);
                break;
            case "Aqua":
                finalDamage *= (1f - summonerData.aquaResistance);
                break;
            case "Geo":
                finalDamage *= (1f - summonerData.geoResistance);
                break;
            case "Ventus":
                finalDamage *= (1f - summonerData.ventusResistance);
                break;
        }

        int dmgRounded = Mathf.RoundToInt(finalDamage);
        summonerData.currentHealth -= dmgRounded;
        summonerData.knockbackDirection = (transform.position - hitPosition).normalized;
        Debug.Log($"Summoner recibió {dmgRounded} de daño {damageSource}. Vida actual: {summonerData.currentHealth}");

        if (summonerData.currentHealth <= 0)
        {
            Die();
        }
        else if (summonerData.canBeStunned)
        {
            TransitionToState(SummonerState.Hit);
        }
    }

    /// <summary>
    /// Propiedad de solo lectura que retorna la transformación actual.
    /// </summary>
    public Transform EnemyTransform => transform;

    #endregion

    /// <summary>
    /// Llamado cuando la salud llega a cero.
    /// </summary>
    public void Die()
    {
        if (!summonerData.isDead)
        {
            TransitionToState(SummonerState.Dead);
        }
    }

    #region Visualización con Gizmos

    private void OnDrawGizmosSelected()
    {
        if (summonerData == null) return;

        // Rango de detección
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, summonerData.detectionRange);

        // Rango de invocación
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(transform.position, summonerData.summonRange);

        // Distancia para dejar de perseguir
        Gizmos.color = Color.blue;
        Gizmos.DrawWireSphere(transform.position, summonerData.stopChaseDistance);

        if (summonerData.targetTransform != null)
        {
            Gizmos.color = Color.cyan;
            Gizmos.DrawLine(transform.position, summonerData.targetTransform.position);
        }

        if (summonerData.summonedEnemies != null && summonerData.summonedEnemies.Count > 0)
        {
            Gizmos.color = Color.magenta;
            foreach (var enemy in summonerData.summonedEnemies)
            {
                if (enemy != null)
                {
                    Gizmos.DrawLine(transform.position, enemy.transform.position);
                }
            }
        }
    }

    #endregion
}