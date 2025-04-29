using UnityEngine;

/// <summary>Pequeña pausa tras limpiar la oleada (recompensas, FX, etc.).</summary>
public class CleanupState : BaseState<WaveManagerStates>
{
    private readonly WaveManager mgr;
    private float delay = 3f;   // Tiempo para recompensas o FX

    public CleanupState(WaveManager mgr) : base(WaveManagerStates.Cleanup)
        => this.mgr = mgr;

    public override void EnterState()
    {
        Debug.Log("Limpiando la oleada.");
        mgr.RaiseWaveCompleted();
        // Ej.: RewardManager.Instance.GrantLoot();
    }

    public override void UpdateState()
    {
        delay -= Time.deltaTime;
        if (delay <= 0f)
            mgr.TransitionToState(WaveManagerStates.PrepareWave);
    }

    public override void ExitState() { }

    public override WaveManagerStates GetNextState() => WaveManagerStates.Cleanup;

    public override void OnTriggerEnter(Collider _) { }
    public override void OnTriggerStay(Collider _)  { }
    public override void OnTriggerExit(Collider _)  { }
}
