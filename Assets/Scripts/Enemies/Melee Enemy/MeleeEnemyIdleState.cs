using UnityEngine;

public class MeleeEnemyIdleState : BaseState<MeleeEnemyStates>
{
    private MeleeEnemyAI manager;
    private MeleeEnemyData enemyData;
    private float checkTimer;

    public MeleeEnemyIdleState(MeleeEnemyStates stateKey, MeleeEnemyAI manager, MeleeEnemyData enemyData) : base(stateKey)
    {
        this.manager = manager;
        this.enemyData = enemyData;
    }

    public override void EnterState()
    {
        Debug.Log("Enemigo entr� en estado: IDLE (Nuevo)");
        // Detener el agente
        if (enemyData.agent != null)
        {
            enemyData.agent.isStopped = true;
            enemyData.agent.velocity = Vector3.zero;
        }
        // Ajustar animaci�n
        if (enemyData.animator != null)
        {
            enemyData.animator.SetBool("IsMoving", false);
        }
        // Reiniciar temporizador
        checkTimer = 0f;
    }

    public override void ExitState()
    {
        Debug.Log("Enemigo saliendo de estado: IDLE (Nuevo)");
    }

    public override void UpdateState()
    {
        checkTimer -= Time.deltaTime;
        if (checkTimer <= 0f)
        {
            checkTimer = enemyData.idleCheckInterval;
            // Aqu� se podr�a actualizar l�gica interna (p.ej. animaciones o efectos)
        }
    }

    public override MeleeEnemyStates GetNextState()
    {
        // Si hay un objetivo en rango, pasar a Chase; de lo contrario, permanecer en Idle.
        if (manager.CheckForTargetsInRange(enemyData.detectionRange))
        {
            return MeleeEnemyStates.Chase;
        }
        return MeleeEnemyStates.Idle;
    }

    public override void OnTriggerEnter(Collider other) { }

    public override void OnTriggerStay(Collider other) { }

    public override void OnTriggerExit(Collider other) { }
}
