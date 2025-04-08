using System.Collections.Generic;
using UnityEngine;

public class MeleeEnemyAI : StateManager<MeleeEnemyStates>, IEnemy
{
    public MeleeEnemyData enemyData;

    void Awake()
    {
        // Asegurarse de que enemyData esté asignado
        if (enemyData == null)
        {
            enemyData = GetComponent<MeleeEnemyData>();
        }

        if (enemyData == null)
        {
            Debug.LogError("MeleeEnemyData no está asignado en MeleeEnemyAI.");
            return;
        }

        enemyData.currentHealth = enemyData.maxHealth;

        // Inicializar la máquina de estados usando la arquitectura reusable
        States = new Dictionary<MeleeEnemyStates, BaseState<MeleeEnemyStates>>
        {
            { MeleeEnemyStates.Idle, new MeleeEnemyIdleState(MeleeEnemyStates.Idle, this, enemyData) },
            { MeleeEnemyStates.Chase, new MeleeEnemyChaseState(MeleeEnemyStates.Chase, this, enemyData) },
            { MeleeEnemyStates.Attack, new MeleeEnemyAttackState(MeleeEnemyStates.Attack, this, enemyData) },
            { MeleeEnemyStates.Hit, new MeleeEnemyHitState(MeleeEnemyStates.Hit, this, enemyData) },
            { MeleeEnemyStates.Dead, new MeleeEnemyDeadState(MeleeEnemyStates.Dead, this, enemyData) }
        };

        CurrentState = States[MeleeEnemyStates.Idle];
    }

    public void StopAgent()
    {
        if (enemyData.agent != null)
        {
            enemyData.agent.isStopped = true;
            enemyData.agent.velocity = Vector3.zero;
        }
    }

    public void ResumeAgent()
    {
        if (enemyData.agent != null)
        {
            enemyData.agent.isStopped = false;
        }
    }

// Método auxiliar para verificar objetivos (Tower o Player)
    public bool CheckForTargetsInRange(float range)
    {
        GameObject[] towers = GameObject.FindGameObjectsWithTag("Tower");
        Transform nearestTower = null;
        float nearestDistance = Mathf.Infinity;
        Vector3 myPosition = transform.position;

        foreach (GameObject tower in towers)
        {
            float distance = Vector3.Distance(myPosition, tower.transform.position);
            if (distance <= range && distance < nearestDistance)
            {
                nearestDistance = distance;
                nearestTower = tower.transform;
            }
        }

        if (nearestTower != null)
        {
            enemyData.playerTransform = nearestTower;
            return true;
        }

        if (enemyData.playerTransform != null)
        {
            float playerDistance = Vector3.Distance(myPosition, enemyData.playerTransform.position);
            if (playerDistance <= range)
            {
                return true;
            }
        }

        return false;
    }

    #region Implementación IEnemy

    public void ChangeState(object newState)
    {
        if (newState is MeleeEnemyStates state)
        {
            TransitionToState(state);
        }
        else if (newState is string stateString)
        {
            // Interpretar el string para cambiar al estado correspondiente.
            if (stateString == "Hit")
                TransitionToState(MeleeEnemyStates.Hit);
            else if (stateString == "Idle")
                TransitionToState(MeleeEnemyStates.Idle);
            else if (stateString == "Chase")
                TransitionToState(MeleeEnemyStates.Chase);
            else if (stateString == "Attack")
                TransitionToState(MeleeEnemyStates.Attack);
            else if (stateString == "Dead")
                TransitionToState(MeleeEnemyStates.Dead);
            else
                Debug.LogWarning("Nuevo estado no reconocido para MeleeEnemyAI.");
        }
    }

    public void TakeDamage(int damage, Vector3 hitPosition, string damageSource)
    {
        // Si el enemigo ya está muerto o aturdido, se ignoran nuevos impactos
        if (enemyData.isDead || enemyData.isStunned) return;

        // Se aplica la resistencia elemental
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

        // Almacenar dirección del knockback
        enemyData.knockbackDirection = (transform.position - hitPosition).normalized;

        Debug.Log($"{damageSource} aplica {dmgRounded} de daño a {gameObject.name}");

        // Transición al estado de HIT o DEATH según corresponda
        if (enemyData.currentHealth <= 0)
        {
            TransitionToState(MeleeEnemyStates.Dead);
        }
        else if (enemyData.canBeStunned)
        {
            TransitionToState(MeleeEnemyStates.Hit);
        }
    }

    public Transform EnemyTransform => transform;

    #endregion

// ------------------ Visualización con Gizmos ------------------
    private void OnDrawGizmos()
    {
        if (enemyData != null)
        {
            // Rango de detección (Amarillo)
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(transform.position, enemyData.detectionRange);

            // Rango de ataque (Rojo)
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(transform.position, enemyData.attackRange);

            // Rango para detener la persecución (Verde)
            Gizmos.color = Color.green;
            Gizmos.DrawWireSphere(transform.position, enemyData.stopChaseDistance);
        }
    }
}