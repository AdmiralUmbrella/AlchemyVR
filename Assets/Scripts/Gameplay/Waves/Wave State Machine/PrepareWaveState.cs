using UnityEngine;

/// <summary>Cuenta atrás previa al spawn (muestra UI, sonido, etc.).</summary>
public class PrepareWaveState : BaseState<WaveManagerStates>
{
    private readonly WaveManager mgr;
    private float timer;

    internal PrepareWaveState(WaveManager mgr) : base(WaveManagerStates.PrepareWave)
        => this.mgr = mgr;

    public override void EnterState()
    {
        
        
        timer = mgr.GetCurrentDefinition().warmupTime;

        if (mgr.warmupCountdownText != null)
        {
            mgr.warmupCountdownText.gameObject.SetActive(true);
            mgr.startNextWaveButton.gameObject.SetActive(true);
            mgr.warmupCountdownIndicator.gameObject.SetActive(true);
            mgr.warmupCountdownText.text = Mathf.CeilToInt(timer).ToString("00");
        }
    }

    public override void UpdateState()
    {
        timer -= Time.unscaledDeltaTime;                        // sigue contando aunque el juego esté en pausa
        if (mgr.warmupCountdownText)
            mgr.warmupCountdownText.text = Mathf.CeilToInt(timer).ToString("00");

        if (timer <= 0f)
            FinishCountdown();
    }

    /// <summary>Llamado desde WaveManager.SkipWarmup()</summary>
    internal void Skip() => FinishCountdown();

    private void FinishCountdown()
    {
        if (mgr.warmupCountdownText)
        {
            mgr.warmupCountdownText.gameObject.SetActive(false);
            mgr.startNextWaveButton.gameObject.SetActive(false);           
            mgr.warmupCountdownIndicator.gameObject.SetActive(false);
        }

        mgr.TransitionToState(WaveManagerStates.Spawning);
    }

    public override void ExitState() { }
    public override WaveManagerStates GetNextState() => WaveManagerStates.PrepareWave;
    
    public override void OnTriggerEnter(Collider _) { }
    public override void OnTriggerStay(Collider _)  { }
    public override void OnTriggerExit(Collider _)  { }
}