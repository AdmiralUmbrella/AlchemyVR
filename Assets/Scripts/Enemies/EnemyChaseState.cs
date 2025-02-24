using UnityEngine;

public class EnemyChaseState : EnemyBaseState
{
    // Temporizador para actualizar la ruta de forma periódica
    private float pathUpdateTimer;

    /// <summary>
    /// Constructor que llama al base (EnemyBaseState).
    /// </summary>
    public EnemyChaseState(EnemyStateManager manager, EnemyData enemyData)
        : base(manager, enemyData) { }

    /// <summary>
    /// Se llama al entrar en el estado de persecución (Chase).
    /// Activa el movimiento del enemigo y configura la animación de caminar.
    /// </summary>
    public override void Enter()
    {
        Debug.Log("Enemigo entró en estado: CHASE");

        // Reanudar el movimiento (desbloqueamos NavMeshAgent)
        manager.ResumeAgent();

        // Inicializamos el temporizador para la ruta
        pathUpdateTimer = 0f;

        // Ajustar la animación a "en movimiento"
        if (enemyData.animator != null)
        {
            enemyData.animator.SetBool("IsMoving", true);
        }

        // Pedir una ruta inicial hacia el objetivo
        UpdateChaseTarget();
    }

    /// <summary>
    /// Se llama en cada frame mientras el enemigo está en estado Chase.
    /// Verifica la distancia al objetivo para decidir si ataca, sigue persiguiendo o regresa a Idle.
    /// </summary>
    public override void Update()
    {
        // Si no tenemos referencia al jugador/torre, volvemos a Idle
        if (enemyData.playerTransform == null)
        {
            manager.ChangeState(manager.IdleState);
            return;
        }

        // Actualizamos el path cada cierto intervalo para ahorrar recursos
        pathUpdateTimer -= Time.deltaTime;
        if (pathUpdateTimer <= 0f)
        {
            pathUpdateTimer = enemyData.pathUpdateInterval;
            UpdateChaseTarget();
        }

        // Calculamos la distancia al objetivo
        float distanceToPlayer = Vector3.Distance(
            enemyData.agent.transform.position,
            enemyData.playerTransform.position
        );

        // Si está demasiado lejos, vuelve a Idle (se sale de rango de persecución)
        if (distanceToPlayer > enemyData.stopChaseDistance)
        {
            Debug.Log("Objetivo muy lejos, volviendo a IDLE");
            manager.ChangeState(manager.IdleState);
            return;
        }

        // Si está dentro del rango de ataque y no hay cooldown, pasamos a Attack
        bool inAttackRange = (distanceToPlayer <= enemyData.attackRange);
        bool canAttackNow = (enemyData.attackCooldownTimer <= 0);

        if (inAttackRange && canAttackNow)
        {
            manager.ChangeState(manager.AttackState);
            return;
        }
    }

    /// <summary>
    /// Se llama al salir de este estado (Chase).
    /// Detiene la animación de caminar, si corresponde.
    /// </summary>
    public override void Exit()
    {
        Debug.Log("Enemigo saliendo de estado: CHASE");

        // Detener la animación de "correr" o "moverse"
        if (enemyData.animator != null)
        {
            enemyData.animator.SetBool("IsMoving", false);
        }

        // (No detenemos el agente aquí, porque el siguiente estado podría necesitarlo activo)
    }

    /// <summary>
    /// Actualiza la ruta del enemigo hacia la posición del objetivo.
    /// </summary>
    private void UpdateChaseTarget()
    {
        // Verificamos que tengamos el NavMeshAgent y la posición del objetivo
        if (enemyData.agent != null && enemyData.playerTransform != null)
        {
            // Ordenar al agente recalcular su camino al objetivo
            enemyData.agent.SetDestination(enemyData.playerTransform.position);

            // (Opcional) dibujar línea para debug
            Debug.DrawLine(
                enemyData.agent.transform.position,
                enemyData.playerTransform.position,
                Color.red,
                enemyData.pathUpdateInterval
            );
        }
    }
}
