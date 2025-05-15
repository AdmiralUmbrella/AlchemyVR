using UnityEngine;
using UnityEngine.AI;
using System.Collections;

[CreateAssetMenu(menuName = "Alchemy/Effects/StunEffect")]
public class StunEffectSO : PotionEffectSO       // hereda de tu clase base
{
    [Tooltip("Segundos que el objetivo permanece paralizado.")]
    public float duration = 2f;

    public override void ApplyEffect(IEnemy enemy, Vector3 hitPosition)
    {
        // Necesitamos un MonoBehaviour para arrancar la corutina
        var enemyMB = enemy as MonoBehaviour;
        if (enemyMB == null) return;

        // Buscamos su NavMeshAgent
        var agent = enemyMB.GetComponent<NavMeshAgent>();
        if (agent == null) return;

        // Lanzamos la corutina de stun
        enemyMB.StartCoroutine(StunRoutine(agent));
    }

    IEnumerator StunRoutine(NavMeshAgent agent)
    {
        // Guardamos estado previo
        bool  wasStopped    = agent.isStopped;
        float originalSpeed = agent.speed;

        // ► Paralizamos
        agent.isStopped = true;
        agent.speed     = 0f;

        yield return new WaitForSeconds(duration);

        // ◄ Restauramos
        agent.isStopped = wasStopped;
        agent.speed     = originalSpeed;
    }
}