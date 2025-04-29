using UnityEngine;
using System.Collections;

/// <summary>Instancia a los enemigos siguiendo la WaveDefinition.</summary>
public class SpawningState : BaseState<WaveManagerStates>
{
    private readonly WaveManager mgr;

    public SpawningState(WaveManager mgr) : base(WaveManagerStates.Spawning)
        => this.mgr = mgr;

    public override void EnterState() => mgr.StartCoroutine(SpawnRoutine());

    private IEnumerator SpawnRoutine()
    {
        var def = mgr.GetCurrentDefinition();

        foreach (var entry in def.enemies)
        {
            for (int i = 0; i < entry.count; i++)
            {
                Transform p = mgr.spawnPoints[Random.Range(0, mgr.spawnPoints.Length)];
                GameObject enemy = Object.Instantiate(entry.enemyPrefab, p.position, p.rotation);

                mgr.aliveEnemies.Add(enemy);
                yield return new WaitForSeconds(def.spawnInterval);
            }
        }

        mgr.TransitionToState(WaveManagerStates.InProgress);
    }

    public override void UpdateState() { }
    public override void ExitState() { }
    public override WaveManagerStates GetNextState() => WaveManagerStates.Spawning;

    public override void OnTriggerEnter(Collider _) { }
    public override void OnTriggerStay(Collider _)  { }
    public override void OnTriggerExit(Collider _)  { }
}