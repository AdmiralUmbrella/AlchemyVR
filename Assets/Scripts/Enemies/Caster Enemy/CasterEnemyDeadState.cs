using UnityEngine;

/// <summary>
/// Estado Dead del CasterEnemy. Se activa cuando el enemigo muere, ejecuta la animación de muerte y puede destruir el objeto.
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
        if (enemyData.agent != null)
        {
            enemyData.agent.enabled = false;
        }
        if (enemyData.animator != null)
        {
            enemyData.animator.SetTrigger("Dead");
        }
        // Aquí se pueden notificar otros sistemas (por ejemplo, GameManager) y ejecutar el cleanup
    }

    public override void UpdateState()
    {
        // Esperar a que la animación de muerte termine y luego destruir el objeto, si así se desea.
    }

    public override CasterEnemyState GetNextState()
    {
        return CasterEnemyState.Dead; // Estado terminal
    }

    public override void ExitState() { }
    public override void OnTriggerEnter(Collider other) { }
    public override void OnTriggerStay(Collider other) { }
    public override void OnTriggerExit(Collider other) { }
}