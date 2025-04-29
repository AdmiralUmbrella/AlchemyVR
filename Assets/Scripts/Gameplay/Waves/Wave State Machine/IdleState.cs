using UnityEngine;

/// <summary>Estado inicial: espera a que el juego pida la siguiente oleada.</summary>
public class IdleState : BaseState<WaveManagerStates>
{
    private readonly WaveManager mgr;

    public IdleState(WaveManager mgr) : base(WaveManagerStates.Idle)
        => this.mgr = mgr;

    public override void EnterState()  { Debug.Log("Comenzando en oleada: " + mgr.CurrentWaveIndex); }
    public override void ExitState()   { }
    public override void UpdateState() { }  // Lógica externa iniciará StartNextWave()

    public override WaveManagerStates GetNextState() =>
        mgr.CurrentWaveIndex >= 0 ? WaveManagerStates.PrepareWave : WaveManagerStates.Idle;

    /* Triggers no usados en este estado */
    public override void OnTriggerEnter(Collider _) { }
    public override void OnTriggerStay(Collider _)  { }
    public override void OnTriggerExit(Collider _)  { }
}