using UnityEngine;

/// <summary>
/// Estado Dead del CasterEnemy. Se activa cuando el enemigo muere, ejecuta la animación de muerte y, 
/// tras un tiempo configurado, destruye el GameObject si así se desea.
/// </summary>
public class CasterEnemyDeadState : BaseState<CasterEnemyState>
{
    private CasterEnemyAI manager;
    private CasterEnemyData enemyData;

    /// <summary>
    /// Constructor del estado Dead.
    /// </summary>
    public CasterEnemyDeadState(CasterEnemyState stateKey, CasterEnemyAI manager, CasterEnemyData enemyData) : base(stateKey)
    {
        this.manager = manager;
        this.enemyData = enemyData;
    }

    public override void EnterState()
    {
        Debug.Log("CasterEnemy entró en estado: DEAD");
        enemyData.isDead = true;
        manager.NotifyEnemyDead(); 

        // Desactivar el agente de movimiento
        if (enemyData.agent != null)
        {
            enemyData.agent.enabled = false;
        }
        
        // Activar la animación de muerte
        if (enemyData.animator != null)
        {
            enemyData.animator.SetTrigger("Dead");
        }

        // Reiniciar el contador si se va a destruir el GameObject
        if (enemyData.shouldDestroyOnDeath)
        {
            enemyData.currentDeathTime = enemyData.deathDuration;
        }

        // Notificar a sistemas externos si es necesario (GameManager, etc.)
        // Por ejemplo: GameManager.Instance.EnemyDied(this);
    }

    public override void UpdateState()
    {
        // Si no se desea destruir tras la muerte, no se hace la cuenta regresiva
        if (!enemyData.shouldDestroyOnDeath) return;

        // Contar el tiempo de muerte
        enemyData.currentDeathTime -= Time.deltaTime;
        if (enemyData.currentDeathTime <= 0f)
        {
            // Aquí podrías soltar ítems o sumar puntos antes de destruir
            // Ejemplo: LootManager.Instance.SpawnLoot(manager.transform.position);
            // Ejemplo: ScoreManager.Instance.AddScore(50);

            // Finalmente, destruir el gameObject del enemigo
            GameObject.Destroy(manager.gameObject);
        }
    }

    public override CasterEnemyState GetNextState()
    {
        // Se permanece en Dead, es un estado terminal
        return CasterEnemyState.Dead;
    }

    public override void ExitState()
    {
        // Normalmente este estado no se abandona.
        // Pero si hubiera lógica de "resurrección" o "transformación", se podría manejar aquí.
    }

    public override void OnTriggerEnter(Collider other) { }
    public override void OnTriggerStay(Collider other) { }
    public override void OnTriggerExit(Collider other) { }
}
