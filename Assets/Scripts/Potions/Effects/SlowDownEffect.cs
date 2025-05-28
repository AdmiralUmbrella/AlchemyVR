using UnityEngine;
using UnityEngine.AI;
using System.Collections;
using System.Collections.Generic;

/// <summary>
/// Reduces a NavMeshAgent’s speed by a multiplier for a duration,
/// without stacking reductions on repeated hits.
/// </summary>
[CreateAssetMenu(menuName = "Alchemy/Effects/SlowDownEffect")]
public class SlowDownEffectSO : PotionEffectSO
{
    [Tooltip("Factor to multiply the agent’s original speed (e.g. 0.5 = 50%)")]
    public float speedMultiplier = 0.5f;
    [Tooltip("Duration of the slow effect")]
    public float duration = 2f;

    // Stores each agent’s original speed so we never stack multipliers
    private static Dictionary<NavMeshAgent, float> originalSpeeds = new();

    public override void ApplyEffect(IEnemy enemy, Vector3 hitPos)
    {
        var mb = enemy as MonoBehaviour;
        if (mb == null) return;

        var agent = mb.GetComponent<NavMeshAgent>();
        if (agent == null) return;

        mb.StartCoroutine(SlowRoutine(agent));
    }

    private IEnumerator SlowRoutine(NavMeshAgent agent)
    {
        // Record original speed once
        if (!originalSpeeds.ContainsKey(agent))
            originalSpeeds[agent] = agent.speed;

        // Apply slow based on original
        agent.speed = originalSpeeds[agent] * speedMultiplier;

        yield return new WaitForSeconds(duration);

        // Restore original speed and clean up
        if (originalSpeeds.TryGetValue(agent, out var original))
            agent.speed = original;
        originalSpeeds.Remove(agent);
    }
}