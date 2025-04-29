using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Serialization;

[System.Serializable]
public class CasterEnemyData
{
    #region Stats
    [Header("Stats")]
    public int maxHealth = 100;
    [HideInInspector] public int currentHealth;
    #endregion

    #region Movement
    [Header("Movement")]
    public float moveSpeed = 3.5f;
    #endregion

    #region Detection & Attack
    [Header("Detection & Attack")]
    public float detectionRange = 15f;
    public float attackRange = 10f;
    public int attackDamage = 10;
    public GameObject beamPrefab;      
    #endregion

    #region Casting Settings
    [Header("Casting Settings")]
    public float castingTime = 2f;
    public float attackCooldown = 3f;
    #endregion
    
    #region Death Settings
    [Header("Death Settings")]
    [Tooltip("Duración de la animación de muerte antes de destruir al enemigo (si shouldDestroyOnDeath es true).")]
    public float deathDuration = 2f;

    [Tooltip("Si es true, el enemigo se destruye automáticamente tras la animación de muerte.")]
    public bool shouldDestroyOnDeath = true;
    
    // Tiempo interno para el conteo regresivo
    [HideInInspector]
    public float currentDeathTime;
    #endregion
    
    #region References
    [Header("References")]
    public Transform targetTransform;
    public Animator animator;
    public NavMeshAgent agent;
    public GameObject modelRoot;
    public TowerAI tower;
    #endregion

    #region Defensive Settings
    [Header("Defensive Settings")]
    [Range(0f, 1f)] public float pyroResistance = 0f;
    [Range(0f, 1f)] public float aquaResistance = 0f;
    [Range(0f, 1f)] public float geoResistance = 0f;
    [Range(0f, 1f)] public float ventusResistance = 0f;
    #endregion

    #region Runtime Values (No modificar)
    [Header("Runtime Values (No modificar)")]
    [HideInInspector] public float currentCastingTime;
    [HideInInspector] public float currentCooldown;
    [HideInInspector] public bool isCasting = false;
    [HideInInspector] public bool isDead = false;
    [HideInInspector] public bool isStunned = false;
    [HideInInspector] public float hitStunDuration = 0.5f;
    [HideInInspector] public float currentHitStunTime;
    [HideInInspector] public Vector3 knockbackDirection;
    #endregion

    #region Patrolling
    [Header("Waypoints (Patrol)")]
    public Transform[] waypoints;
    [HideInInspector] public int currentWaypointIndex = 0;
    public float waypointArriveThreshold = 1.0f;
    public float pathUpdateInterval = 0.5f;
    #endregion
    
}
