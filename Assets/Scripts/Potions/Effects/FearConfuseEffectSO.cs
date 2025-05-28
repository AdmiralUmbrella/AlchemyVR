using UnityEngine;
using UnityEngine.AI;
using System.Collections;

/// <summary>
/// 1) Desactiva temporalmente la FSM del enemigo para que no interfiera.
/// 2) Huye en línea recta “fleeDistance” metros desde el punto de impacto.
/// 3) Después vaga al azar dentro de “wanderRadius” hasta agotar “totalDuration”.
/// 4) Al terminar, reactiva la FSM en el último estado que tenía.
/// </summary>
[CreateAssetMenu(menuName = "Alchemy/Effects/FearConfuseEffect")]
public class FearConfuseEffectSO : PotionEffectSO
{
    [Header("Duraciones")]
    public float totalDuration = 5f;
    public float fleePhase    = 1.5f;

    [Header("Distancias")]
    public float fleeDistance   = 6f;
    public float wanderRadius   = 4f;
    public float wanderInterval = 0.6f;

    public override void ApplyEffect(IEnemy enemy, Vector3 hitPosition)
    {
        // Necesitamos MonoBehaviour para iniciar corrutina y desactivar la FSM
        var mb = enemy as MonoBehaviour;
        if (mb == null) return;

        var agent = mb.GetComponent<NavMeshAgent>();
        if (agent == null) return;

        // Buscar cuál script de FSM tiene este enemigo y desactivarlo
        MonoBehaviour fsm =
            mb.GetComponent<AdvancedMeleeEnemyAI>() ??
            (MonoBehaviour)mb.GetComponent<AdvancedCasterEnemyAI>()  ??
            mb.GetComponent<AdvancedSummonerEnemyAI>();

        if (fsm != null)
            fsm.enabled = false;

        // Arrancar flecha/confusión
        mb.StartCoroutine(FearConfuseRoutine(agent, mb.transform, hitPosition, fsm));
    }

    IEnumerator FearConfuseRoutine(
        NavMeshAgent agent,
        Transform tr,
        Vector3 origin,
        MonoBehaviour fsm
    )
    {
        float elapsed = 0f;

        // ── Fase 1: Huida ──
        Vector3 fleeDir = (tr.position - origin).normalized;
        if (fleeDir.sqrMagnitude < 0.01f)
            fleeDir = Vector3.back;  // fallback

        Vector3 fleeTarget = tr.position + fleeDir * fleeDistance;
        if (NavMesh.SamplePosition(fleeTarget, out var hit, 2f, agent.areaMask))
            agent.SetDestination(hit.position);

        agent.isStopped = false; // asegurarnos de que camine

        while (elapsed < fleePhase)
        {
            elapsed += Time.deltaTime;
            yield return null;
        }

        // ── Fase 2: Confusión / vagar ──
        while (elapsed < totalDuration)
        {
            // nuevo destino aleatorio
            Vector3 randomPoint = tr.position + Random.insideUnitSphere * wanderRadius;
            if (NavMesh.SamplePosition(randomPoint, out var wanderHit, 2f, agent.areaMask))
                agent.SetDestination(wanderHit.position);

            float timer = 0f;
            while (timer < wanderInterval)
            {
                elapsed += Time.deltaTime;
                timer  += Time.deltaTime;
                yield return null;
            }
        }

        // ── Fin del efecto: reactivar la FSM ──
        agent.isStopped = false;
        if (fsm != null)
            fsm.enabled = true;
    }
}
