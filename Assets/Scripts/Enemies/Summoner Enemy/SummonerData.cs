using UnityEngine;
using System.Collections.Generic;
using UnityEngine.Serialization;

/// <summary>
/// Contiene la información básica y parámetros configurables
/// del Summoner, siguiendo la estructura de EnemyData.
/// </summary>
[System.Serializable]
public class SummonerData
{
    /*--------------------------------------------------------------------------------------------------------
     * Estadísticas generales (Stats)
     *--------------------------------------------------------------------------------------------------------*/
    [Header("Stats")]
    [Tooltip("Vida máxima del summoner.")]
    public int maxHealth = 100;
    [HideInInspector] public int currentHealth;

    [Tooltip("Velocidad base de desplazamiento.")]
    public float moveSpeed = 3.5f;

    [Tooltip("Rango principal de detección/invocación del Summoner.")]
    public float detectionRange = 10f;

    [Tooltip("Distancia a la que el Summoner deja de perseguir.")]
    public float stopChaseDistance = 15f;

    /*--------------------------------------------------------------------------------------------------------
     * Referencias a componentes
     *--------------------------------------------------------------------------------------------------------*/
    [Header("References")]
    [Tooltip("Raíz del modelo, útil para buscar el Animator si no se asigna.")]
    public GameObject modelRoot;

    [Tooltip("Animator principal del Summoner (para Idle, Summon, Hit, Dead).")]
    public Animator animator;

    [HideInInspector] public Transform targetTransform; // A quién persigue / ve

    /*--------------------------------------------------------------------------------------------------------
     * Parámetros para Idle / Chase
     *--------------------------------------------------------------------------------------------------------*/
    [Header("Idle / Chase Settings")]
    [Tooltip("Cada cuántos segundos revisamos condiciones en Idle.")]
    public float idleCheckInterval = 0.5f;

    [Tooltip("Cada cuántos segundos actualizamos el camino en la persecución.")]
    public float pathUpdateInterval = 0.2f;

    /*--------------------------------------------------------------------------------------------------------
     * Parámetros de Invocación (Summon)
     *--------------------------------------------------------------------------------------------------------*/
    [Header("Summon Settings")]
    [Tooltip("Prefab del enemigo básico que invoca el Summoner.")]
    public GameObject basicEnemyPrefab;

    [Tooltip("Máximo de enemigos invocados a la vez.")]
    public int maxSummonedEnemies = 5;

    [Tooltip("Cooldown (segundos) entre invocaciones.")]
    public float summonCooldown = 5f;

    [Tooltip("Distancia a la cual el summoner comienza a invocar.")]
    public float summonRange = 10f;

    // Temporizador para saber cuándo puede invocar otra vez
    [HideInInspector] public float currentSummonTimer;

    // Lista de enemigos invocados y vivos
    [HideInInspector] public List<GameObject> summonedEnemies = new List<GameObject>();

    /*--------------------------------------------------------------------------------------------------------
     * Parámetros de impacto (Hit State) / Knockback
     *--------------------------------------------------------------------------------------------------------*/
    [Header("Hit Settings")]
    [Tooltip("Duración en segundos del aturdimiento (stun) cuando recibe un golpe fuerte.")]
    public float stunDuration = 0.5f;
    [Tooltip("Fuerza del knockback cuando el summoner es golpeado.")]
    public float knockbackForce = 3f;
    [Tooltip("Si el summoner puede o no ser aturdido.")]
    public bool canBeStunned = true;

    [HideInInspector] public bool isStunned;
    [HideInInspector] public float currentStunTime;
    [HideInInspector] public Vector3 knockbackDirection;

    /*--------------------------------------------------------------------------------------------------------
     * Parámetros de muerte
     *--------------------------------------------------------------------------------------------------------*/
    [Header("Death Settings")]
    [Tooltip("Duración de la animación de muerte.")]
    public float deathDuration = 2f;

    [Tooltip("Si al terminar la animación de muerte se destruye el GameObject.")]
    public bool shouldDestroyOnDeath = true;
    
    [Tooltip("GameObject que contiene el VFX de muerte del enemigo.")]
    public GameObject deathVFX;

    [HideInInspector] public bool isDead = false;
    [HideInInspector] public float currentDeathTime;

    /*--------------------------------------------------------------------------------------------------------
     * Resistencias Elementales (igual que EnemyData)
     *--------------------------------------------------------------------------------------------------------*/
    [Header("Elemental Resistances\n(Valores negativos = vulnerable)")]
    [Tooltip("Resistencia al Fuego (Pyro). Reduce el daño de ataques de fuego.")]
    [Range(0f, 1f)]
    public float pyroResistance = 0f;

    [Tooltip("Resistencia al Agua (Aqua). Reduce el daño de ataques de agua.")]
    [Range(0f, 1f)]
    public float aquaResistance = 0f;

    [Tooltip("Resistencia a la Tierra (Geo). Reduce el daño de ataques de tierra.")]
    [Range(0f, 1f)]
    public float geoResistance = 0f;
    
    [Tooltip("Resistencia al Aire (Ventus). Reduce el daño de ataques de aire.")]
    [Range(0f, 1f)]
    public float ventusResistance = 0f;
    
    /*--------------------------------------------------------------------------------------------------------
     * Waypoints
     *--------------------------------------------------------------------------------------------------------*/
    [Header("Patrol (Waypoints)")]
    public Transform[] waypoints;            // Asignar en el Inspector
    [HideInInspector] public int currentWaypointIndex = 0;
    public float waypointArriveThreshold = 1f; 

}
