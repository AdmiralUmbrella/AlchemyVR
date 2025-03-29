using UnityEngine;
using UnityEngine.AI;

/// <summary>
/// Equivalente a EnemyStateManager, maneja la FSM del Summoner
/// con el mismo formato que la clase del enemigo básico.
/// </summary>
public class SummonerStateManager : MonoBehaviour
{
    // Delegado/Evento por si deseas notificar la muerte
    public delegate void SummonerDeathHandler();
    public event SummonerDeathHandler OnSummonerDeath;

    [Header("Datos del Summoner (asignar en Inspector)")]
    public SummonerData summonerData;

    private NavMeshAgent agent;           // Referencia interna al NavMeshAgent
    private SummonerBaseState currentState;  // Estado actual de la FSM

    /*--------------------------------------------------------------------------
     * Estados del Summoner (Idle, Chase, Summon, Hit, Dead)
     * Mismo formato: public XState NombreState { get; private set; }
     *------------------------------------------------------------------------*/
    public SummonerIdleState IdleState { get; private set; }
    public SummonerChaseState ChaseState { get; private set; }
    public SummonerSummonState SummonState { get; private set; }
    public SummonerHitState HitState { get; private set; }
    public SummonerDeadState DeadState { get; private set; }

    private void Awake()
    {
        // 1) Configurar NavMeshAgent
        agent = GetComponent<NavMeshAgent>();
        if (agent != null)
        {
            agent.speed = summonerData.moveSpeed;
        }

        // 2) Inicializar vida
        summonerData.currentHealth = summonerData.maxHealth;

        // 3) Crear instancias de los estados
        IdleState = new SummonerIdleState(this, summonerData);
        ChaseState = new SummonerChaseState(this, summonerData);
        SummonState = new SummonerSummonState(this, summonerData);
        HitState = new SummonerHitState(this, summonerData);
        DeadState = new SummonerDeadState(this, summonerData);

        // 4) Buscar Animator si no se asignó
        if (summonerData.animator == null && summonerData.modelRoot != null)
        {
            summonerData.animator = summonerData.modelRoot.GetComponent<Animator>();
        }
        if (summonerData.animator == null)
        {
            summonerData.animator = GetComponentInChildren<Animator>();
        }
    }

    private void Start()
    {
        // Empezar en Idle
        ChangeState(IdleState);
    }

    private void Update()
    {
        // Reducir cooldown de invocación, si aplica
        if (summonerData.currentSummonTimer > 0f && !summonerData.isDead)
        {
            summonerData.currentSummonTimer -= Time.deltaTime;
        }

        // Actualizar el estado actual
        currentState?.Update();
    }

    /// <summary>
    /// Mismo método para cambiar de estado que en EnemyStateManager.
    /// </summary>
    public void ChangeState(SummonerBaseState newState)
    {
        currentState?.Exit();
        currentState = newState;
        currentState?.Enter();
    }

    /*--------------------------------------------------------------------------
     * Métodos auxiliares para movimiento, búsqueda de targets, daño, etc.
     * Iguales a EnemyStateManager, adaptados al Summoner.
     *------------------------------------------------------------------------*/
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

    /// <summary>
    /// Prioriza torre si está en rango; si no, busca Player. Igual que EnemyStateManager.
    /// </summary>
    public bool CheckForTargetsInRange(float range)
    {
        // 1) Buscar torres
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

        // Si encontramos torre, la asignamos
        if (nearestTower != null)
        {
            summonerData.targetTransform = nearestTower;
            return true;
        }

        // 2) Si no hay torre, buscamos Player
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

    /// <summary>
    /// Recibir daño con resistencias elementales y knockback,
    /// igual que en EnemyStateManager.
    /// </summary>
    public void TakeDamage(int damage, Vector3 attackerPosition, string damageType)
    {
        // Si está muerto o aturdido, ignoramos
        if (summonerData.isDead || summonerData.isStunned) return;

        float finalDamage = damage;

        // Aplicamos resistencias, igual que EnemyStateManager
        switch (damageType)
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
            case "Aero":
                finalDamage *= (1f - summonerData.aeroResistance);
                break;
        }

        // Redondeamos y descontamos vida
        int dmgRounded = Mathf.RoundToInt(finalDamage);
        summonerData.currentHealth -= dmgRounded;

        // Guardar dirección de knockback
        summonerData.knockbackDirection = (transform.position - attackerPosition).normalized;

        Debug.Log($"Summoner recibió {dmgRounded} de daño {damageType}. Vida actual: {summonerData.currentHealth}");

        if (summonerData.currentHealth <= 0)
        {
            Die();
        }
        else if (summonerData.canBeStunned)
        {
            // Pasamos al estado Hit
            ChangeState(HitState);
        }
    }

    public void Die()
    {
        if (!summonerData.isDead)
        {
            OnSummonerDeath?.Invoke();
            ChangeState(DeadState);
        }
    }

    /*--------------------------------------------------------------------------
     * Visual Cues (Gizmos), igual que EnemyStateManager
     *------------------------------------------------------------------------*/
    private void OnDrawGizmosSelected()
    {
        if (summonerData == null) return;

        // 1) Rango principal de detección
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, summonerData.detectionRange);

        // 2) Rango de invocación
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(transform.position, summonerData.summonRange);

        // 3) Distancia para dejar de perseguir
        Gizmos.color = Color.blue;
        Gizmos.DrawWireSphere(transform.position, summonerData.stopChaseDistance);

        // Si tiene un target, dibujamos línea
        if (summonerData.targetTransform != null)
        {
            Gizmos.color = Color.cyan;
            Gizmos.DrawLine(transform.position, summonerData.targetTransform.position);
        }

        // Dibujar conexiones a enemigos invocados
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

}
