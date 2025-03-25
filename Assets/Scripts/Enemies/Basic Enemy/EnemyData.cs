using UnityEngine;
using UnityEngine.AI;

/// <summary>
/// Contiene la información básica y parámetros configurables
/// del enemigo, incluyendo referencias y valores de la jugabilidad.
/// </summary>
[System.Serializable]
public class EnemyData
{
    /*--------------------------------------------------------------------------------------------------------
     * Estadísticas generales (Stats)
     *--------------------------------------------------------------------------------------------------------*/
    [Header("Stats")]
    [Tooltip("Vida máxima del enemigo.")]
    public int maxHealth = 100;

    public int currentHealth;

    [Tooltip("Rango de detección para iniciar la persecución.")]
    public float detectionRange = 10f;

    [Tooltip("Rango de ataque para golpear al jugador.")]
    public float attackRange = 2f;

    [Tooltip("Velocidad base de desplazamiento del enemigo.")]
    public float moveSpeed = 3.5f;

    /*--------------------------------------------------------------------------------------------------------
     * Referencias a componentes
     *--------------------------------------------------------------------------------------------------------*/
    [Header("References")]
    [Tooltip("Referencia al transform del jugador/tower que el enemigo persigue o ataca.")]
    public Transform playerTransform;

    [Tooltip("Animator principal del enemigo (para sus animaciones de Idle, Attack, etc.).")]
    public Animator animator;

    [Tooltip("NavMeshAgent para el movimiento y pathfinding del enemigo.")]
    public NavMeshAgent agent;

    [Tooltip("Raíz del modelo, útil para buscar el Animator si no se asigna manualmente.")]
    public GameObject modelRoot;

    /*--------------------------------------------------------------------------------------------------------
     * Parámetros para la IA de Patrulla / Idle / Pathfinding
     *--------------------------------------------------------------------------------------------------------*/
    [Header("Path Finding")]
    [Tooltip("Cada cuántos segundos actualizamos el camino en la persecución.")]
    public float pathUpdateInterval = 0.2f;

    [Tooltip("Intervalo de tiempo para verificar condiciones en Idle.")]
    public float idleCheckInterval = 0.5f;

    [Tooltip("Distancia a la que el enemigo deja de perseguir (un poco mayor que el detectionRange).")]
    public float stopChaseDistance = 15f;

    [HideInInspector]
    public bool isAttacking = false;  // Bandera para saber si está en ataque

    /*--------------------------------------------------------------------------------------------------------
     * Parámetros de ataque
     *--------------------------------------------------------------------------------------------------------*/
    [Header("Attack Settings")]
    [Tooltip("Daño base que inflige el enemigo al jugador/tower.")]
    public int attackDamage = 10;

    [Tooltip("Duración total de la animación de ataque.")]
    public float attackDuration = 1f;

    [Tooltip("Momento (en segundos) dentro de la animación en el que se aplica el daño.")]
    public float damageDelay = 0.5f;

    [Tooltip("Tiempo de reutilización (cooldown) antes de volver a atacar.")]
    public float attackCooldown = 2f;

    // Campos internos para manejar el flujo del ataque
    [HideInInspector] public float currentAttackTime;
    [HideInInspector] public float attackCooldownTimer;
    [HideInInspector] public bool hasDealtDamage; // Para saber si ya se aplicó el daño durante la animación

    /*--------------------------------------------------------------------------------------------------------
     * Parámetros de impacto (Hit State)
     *--------------------------------------------------------------------------------------------------------*/
    [Header("Hit Settings")]
    [Tooltip("Duración en segundos del aturdimiento (stun) cuando recibe un golpe fuerte.")]
    public float hitStunDuration = 0.5f;

    [Tooltip("Fuerza del knockback cuando el enemigo es golpeado.")]
    public float knockbackForce = 3f;

    [Tooltip("Si el enemigo puede o no ser aturdido.")]
    public bool canBeStunned = true;

    // Variables internas
    [HideInInspector] public float currentHitStunTime;
    [HideInInspector] public Vector3 knockbackDirection;
    [HideInInspector] public bool isStunned;

    /*--------------------------------------------------------------------------------------------------------
     * Parámetros de muerte
     *--------------------------------------------------------------------------------------------------------*/
    [Header("Death Settings")]
    [Tooltip("Duración de la animación de muerte.")]
    public float deathDuration = 2f;

    [Tooltip("Si al terminar la animación de muerte se destruye el GameObject del enemigo.")]
    public bool shouldDestroyOnDeath = true;

    [HideInInspector] public float currentDeathTime;
    [HideInInspector] public bool isDead = false;

    /*--------------------------------------------------------------------------------------------------------
     * Resistencias Elementales
     *--------------------------------------------------------------------------------------------------------*/
    [Header("Elemental Resistances\n(Valores negativos hacen vulnerables al enemigo al tipo de daño)")]
    [Tooltip("Resistencia al Fuego (Pyro). Reduce el daño de ataques de fuego.")]
    public float pyroResistance = 0f;

    [Tooltip("Resistencia al Agua (Aqua). Reduce el daño de ataques de agua.")]
    public float aquaResistance = 0f;

    [Tooltip("Resistencia a la Tierra (Geo). Reduce el daño de ataques de tierra.")]
    public float geoResistance = 0f;

    [Tooltip("Resistencia al Aire (Aero). Reduce el daño de ataques de aire.")]
    public float aeroResistance = 0f;


}
