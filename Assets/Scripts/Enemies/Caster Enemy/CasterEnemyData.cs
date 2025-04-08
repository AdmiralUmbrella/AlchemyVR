using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Serialization;

/// <summary>
/// Contiene los parámetros y referencias del enemigo Caster, 
/// permitiendo configurar desde el Inspector todo lo relativo a la detección, ataque y animaciones.
/// </summary>
[System.Serializable]
public class CasterEnemyData
{
    #region Stats
    [Header("Stats")]
    [Tooltip("Vida máxima del enemigo.")]
    public int maxHealth = 100;

    [Tooltip("Vida actual del enemigo (se actualiza en tiempo de ejecución).")]
    [HideInInspector]
    public int currentHealth;
    #endregion

    #region Movement
    [Header("Movement")]
    [Tooltip("Velocidad base de desplazamiento del enemigo.")]
    public float moveSpeed = 3.5f;
    #endregion

    #region Detection & Attack
    [Header("Detection & Attack")]
    [Tooltip("Rango en el que el enemigo detecta al objetivo (torres o jugador).")]
    public float detectionRange = 15f;

    [Tooltip("Rango desde el cual se puede lanzar el hechizo.")]
    public float attackRange = 10f;
    #endregion

    #region Casting Settings
    [Header("Casting Settings")]
    [Tooltip("Tiempo (en segundos) que dura el casteo del hechizo.")]
    public float castingTime = 2f;

    [Tooltip("Cooldown entre lanzamientos, para evitar disparos constantes.")]
    public float attackCooldown = 3f;
    #endregion

    #region References
    [Header("References")]
    [Tooltip("Referencia al objetivo actual (torre o jugador).")]
    public Transform targetTransform;

    [Tooltip("Referencia al Animator para controlar las animaciones del enemigo.")]
    public Animator animator;

    [Tooltip("Referencia al NavMeshAgent para el control del movimiento.")]
    public NavMeshAgent agent;

    [Tooltip("Raíz del modelo (útil para buscar componentes en caso de faltar alguna referencia).")]
    public GameObject modelRoot;
    #endregion

    #region Defensive Settings
    [Header("Defensive Settings")]
    [Tooltip("Resistencia al fuego (Pyro). Valor entre 0 y 1.")]
    [Range(0f, 1f)]
    public float pyroResistance = 0f;

    [Tooltip("Resistencia al agua (Aqua). Valor entre 0 y 1.")]
    [Range(0f, 1f)]
    public float aquaResistance = 0f;

    [Tooltip("Resistencia a la tierra (Geo). Valor entre 0 y 1.")]
    [Range(0f, 1f)]
    public float geoResistance = 0f;
    
    [Tooltip("Resistencia al aire (Ventus). Valor entre 0 y 1.")]
    [Range(0f, 1f)]
    public float ventusResistance = 0f;
    #endregion

    #region Internal Variables
    [Header("Runtime Values (No modificar)")]
    [Tooltip("Tiempo restante de casteo.")]
    [HideInInspector]
    public float currentCastingTime;

    [Tooltip("Tiempo de cooldown actual.")]
    [HideInInspector]
    public float currentCooldown;

    [Tooltip("Indica si el enemigo está realizando el casteo del hechizo.")]
    [HideInInspector]
    public bool isCasting = false;

    [Tooltip("Indica si el enemigo está muerto.")]
    [HideInInspector]
    public bool isDead = false;

    [Tooltip("Indica si el enemigo está aturdido.")]
    [HideInInspector]
    public bool isStunned = false;

    [Tooltip("Duración (en segundos) del stun al recibir un golpe fuerte.")]
    [HideInInspector]
    public float hitStunDuration = 0.5f;

    [Tooltip("Tiempo restante de aturdimiento.")]
    [HideInInspector]
    public float currentHitStunTime;

    [Tooltip("Dirección del knockback al ser golpeado.")]
    [HideInInspector]
    public Vector3 knockbackDirection;
    #endregion
}
