using UnityEngine;
using UnityEngine.AI;

/// <summary>
/// Esta clase gestiona la FSM del enemigo.
/// Aquí creamos las instancias de cada estado y controlamos el flujo entre ellos.
/// </summary>
public class EnemyStateManager : MonoBehaviour
{
    // Evento opcional para notificar muerte del enemigo a otros sistemas.
    public delegate void EnemyDeathHandler();
    public event EnemyDeathHandler OnEnemyDeath;

    // En este proyecto, podemos usar [SerializeField] para asignar o comprobar en Inspector.
    [SerializeField]
    private EnemyData enemyData;

    // Referencia al estado actual
    private EnemyBaseState currentState;

    // Referencias a los estados concretos
    // (Los instanciamos en Awake para que estén listos antes de Start)
    public EnemyIdleState IdleState { get; private set; }
    public EnemyChaseState ChaseState { get; private set; }
    public EnemyAttackState AttackState { get; private set; }
    public EnemyHitState HitState { get; private set; }
    public EnemyDeadState DeadState { get; private set; }

    private void Awake()
    {
        // Configurar componentes necesarios (NavMeshAgent, Animator, etc.)
        SetupComponents();

        // Crear instancias de los estados concretos, pasándoles 'this' y 'enemyData'
        IdleState = new EnemyIdleState(this, enemyData);
        ChaseState = new EnemyChaseState(this, enemyData);
        AttackState = new EnemyAttackState(this, enemyData);
        HitState = new EnemyHitState(this, enemyData);
        DeadState = new EnemyDeadState(this, enemyData);

        // Inicializar la salud actual
        enemyData.currentHealth = enemyData.maxHealth;
    }

    /// <summary>
    /// Aquí configuramos todos los componentes que el enemigo necesita,
    /// por ejemplo NavMeshAgent o Animator, en caso de no estar asignados.
    /// </summary>
    private void SetupComponents()
    {
        // Obtener el NavMeshAgent
        enemyData.agent = GetComponent<NavMeshAgent>();
        if (enemyData.agent != null)
        {
            enemyData.agent.speed = enemyData.moveSpeed;
            enemyData.agent.stoppingDistance = enemyData.attackRange;
        }

        // Configurar el animator si no está asignado en el Inspector
        if (enemyData.animator == null && enemyData.modelRoot != null)
        {
            enemyData.animator = enemyData.modelRoot.GetComponent<Animator>();
        }
    }

    private void Start()
    {
        // Buscar al "jugador" o a la "torre" que el enemigo atacará
        // (Puede ser un GameObject con tag "Player" o "Tower", según tu diseño)
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            enemyData.playerTransform = player.transform;
        }

        // Iniciar la FSM en estado Idle
        ChangeState(IdleState);
    }

    private void Update()
    {
        // Actualizar el cooldown del ataque, si el enemigo no está muerto
        if (enemyData.attackCooldownTimer > 0 && !enemyData.isDead)
        {
            enemyData.attackCooldownTimer -= Time.deltaTime;
        }

        // Actualizar el estado actual
        currentState?.Update();
    }

    /// <summary>
    /// Cambia el estado actual de la FSM.
    /// Llama el Exit() del estado anterior y el Enter() del nuevo estado.
    /// </summary>
    /// <param name="newState">El nuevo estado al que transicionamos.</param>
    public void ChangeState(EnemyBaseState newState)
    {
        currentState?.Exit();    // Salimos del estado anterior
        currentState = newState; // Actualizamos la referencia
        currentState?.Enter();   // Entramos al nuevo estado
    }

    /// <summary>
    /// Detener el NavMeshAgent para que el enemigo no se mueva (ej. al atacar).
    /// </summary>
    public void StopAgent()
    {
        if (enemyData.agent != null)
        {
            enemyData.agent.isStopped = true;
            enemyData.agent.velocity = Vector3.zero;
        }
    }

    /// <summary>
    /// Reanudar el movimiento del NavMeshAgent.
    /// </summary>
    public void ResumeAgent()
    {
        if (enemyData.agent != null)
        {
            enemyData.agent.isStopped = false;
        }
    }

    /// <summary>
    /// Verifica si el jugador/tower está dentro de un rango específico.
    /// </summary>
    public bool IsPlayerInRange(float range)
    {
        if (enemyData.playerTransform == null) return false;

        float distance = Vector3.Distance(
            transform.position,
            enemyData.playerTransform.position
        );
        return (distance <= range);
    }

    /// <summary>
    /// Método que recibe daño desde otras partes (pociones, armas, etc.).
    /// </summary>
    /// <param name="damage">Cantidad de daño a aplicar.</param>
    /// <param name="attackerPosition">Posición del atacante para calcular knockback.</param>
    public void TakeDamage(int damage, Vector3 attackerPosition)
    {
        // Si el enemigo ya está muerto o aturdido, ignorar
        if (enemyData.isDead || enemyData.isStunned) return;

        // Calcular la dirección del knockback
        enemyData.knockbackDirection = (transform.position - attackerPosition).normalized;

        // Aplicar daño
        enemyData.currentHealth -= damage;
        Debug.Log($"Enemigo recibió {damage} de daño. Vida actual: {enemyData.currentHealth}");

        // Verificar muerte
        if (enemyData.currentHealth <= 0)
        {
            Die();
        }
        else if (enemyData.canBeStunned)
        {
            // Si puede ser aturdido, cambiar al estado Hit
            ChangeState(HitState);
        }
        // Si no puede ser aturdido, podrías quedarte en el estado actual
    }

    /// <summary>
    /// Lógica para matar al enemigo.
    /// Emite el evento OnEnemyDeath e inicia el estado Dead.
    /// </summary>
    public void Die()
    {
        if (!enemyData.isDead)
        {
            OnEnemyDeath?.Invoke();  // Notificar a otros sistemas
            ChangeState(DeadState);
        }
    }

    // Dibujar esferas en la escena para visualizar rangos. Opcional.
    private void OnDrawGizmosSelected()
    {
        if (enemyData == null) return;

        // Rango de detección
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, enemyData.detectionRange);

        // Rango de ataque
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, enemyData.attackRange);

        // Rango de parada de persecución
        Gizmos.color = Color.blue;
        Gizmos.DrawWireSphere(transform.position, enemyData.stopChaseDistance);
    }
}
