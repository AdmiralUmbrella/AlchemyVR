using UnityEngine;
using UnityEngine.AI;
using System.Collections;

[CreateAssetMenu(fileName = "StunEffectSO",
    menuName  = "Potions/Effects/Stun")]
public class StunEffectSO : PotionEffectSO          // tu clase base
{
    [Tooltip("Segundos que el objetivo permanece paralizado.")]
    public float duration = 2f;

    public override void ApplyEffect(IEnemy enemy, Vector3 hitPos)
    {
        // Necesitamos un MonoBehaviour para arrancar corutinas
        var enemyMB = enemy as MonoBehaviour;
        if (enemyMB == null) return;

        // Buscamos NavMeshAgent y Animator (ambos opcionales; salimos si no hay agente)
        var agent    = enemyMB.GetComponent<NavMeshAgent>();
        var animator = enemyMB.GetComponent<Animator>();

        if (agent == null || !agent.isOnNavMesh) return;

        enemyMB.StartCoroutine(StunRoutine(agent, animator));
    }

    IEnumerator StunRoutine(NavMeshAgent agent, Animator animator)
    {
        /* ─── Guardamos estado previo ─── */
        bool  wasStopped    = agent.isStopped;
        float originalSpeed = agent.speed;

        float originalAnimSpeed = 1f;
        if (animator != null)
            originalAnimSpeed = animator.speed;

        /* ─── Aplicamos stun ─── */
        agent.isStopped = true;
        agent.speed     = 0f;

        if (animator != null)
            animator.speed = 0f;        // pausa animaciones

        /* ─── Esperamos la duración ─── */
        yield return new WaitForSeconds(duration);

        /* ─── Restauramos ─── */
        agent.isStopped = wasStopped;
        agent.speed     = originalSpeed;

        if (animator != null)
            animator.speed = originalAnimSpeed;
    }
}