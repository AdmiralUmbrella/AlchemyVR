using UnityEngine;
using UnityEngine.AI;
using System.Collections;

/// <summary>
/// 1) El enemigo huye en línea recta “fleeDistance” metros desde el punto de impacto.
/// 2) Después vaga al azar dentro de “wanderRadius” hasta agotar “totalDuration”.
/// </summary>
[CreateAssetMenu(menuName = "Alchemy/Effects/FearConfuseEffect")]
public class FearConfuseEffectSO : PotionEffectSO   // tu clase base :contentReference[oaicite:0]{index=0}:contentReference[oaicite:1]{index=1}
{
    [Header("Duraciones")]
    [Tooltip("Tiempo total del efecto (fase huida + fase confusión).")]
    public float totalDuration = 5f;
    [Tooltip("Cuánto tiempo dedica a huir antes de empezar a vagar.")]
    public float fleePhase   = 1.5f;

    [Header("Distancias")]
    [Tooltip("Distancia a la que intenta escapar inicialmente.")]
    public float fleeDistance  = 6f;
    [Tooltip("Radio en el que deambula una vez asustado.")]
    public float wanderRadius  = 4f;

    [Header("Confusión")]
    [Tooltip("Cada cuántos segundos elige un nuevo punto aleatorio.")]
    public float wanderInterval = 0.6f;

    public override void ApplyEffect(IEnemy enemy, Vector3 hitPosition)
    {
        // Necesitamos un MonoBehaviour para arrancar corutinas.
        var enemyMB = enemy as MonoBehaviour;           // IEnemy → MonoBehaviour :contentReference[oaicite:2]{index=2}:contentReference[oaicite:3]{index=3}
        if (enemyMB == null) return;

        // Agente de navegación; si el enemigo no usa NavMesh, salimos.
        var agent = enemyMB.GetComponent<NavMeshAgent>();
        if (agent == null || !agent.isOnNavMesh) return;

        enemyMB.StartCoroutine(FearConfuseRoutine(agent, hitPosition));
    }

    IEnumerator FearConfuseRoutine(NavMeshAgent agent, Vector3 origin)
    {
        float elapsed = 0f;

        /* ─────────────── 1. Fase de huida ─────────────── */
        Vector3 fleeDir = (agent.transform.position - origin).normalized;
        if (fleeDir.sqrMagnitude < 0.01f)          // si impactó casi encima
            fleeDir = Random.insideUnitSphere;     // huimos en dirección random

        Vector3 fleeTarget = agent.transform.position + fleeDir * fleeDistance;

        // Aseguramos que el punto esté en NavMesh
        if (NavMesh.SamplePosition(fleeTarget, out var hit, 2f, agent.areaMask))
            agent.SetDestination(hit.position);

        while (elapsed < fleePhase)
        {
            elapsed += Time.deltaTime;
            yield return null;
        }

        /* ─────────────── 2. Fase de confusión ─────────────── */
        float confuseEnd = totalDuration;
        while (elapsed < confuseEnd)
        {
            // Elegimos un destino aleatorio dentro de wanderRadius
            Vector3 randomDir = Random.insideUnitSphere * wanderRadius;
            randomDir += agent.transform.position;

            if (NavMesh.SamplePosition(randomDir, out var wanderHit, 2f, agent.areaMask))
                agent.SetDestination(wanderHit.position);

            // Esperamos hasta la siguiente decisión
            float t = 0f;
            while (t < wanderInterval)
            {
                t       += Time.deltaTime;
                elapsed += Time.deltaTime;
                if (elapsed >= confuseEnd) break;
                yield return null;
            }
        }
        
        agent.ResetPath();
    }
}
