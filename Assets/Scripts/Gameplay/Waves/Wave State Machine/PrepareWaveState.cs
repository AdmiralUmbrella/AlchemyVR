using UnityEngine;

/// <summary>Cuenta atrás previa al spawn (muestra UI, sonido, etc.).</summary>
public class PrepareWaveState : BaseState<WaveManagerStates>
{
    private readonly WaveManager mgr;
    private float timer;

    public PrepareWaveState(WaveManager mgr) : base(WaveManagerStates.PrepareWave)
        => this.mgr = mgr;

    public override void EnterState()
    {
        Debug.Log("Preparando la oleada: " + mgr.CurrentWaveIndex);
        timer = mgr.GetCurrentDefinition().warmupTime;
        // Ej.: UIManager.Instance.StartCountdown(timer);
    }

    public override void UpdateState()
    {
        timer -= Time.deltaTime;
        if (timer <= 0f) mgr.TransitionToState(WaveManagerStates.Spawning);
    }

    public override void ExitState() { }

    public override WaveManagerStates GetNextState() => WaveManagerStates.PrepareWave;

    public override void OnTriggerEnter(Collider _) { }
    public override void OnTriggerStay(Collider _)  { }
    public override void OnTriggerExit(Collider _)  { }
}