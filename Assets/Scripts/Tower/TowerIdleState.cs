using UnityEngine;

public class TowerIdleState : BaseState<TowerState>
{
    private TowerAI towerManager;

    public TowerIdleState(TowerAI manager) : base(TowerState.Idle)
    {
        towerManager = manager;
    }

    public override void EnterState()
    {
        towerManager.towerData.towerCrystal.SetBool("IsCharging", false);
        Debug.Log("Torre en estado IDLE: esperando que se inserte una poción en el núcleo.");
    }

    public override void ExitState() { }
    public override void UpdateState() { }
    public override TowerState GetNextState() => TowerState.Idle;

    public override void OnTriggerEnter(Collider other) { }
    public override void OnTriggerStay(Collider other) { }
    public override void OnTriggerExit(Collider other) { }
}