using UnityEngine;

/// <summary>La oleada está activa; espera a que todos los enemigos mueran.</summary>
public class InProgressState : BaseState<WaveManagerStates>
{
    private readonly WaveManager mgr;

    public InProgressState(WaveManager mgr) : base(WaveManagerStates.InProgress)
        => this.mgr = mgr;

    public override void UpdateState()
    {
        mgr.aliveEnemies.RemoveAll(e => e == null);
        if (mgr.aliveEnemies.Count == 0)
            mgr.TransitionToState(WaveManagerStates.Cleanup);
    }

    public override void EnterState() { }
    public override void ExitState()  { }

    public override WaveManagerStates GetNextState() => WaveManagerStates.InProgress;

    public override void OnTriggerEnter(Collider _) { }
    public override void OnTriggerStay(Collider _)  { }
    public override void OnTriggerExit(Collider _)  { }
}