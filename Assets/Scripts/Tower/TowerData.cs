using UnityEngine;

[System.Serializable]
public class TowerData
{
    [Header("Vida de la Torre")]
    [Tooltip("Vida máxima que puede tener la torre.")]
    public int maxHealth = 100;
    
    [Tooltip("Vida actual de la torre, se inicializa al comenzar el juego.")]
    public int currentHealth;

    [Header("Parámetros de Disparo")]
    [Tooltip("Intervalo de tiempo entre disparos en estado activo.")]
    public float fireInterval = 1f;
    
    [Tooltip("Duración en segundos en que la torre permanece en estado activo (disparando).")]
    public float activeDuration = 5f;
    
    [Tooltip("Tiempo en segundos para recargar la torre después de disparar.")]
    public float cooldownDuration = 3f;

    [Header("Detección")]
    [Tooltip("Radio de detección para buscar enemigos (se usa en OverlapSphere).")]
    public float detectionRadius = 15f;

    [Header("Animación y Efectos")]
    [Tooltip("Referencia al Animator que controla las animaciones específicas de la torre (cada torre tiene su propio Animator).")]
    public Animator towerAnimator;
    public Animator towerCrystal;
    
    [Header("Visual Projectile")]
    public ProjectilePool projectilePool;   // arrástralo en el inspector
    public float projectileSpeed = 10f;     // editable por torre

}