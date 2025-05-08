using UnityEngine;

public class MeleeEnemyDeadState : BaseState<MeleeEnemyStates>
{
    private MeleeEnemyAI manager;
    private MeleeEnemyData enemyData;

    public MeleeEnemyDeadState(MeleeEnemyStates stateKey, MeleeEnemyAI manager, MeleeEnemyData enemyData) : base(stateKey)
    {
        this.manager = manager;
        this.enemyData = enemyData;
    }

    public override void EnterState()
    {
        Debug.Log("Enemigo entr� en estado: DEAD (Nuevo)");
        enemyData.isDead = true;
        if (enemyData.agent != null)
        {
            enemyData.agent.enabled = false;
        }
        enemyData.currentDeathTime = enemyData.deathDuration;
        if (enemyData.animator != null)
        {
            enemyData.animator.ResetTrigger("Attack");
            enemyData.animator.SetBool("IsMoving", false);
            enemyData.modelRoot.SetActive(false);
            enemyData.deathVFX.SetActive(true);
        }
        // Aqu� podr�as notificar a otros sistemas (por ejemplo, conteo de kills)
    }

    public override void ExitState() { }

    public override void UpdateState()
    {
        if (!enemyData.shouldDestroyOnDeath) return;
        enemyData.currentDeathTime -= Time.deltaTime;
        if (enemyData.currentDeathTime <= 0f)
        {
            Debug.Log("Destruyendo enemigo muerto (Nuevo).");
            manager.NotifyEnemyDestroyed();
            GameObject.Destroy(manager.gameObject);
        }
    }

    public override MeleeEnemyStates GetNextState()
    {
        return MeleeEnemyStates.Dead; // Estado terminal.
    }

    public override void OnTriggerEnter(Collider other) { }

    public override void OnTriggerStay(Collider other) { }

    public override void OnTriggerExit(Collider other) { }
}
