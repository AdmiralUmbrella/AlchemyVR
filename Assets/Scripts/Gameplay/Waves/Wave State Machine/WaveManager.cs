using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using TMPro;
using Random = UnityEngine.Random;

/// <summary>
/// Coordinates waves, communicates with UI and other systems via events,
/// and now unlocks potion recipes as the player progresses.
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

    [Header("Potion Recipes")]
    [Tooltip("Recipe GameObjects to unlock per wave. Index 0 → Wave 1, etc.")]
    public GameObject[] potionRecipeObjects;

    public static event Action<int> OnWaveStarted;
    public static event Action<int> OnWaveCompleted;

    /* ---------- Runtime ---------- */
    internal int CurrentWaveIndex { get; private set; } = -1;
    internal List<GameObject> aliveEnemies = new();

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

    /// <summary>
    /// Called by UI or game logic to begin the next wave.
    /// </summary>
    public void StartNextWave()
    {
        CurrentWaveIndex++;
        ActivateRecipeForWave(CurrentWaveIndex);
        OnWaveStarted?.Invoke(CurrentWaveIndex + 1);
    }

    /// <summary>
    /// Called when a wave fully completes (all enemies dead, cleanup done).
    /// Advances the wave index so PrepareWave uses the next definition.
    /// </summary>
    public void RaiseWaveCompleted()
    {
        // <-- Re-add this increment so wave index advances properly
        CurrentWaveIndex++;
        OnWaveCompleted?.Invoke(CurrentWaveIndex + 1);
    }

    /// <summary>
    /// Skips the warmup countdown.
    /// </summary>
    public void SkipWarmup()
    {
        if (CurrentState is PrepareWaveState prep)
            prep.Skip();
        ActivateRecipeForWave(CurrentWaveIndex);
    }

    /// <summary>
    /// Returns the WaveDefinition for the current wave, scaling after handcrafted waves.
    /// </summary>
    public WaveDefinition GetCurrentDefinition()
    {
        if (CurrentWaveIndex < waves.Length)
            return waves[CurrentWaveIndex];

        // Procedural wave: clone last and boost counts
        WaveDefinition last = waves[^1];
        WaveDefinition copy = ScriptableObject.CreateInstance<WaveDefinition>();
        copy.enemies       = last.enemies;
        copy.warmupTime    = last.warmupTime;
        copy.spawnInterval = last.spawnInterval;

        int bonus = extraEnemiesPerWave * ((CurrentWaveIndex + 1) - waves.Length);
        for (int i = 0; i < copy.enemies.Length; i++)
            copy.enemies[i].count += bonus;

        return copy;
    }

    /// <summary>
    /// Activates the potion recipe object corresponding to the given wave.
    /// </summary>
    public void ActivateRecipeForWave(int waveIndex)
    {
        if (potionRecipeObjects == null) return;
        if (waveIndex >= 0 && waveIndex < potionRecipeObjects.Length)
            potionRecipeObjects[waveIndex].SetActive(true);
    }
}
