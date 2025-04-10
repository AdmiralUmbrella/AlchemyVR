using UnityEngine;
using UnityEngine.AI;
using System.Collections.Generic;

/// <summary>
/// Controla la lógica de la máquina de estados del CasterEnemy,
/// encargándose de instanciar y transicionar entre estados, y de implementar la interfaz IEnemy para efectos externos.
/// </summary>
public class CasterEnemyAI : StateManager<CasterEnemyState>, IEnemy
{
    [Header("Caster Enemy Data")]
    [Tooltip("Datos configurables para el Caster, como rangos, tiempos y referencias.")]
    public CasterEnemyData enemyData;

    private void Awake()
    {
        // Asegurarse de que enemyData esté asignado correctamente
        if (enemyData == null)
        {
            enemyData = GetComponent<CasterEnemyData>();
        }
        if (enemyData == null)
        {
            Debug.LogError("CasterEnemyData no está asignado en CasterEnemyAI.");
            return;
        }

        enemyData.currentHealth = enemyData.maxHealth;
        enemyData.currentCooldown = 0f;

        // Configurar NavMeshAgent
        enemyData.agent = GetComponent<NavMeshAgent>();
        if (enemyData.agent != null)
        {
            enemyData.agent.speed = enemyData.moveSpeed;
        }

        // Buscar el Animator si no se asignó manualmente
        if (enemyData.animator == null && enemyData.modelRoot != null)
        {
            enemyData.animator = enemyData.modelRoot.GetComponent<Animator>();
        }
        if (enemyData.animator == null)
        {
            enemyData.animator = GetComponentInChildren<Animator>();
        }

        // Inicializar la máquina de estados
        States = new Dictionary<CasterEnemyState, BaseState<CasterEnemyState>>();
        States.Add(CasterEnemyState.Idle, new CasterEnemyIdleState(CasterEnemyState.Idle, this, enemyData));
        States.Add(CasterEnemyState.Chase, new CasterEnemyChaseState(CasterEnemyState.Chase, this, enemyData));
        States.Add(CasterEnemyState.Casting, new CasterEnemyCastingState(CasterEnemyState.Casting, this, enemyData, enemyData.tower));
        States.Add(CasterEnemyState.Hit, new CasterEnemyHitState(CasterEnemyState.Hit, this, enemyData));
        States.Add(CasterEnemyState.Dead, new CasterEnemyDeadState(CasterEnemyState.Dead, this, enemyData));

        CurrentState = States[CasterEnemyState.Idle];
    }

    #region Métodos de Utilidad

    /// <summary>
    /// Verifica si hay objetivos en rango, dando prioridad a torres sobre el jugador.
    /// Asigna el targetTransform en enemyData si se encuentra un objetivo.
    /// </summary>
    /// <param name="range">Rango de detección.</param>
    /// <returns>True si se encuentra un objetivo, false en caso contrario.</returns>
    public bool CheckForTargetsInRange(float range)
    {
        Vector3 myPosition = transform.position;

        // Buscar torres primero (tag: "Tower")
        GameObject[] towers = GameObject.FindGameObjectsWithTag("Tower");
        Transform nearestTower = null;
        float nearestDistance = Mathf.Infinity;
        foreach (GameObject tower in towers)
        {
            float d = Vector3.Distance(myPosition, tower.transform.position);
            if (d <= range && d < nearestDistance)
            {
                nearestDistance = d;
                nearestTower = tower.transform;
            }
        }
        if (nearestTower != null)
        {
            enemyData.targetTransform = nearestTower;
            return true;
        }

        // Si no hay torres en rango, buscar al jugador (tag: "Player")
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            float d = Vector3.Distance(myPosition, player.transform.position);
            if (d <= range)
            {
                enemyData.targetTransform = player.transform;
                return true;
            }
        }

        enemyData.targetTransform = null;
        return false;
    }

    #endregion

    #region IEnemy Implementation

    /// <summary>
    /// Permite cambiar el estado del enemigo desde efectos externos (por ejemplo, pociones).
    /// </summary>
    /// <param name="newState">Nuevo estado, ya sea como enum o string.</param>
    public void ChangeState(object newState)
    {
        if (newState is CasterEnemyState state)
        {
            TransitionToState(state);
        }
        else if (newState is string stateStr)
        {
            if (stateStr == "Hit")
                TransitionToState(CasterEnemyState.Hit);
            else if (stateStr == "Idle")
                TransitionToState(CasterEnemyState.Idle);
            else if (stateStr == "Chase")
                TransitionToState(CasterEnemyState.Chase);
            else if (stateStr == "Casting")
                TransitionToState(CasterEnemyState.Casting);
            else if (stateStr == "Dead")
                TransitionToState(CasterEnemyState.Dead);
            else
                Debug.LogWarning("Estado no reconocido en ChangeState de CasterEnemyAI.");
        }
    }

    /// <summary>
    /// Aplica daño al enemigo, considerando las resistencias elementales.
    /// </summary>
    public void TakeDamage(int damage, Vector3 hitPosition, string damageSource)
    {
        if (enemyData.isDead || enemyData.isStunned) return;

        float finalDamage = damage;
        switch (damageSource)
        {
            case "Pyro":
                finalDamage *= (1f - enemyData.pyroResistance);
                break;
            case "Aqua":
                finalDamage *= (1f - enemyData.aquaResistance);
                break;
            case "Geo":
                finalDamage *= (1f - enemyData.geoResistance);
                break;
            case "Ventus":
                finalDamage *= (1f - enemyData.ventusResistance);
                break;
        }
        int dmgRounded = Mathf.RoundToInt(finalDamage);
        enemyData.currentHealth -= dmgRounded;
        enemyData.knockbackDirection = (transform.position - hitPosition).normalized;
        Debug.Log($"{damageSource} aplica {dmgRounded} de daño a {gameObject.name}");

        if (enemyData.currentHealth <= 0)
        {
            TransitionToState(CasterEnemyState.Dead);
        }
        else if (enemyData.isStunned)
        {
            TransitionToState(CasterEnemyState.Hit);
        }
    }

    /// <summary>
    /// Devuelve la Transform del enemigo para que otros sistemas puedan referenciarlo.
    /// </summary>
    public Transform EnemyTransform => transform;

    #endregion

    #region Gizmos for Debugging

    /// <summary>
    /// Dibuja en la escena los rangos de detección y ataque, así como una línea al objetivo, para facilitar el debuggeo.
    /// </summary>
    private void OnDrawGizmosSelected()
    {
        if (enemyData == null) return;

        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, enemyData.detectionRange);
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.position, enemyData.attackRange);

        if (enemyData.targetTransform != null)
        {
            Gizmos.color = Color.magenta;
            Gizmos.DrawLine(transform.position, enemyData.targetTransform.position);
        }
    }
    #endregion
}
