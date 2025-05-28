using System;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class TowerAI : StateManager<TowerState>
{
    [Header("Datos de la Torre")]
    [Tooltip("Configuración general de la torre (vida, disparo, detección, etc.).")]
    public TowerData towerData;

    [Header("Efectos y Núcleo")]
    [Tooltip("EssenceSO activo que se obtiene de la poción insertada (a través del script Flask).")]
    public EssenceSO activeEssence;
    public AudioSource audioSource;
    
    [Tooltip("Referencia al transform que representa el núcleo de la torre (punto desde el cual se aplican los efectos/disparo).")]
    public Transform corePosition;

    // ──────────────────────────────────────────────────────────────
    // NUEVO: evento que notifica el estado de la vida (0-1)
    public event Action<float> OnHealthChanged;
    // ──────────────────────────────────────────────────────────────

    private void Awake()
    {
        // Inicializa la vida actual con el maxHealth configurado
        if (towerData != null)
            towerData.currentHealth = towerData.maxHealth;

        // Inicializa la state machine con todos los estados
        States = new Dictionary<TowerState, BaseState<TowerState>>
        {
            { TowerState.Idle, new TowerIdleState(this) },
            { TowerState.Charging, new TowerChargingState(this) },
            { TowerState.Active, new TowerActiveState(this) },
            { TowerState.Cooldown, new TowerCooldownState(this) },
            { TowerState.Destroyed, new TowerDestroyedState(this) }
        };

        // Estado inicial: esperando la poción
        CurrentState = States[TowerState.Idle];

        // Lanza el evento para que las UIs arranquen con el valor correcto
        float initNormalized = (float)towerData.currentHealth / towerData.maxHealth;
        OnHealthChanged?.Invoke(initNormalized);
    }

    /// <summary>
    /// Inserta la esencia procedente de una poción.
    /// • Si la torre ya tenía otra esencia, se reemplaza.
    /// • Se vuelve al estado Charging para reiniciar el ciclo
    ///   (carga → activo → cooldown …) con la nueva esencia.
    /// </summary>
    public void InsertEssence(EssenceSO essence)
    {
        if (essence == null || CurrentState == States[TowerState.Destroyed])
            return;
        
        activeEssence = essence;
        TransitionToState(TowerState.Charging);
    }


    /// <summary>
    /// Busca el enemigo con tag "Enemy" más cercano dentro del radio de detección.
    /// </summary>
    public GameObject GetTargetEnemy()
    {
        Collider[] hits = Physics.OverlapSphere(transform.position, towerData.detectionRadius);
        GameObject closestEnemy = null;
        float minDistance = Mathf.Infinity;

        foreach (Collider hit in hits)
        {
            if (hit.CompareTag("Enemy"))
            {
                float distance = Vector3.Distance(transform.position, hit.transform.position);
                if (distance < minDistance)
                {
                    minDistance = distance;
                    closestEnemy = hit.gameObject;
                }
            }
        }
        return closestEnemy;
    }


    /// <summary>
    /// Resta daño a la torre. Si la vida llega a 0 o menos, transita al estado Destroyed.
    /// </summary>
    public void TakeDamage(int damage)
    {
        towerData.currentHealth -= damage;

        // Notificar a TODAS las UIs registradas
        float normalized = Mathf.Clamp01((float)towerData.currentHealth / towerData.maxHealth);
        OnHealthChanged?.Invoke(normalized);

        if (towerData.currentHealth <= 0)
        {
            Debug.Log("La torre ha sido destruida");
            TransitionToState(TowerState.Destroyed);
        }
    }

    /// <summary>
    /// Dibuja en el editor el radio de detección de la torre.
    /// </summary>
    private void OnDrawGizmos()
    {
        // Dibuja la esfera de detección
        if (towerData != null)
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(transform.position, towerData.detectionRadius);
        }
    
        // Dibuja un rayo indicando el objetivo actual, si existe
        GameObject target = GetTargetEnemy();
        if (target != null)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawLine(transform.position, target.transform.position);
        }
    }
}