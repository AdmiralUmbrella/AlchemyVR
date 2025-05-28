using UnityEngine;
using UnityEngine.AI;
using System.Collections; 

[CreateAssetMenu(menuName = "Alchemy/Effects/PushEffect")]
public class PushEffectSO : PotionEffectSO
{
    [Header("Push Settings")]
    public float pushForce;

    public override void ApplyEffect(IEnemy enemy, Vector3 hitPos)
    {
        var mb = enemy as MonoBehaviour;
        if (mb == null) return;
        var agent = mb.GetComponent<NavMeshAgent>();
        if (agent == null) return;

        mb.StartCoroutine(PushRoutine(agent, mb.transform.position, hitPos));
    }

    IEnumerator PushRoutine(NavMeshAgent agent, Vector3 origin, Vector3 hitPos)
    {
        Vector3 dir = (origin - hitPos).normalized;
        float t = 0f, duration = 0.2f; // breve empujón
        while (t < duration)
        {
            agent.Move(dir * pushForce * Time.deltaTime);
            t += Time.deltaTime;
            yield return null;
        }
    }

}
