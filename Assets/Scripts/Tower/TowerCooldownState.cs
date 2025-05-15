using UnityEngine;
using System.Collections;

public class TowerCooldownState : BaseState<TowerState>
{
    private TowerAI towerManager;
    private float cooldownTimer;

    public TowerCooldownState(TowerAI manager) : base(TowerState.Cooldown)
    {
        towerManager = manager;
    }

    public override void EnterState()
    {
        Debug.Log("Torre en estado COOLDOWN: recargando.");
        towerManager.towerData.towerCrystal.ResetTrigger("Active");
        towerManager.towerData.towerCrystal.SetTrigger("Idle");
        cooldownTimer = towerManager.towerData.cooldownDuration;
    }

    public override void ExitState()
    {
        // Se resetea el Essence activo para permitir la inserción de otra poción
        towerManager.activeEssence = null;
    }

    public override void UpdateState()
    {
        cooldownTimer -= Time.deltaTime;
    }

    public override TowerState GetNextState()
    {
        if (cooldownTimer <= 0)
            return TowerState.Idle;
        return TowerState.Cooldown;
    }

    public override void OnTriggerEnter(Collider other) { }
    public override void OnTriggerStay(Collider other) { }
    public override void OnTriggerExit(Collider other) { }
}