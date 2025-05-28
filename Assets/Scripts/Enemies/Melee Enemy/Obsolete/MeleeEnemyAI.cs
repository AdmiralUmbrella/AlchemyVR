using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using Unity.VisualScripting;
using UnityEngine;

public class MeleeEnemyAI : StateManager<MeleeEnemyStates>, IEnemy
{
    public MeleeEnemyData enemyData;
    public event Action<MeleeEnemyAI> OnEnemyDestroyed; 
    
    void Awake()
    {
        FindAndSortWaypointsByTag("Waypoint");
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
            { MeleeEnemyStates.Dead, new MeleeEnemyDeadState(MeleeEnemyStates.Dead, this, enemyData) },
            { MeleeEnemyStates.Patrol, new MeleeEnemyPatrolState(MeleeEnemyStates.Patrol, this, enemyData) }
        };

        CurrentState = States[MeleeEnemyStates.Patrol];
    }

    // Nuevo método: detecta la torre mediante OverlapSphere dentro del rango dado.
    public bool IsTowerWithinAttackRange(float range)
    {
        Collider[] hits = Physics.OverlapSphere(transform.position, range);
        foreach (Collider col in hits)
        {
            if (col.CompareTag("Tower"))
            {
                enemyData.tower = col.GetComponent<TowerAI>();
                // Asigna el transform del collider de la torre como objetivo.
                enemyData.playerTransform = col.transform;
                return true;
            }
        }
        return false;
    }

    // Este método se mantiene para detectar otros posibles objetivos (como el jugador),
    // pero se complementa con la detección por OverlapSphere para la torre.
    public bool CheckForTargetsInRange(float range)
    {
        if (IsTowerWithinAttackRange(range))
        {
            return true;
        }
        
        if (enemyData.playerTransform != null)
        {
            float playerDistance = Vector3.Distance(transform.position, enemyData.playerTransform.position);
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

    public void NotifyEnemyDestroyed()
    {
        OnEnemyDestroyed?.Invoke(this);
    }
    
    /// <summary>
    /// Busca todos los objetos con 'tagName' y los ordena según el número al final
    /// de su nombre, por ejemplo "Waypoint (1)", "Waypoint (2)", etc.
    /// </summary>
    private void FindAndSortWaypointsByTag(string tagName)
    {
        // Buscar todos los objetos con ese tag
        GameObject[] waypointsObjs = GameObject.FindGameObjectsWithTag(tagName);

        // Crear un patrón para extraer el número entre paréntesis al final
        // Ejemplo: "Waypoint (2)" -> extraemos '2'
        Regex regex = new Regex(@"\((\d+)\)$");

        // Ordenar usando una expresión LINQ
        var sortedWaypoints = waypointsObjs.OrderBy(obj =>
        {
            // Intentar extraer el número de su nombre
            // Si no se encuentra, poner un valor muy alto para que vaya al final
            Match match = regex.Match(obj.name);
            if (match.Success)
            {
                return int.Parse(match.Groups[1].Value);
            }
            else
            {
                return int.MaxValue; 
            }
        });

        // Guardar en la lista de waypoints del SummonerData como Transforms
        enemyData.waypoints = sortedWaypoints
            .Select(obj => obj.transform)
            .ToArray();

        // (Opcional) Resetear el índice a 0
        enemyData.currentWaypointIndex = 0;

        Debug.Log($"Waypoints encontrados y ordenados: {enemyData.waypoints.Length}");
    }

    
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
