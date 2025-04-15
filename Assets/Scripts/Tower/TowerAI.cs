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
    
    [Tooltip("Referencia al transform que representa el núcleo de la torre (punto desde el cual se aplican los efectos/disparo).")]
    public Transform corePosition;

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
    }

    /// <summary>
    /// Se llama cuando se inserta una poción en el transform receptor de la torre.
    /// Se extrae el EssenceSO del Flask y se almacena como activeEssence.
    /// </summary>
    /// <param name="essence">EssenceSO obtenido del objeto de poción.</param>
    public void InsertEssence(EssenceSO essence)
    {
        if (activeEssence == null && essence != null)
        {
            activeEssence = essence;
            TransitionToState(TowerState.Charging);
        }
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
        // Notificar al UI que se debe actualizar la barra de vida
        TowerHealthUI uiComponent = GetComponent<TowerHealthUI>();
        if (uiComponent != null)
        {
            uiComponent.UpdateHealthBar();
        }
        
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
