using UnityEngine;

public class EnemyDeadState : EnemyBaseState
{
    public EnemyDeadState(EnemyStateManager manager, EnemyData enemyData)
        : base(manager, enemyData) { }

    public override void Enter()
    {
        Debug.Log("Enemigo entró en estado: DEAD");

        // Marcar al enemigo como muerto
        enemyData.isDead = true;

        // Desactivar NavMeshAgent
        if (enemyData.agent != null)
        {
            enemyData.agent.enabled = false;
        }

        // Iniciar el temporizador de muerte
        enemyData.currentDeathTime = enemyData.deathDuration;

        // Activar animación de muerte
        if (enemyData.animator != null)
        {
            // Resetear triggers previos por seguridad
            enemyData.animator.ResetTrigger("Attack");
            enemyData.animator.SetBool("IsMoving", false);
            enemyData.animator.SetTrigger("Dead");
        }

        // Notificar a otros sistemas (por ejemplo, conteo de kills)
        NotifyKill();
    }

    public override void Update()
    {
        // Si no deseas destruir el GameObject, podrías omitir esto
        if (!enemyData.shouldDestroyOnDeath) return;

        // Reducir el tiempo hasta que el enemigo sea destruido
        enemyData.currentDeathTime -= Time.deltaTime;

        // Cuando se termina la animación, destruimos el objeto
        if (enemyData.currentDeathTime <= 0f)
        {
            Debug.Log("Destruyendo enemigo muerto.");
            GameObject.Destroy(manager.gameObject);
        }
    }

    public override void Exit()
    {
        // Realmente, este estado es “terminal”: no planeas salir de DeadState,
        // así que normalmente no hay lógica aquí.
    }

    /// <summary>
    /// Ejemplo de notificación de kills al “jugador”, 
    /// o algún sistema de puntuación.
    /// </summary>
    private void NotifyKill()
    {
        // Por ejemplo, si tu PlayerStateManager tiene un método para sumar kills

    }
}
