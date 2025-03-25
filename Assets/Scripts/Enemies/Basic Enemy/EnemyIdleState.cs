using UnityEngine;

/// <summary>
/// Estado donde el enemigo espera (Idle). Revisa periódicamente
/// si hay un objetivo a perseguir.
/// </summary>
public class EnemyIdleState : EnemyBaseState
{
    // Timer para verificar la detección (para no hacerlo cada frame)
    private float checkTimer;

    /// <summary>
    /// Constructor que llama al base (EnemyBaseState).
    /// </summary>
    /// <param name="manager">El StateManager que maneja la FSM.</param>
    /// <param name="enemyData">Datos del enemigo (vida, animador, etc.).</param>
    public EnemyIdleState(EnemyStateManager manager, EnemyData enemyData)
        : base(manager, enemyData) { }

    /// <summary>
    /// Se llama cuando entramos en estado Idle.
    /// Configuramos al enemigo para que deje de moverse y reseteamos valores.
    /// </summary>
    public override void Enter()
    {
        Debug.Log("Enemigo entró en estado: IDLE");

        // Detener el NavMeshAgent
        manager.StopAgent();

        // Cambiar animaciones (por ejemplo, dejar de caminar)
        if (enemyData.animator != null)
        {
            enemyData.animator.SetBool("IsMoving", false);
        }

        // Reiniciar el checkTimer
        checkTimer = 0f;
    }

    /// <summary>
    /// Se llama cada frame mientras el enemigo está en Idle.
    /// Revisamos si hay un objetivo en rango de detección.
    /// </summary>
    public override void Update()
    {
        // Reducimos el contador
        checkTimer -= Time.deltaTime;

        // Cuando se acabe el tiempo, revisamos la detección
        if (checkTimer <= 0f)
        {
            // Lo volvemos a setear para la próxima revisión
            checkTimer = enemyData.idleCheckInterval;

            // Revisar si el jugador/tower está en rango
            if (IsPlayerInRange())
            {
                // Cambiamos al estado de persecución
                manager.ChangeState(manager.ChaseState);
            }
        }
    }

    /// <summary>
    /// Se llama al salir de este estado Idle.
    /// </summary>
    public override void Exit()
    {
        Debug.Log("Enemigo saliendo de estado: IDLE");
    }

    /// <summary>
    /// Método auxiliar para verificar si el jugador/tower está en rango.
    /// </summary>
    /// <returns>True si hay un objetivo en rango, false si no.</returns>
    private bool IsPlayerInRange()
    {
        // Si no tenemos referencia al objetivo, no hacemos nada
        if (enemyData.playerTransform == null) return false;

        // Consultamos al manager si el objetivo está en detectionRange
        return manager.IsPlayerInRange(enemyData.detectionRange);
    }
}
