using UnityEngine;
using System.Collections;

public class TowerChargingState : BaseState<TowerState>
{
    private TowerAI towerManager;
    private float chargeTime = 1f;

    public TowerChargingState(TowerAI manager) : base(TowerState.Charging)
    {
        towerManager = manager;
    }

    public override void EnterState()
    {
        Debug.Log("Torre en estado CHARGING: cargando poción.");
        towerManager.towerData.towerCrystal.SetTrigger("Charging");
        towerManager.StartCoroutine(ChargeRoutine());
    }

    private IEnumerator ChargeRoutine()
    {
        yield return new WaitForSeconds(chargeTime);
        towerManager.TransitionToState(TowerState.Active);
    }

    public override void ExitState() { }
    public override void UpdateState() { }
    public override TowerState GetNextState() => TowerState.Charging;
    public override void OnTriggerEnter(Collider other) { }
    public override void OnTriggerStay(Collider other) { }
    public override void OnTriggerExit(Collider other) { }
}