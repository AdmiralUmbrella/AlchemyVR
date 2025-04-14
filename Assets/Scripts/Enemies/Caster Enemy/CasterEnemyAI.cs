using System;
using UnityEngine;
using UnityEngine.AI;
using System.Collections.Generic;
using Unity.VisualScripting;

/// <summary>
/// Controla la máquina de estados del CasterEnemy.
/// </summary>
public class CasterEnemyAI : StateManager<CasterEnemyState>, IEnemy
{
    [Header("Caster Enemy Data")]
    public CasterEnemyData enemyData;

    public event Action<CasterEnemyAI> OnEnemyDestroyed; 

    private void Awake()
    {
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

        // Animator
        if (enemyData.animator == null && enemyData.modelRoot != null)
        {
            enemyData.animator = enemyData.modelRoot.GetComponent<Animator>();
        }
        if (enemyData.animator == null)
        {
            enemyData.animator = GetComponentInChildren<Animator>();
        }

        // Registrar estados
        States = new Dictionary<CasterEnemyState, BaseState<CasterEnemyState>>
        {
            { CasterEnemyState.Idle, new CasterEnemyIdleState(CasterEnemyState.Idle, this, enemyData) },
            { CasterEnemyState.Patrol, new CasterEnemyPatrolState(CasterEnemyState.Patrol, this, enemyData) },
            { CasterEnemyState.Chase, new CasterEnemyChaseState(CasterEnemyState.Chase, this, enemyData) },
            { CasterEnemyState.Casting, new CasterEnemyCastingState(CasterEnemyState.Casting, this, enemyData) },
            { CasterEnemyState.Hit, new CasterEnemyHitState(CasterEnemyState.Hit, this, enemyData) },
            { CasterEnemyState.Dead, new CasterEnemyDeadState(CasterEnemyState.Dead, this, enemyData) }
        };

        // Estado inicial
        CurrentState = States[CasterEnemyState.Idle];
    }
    
    public bool IsTowerWithinAttackRange(float range)
    {
        Collider[] hits = Physics.OverlapSphere(transform.position, range);
        foreach (Collider col in hits)
        {
            if (col.CompareTag("Tower"))
            {
                enemyData.tower = col.GetComponent<TowerAI>();
                enemyData.targetTransform = col.transform;
                return true;
            }
        }
        return false;
    }

    // ReSharper disable Unity.PerformanceAnalysis
    /// <summary>
    /// Busca torres o jugador en rango; si hay, asigna enemyData.targetTransform.
    /// </summary>
    public bool CheckForTargetsInRange(float range)
    {
        Vector3 myPosition = transform.position;

        if (IsTowerWithinAttackRange(range))
        {
            return true;
        }

        // Buscar al jugador (tag = "Player")
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

        // Nada en rango
        enemyData.targetTransform = null;
        return false;
    }

    #region IEnemy Implementation
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
                Debug.LogWarning("Estado no reconocido en CasterEnemyAI.");
        }
    }

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

    public Transform EnemyTransform => transform;
    #endregion
    
    public void NotifyEnemyDead()
    {
        if (!enemyData.isDead)
        {
            enemyData.isDead = true;
            OnEnemyDestroyed?.Invoke(this); 
        }
    }
    
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
}
