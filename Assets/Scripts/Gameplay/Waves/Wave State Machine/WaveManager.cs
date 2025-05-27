using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using TMPro;
using Random = UnityEngine.Random;

/// <summary>
/// Coordinates waves, communicates with UI and other systems via events.
/// Plug a reference to an array of WaveDefinition assets in the Inspector.
/// </summary>
public class WaveManager : StateManager<WaveManagerStates>
{
    [Header("Wave Data")]
    [Tooltip("Ordered list of handcrafted waves. After the last one difficulty scales procedurally.")]
    public WaveDefinition[] waves;

    [Header("Spawn Points")]
    [Tooltip("Possible locations to instantiate enemies.")]
    public Transform[] spawnPoints;

    [Header("Difficulty Scaling (after last handcrafted wave)")]
    [Tooltip("Extra enemies per wave beyond the last handcrafted one.")]
    public int extraEnemiesPerWave = 2;

    [Header("UI")]
    [Tooltip("Text that displays the countdown for the next wave.")]
    public TextMeshProUGUI warmupCountdownText; 
    public TextMeshProUGUI warmupCountdownIndicator;
    public GameObject startNextWaveButton;
    

    public static event Action<int> OnWaveStarted;
    public static event Action<int> OnWaveCompleted;

    /* ---------- Runtime ---------- */
    internal int CurrentWaveIndex { get; private set; } = -1;
    internal List<GameObject> aliveEnemies = new();

    /* ---------- MonoBehaviour ---------- */
    protected void Start()
    {
        /* Build and register states */
        States.Add(WaveManagerStates.Idle,       new IdleState(this));
        States.Add(WaveManagerStates.PrepareWave,new PrepareWaveState(this));
        States.Add(WaveManagerStates.Spawning,   new SpawningState(this));
        States.Add(WaveManagerStates.InProgress, new InProgressState(this));
        States.Add(WaveManagerStates.Cleanup,    new CleanupState(this));

        CurrentState = States[WaveManagerStates.Idle]; 
    }

    /* ---------- API ---------- */
    public void StartNextWave()
    {
        CurrentWaveIndex++;
        OnWaveStarted?.Invoke(CurrentWaveIndex + 1);
    }

    public void RaiseWaveCompleted()
    {
        CurrentWaveIndex++;
        OnWaveCompleted?.Invoke(CurrentWaveIndex + 1);
    }
    
    public void SkipWarmup()
    {
        // Solo tiene efecto si estamos en la cuenta regresiva
        if (CurrentState is PrepareWaveState prep)
            prep.Skip();                          // delegamos en el estado
    }
    
    public WaveDefinition GetCurrentDefinition()
    {
        if (CurrentWaveIndex < waves.Length)
            return waves[CurrentWaveIndex];

        // Procedural wave: reuse last definition but boost numbers
        WaveDefinition last = waves[^1];
        // Create a lightweight copy in memory
        WaveDefinition copy = ScriptableObject.CreateInstance<WaveDefinition>();
        copy.enemies     = last.enemies;
        copy.warmupTime  = last.warmupTime;
        copy.spawnInterval = last.spawnInterval;

        int bonus = extraEnemiesPerWave * ((CurrentWaveIndex + 1) - waves.Length);
        // Increase counts
        for (int i = 0; i < copy.enemies.Length; i++)
            copy.enemies[i].count += bonus;

        return copy;
    }
}
