using UnityEngine;
using UnityEngine.AI;
using System.Collections;  

[CreateAssetMenu(menuName = "Alchemy/Effects/SlowDownEffect")]
public class SlowDownEffect : PotionEffectSO
{
    [Range(0.05f, 1f)]
    [Tooltip("Factor que se multiplica por la velocidad original (0.5 = 50 %).")]
    public float speedMultiplier = 0.5f;

    [Tooltip("Duración del efecto en segundos.")]
    public float duration = 3f;

    public override void ApplyEffect(IEnemy enemy, Vector3 hitPosition)
    {
        // ── 1) Necesitamos que realmente sea un MonoBehaviour para arrancar la corutina
        var enemyMB = enemy as MonoBehaviour;
        if (enemyMB == null) return;

        // ── 2) Intentamos obtener el NavMeshAgent
        var agent = enemyMB.GetComponent<NavMeshAgent>();
        if (agent == null) return;

        // ── 3) Lanzamos la corutina; si quieres refresh, puedes detener la anterior primero
        enemyMB.StartCoroutine(SlowRoutine(agent));
    }

    IEnumerator SlowRoutine(NavMeshAgent agent)
    {

        // Aplicamos la ralentización
        agent.speed = 3.5f * speedMultiplier;

        // Esperamos la duración configurada
        yield return new WaitForSeconds(duration);

        // Restauramos la velocidad original
        agent.speed = 3.5f;
    }
}