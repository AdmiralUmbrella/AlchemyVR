/// <summary>
/// Finite-state stages of the WaveManager.
/// </summary>
public enum WaveManagerStates
{
    Idle,
    PrepareWave,
    Spawning,
    InProgress,
    Cleanup
}