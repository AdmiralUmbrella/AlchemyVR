using UnityEngine;

[CreateAssetMenu(menuName = "Alchemy/Effects/PushEffect")]
public class PushEffectSO : PotionEffectSO
{
    [Header("Push Settings")]
    public float pushForce;

    public override void ApplyEffect(IEnemy enemy, Vector3 hitPosition)
    {
        // Para aplicar fuerza, se necesita acceder al Rigidbody. Suponiendo que el enemy es también un MonoBehaviour:
        MonoBehaviour enemyMb = enemy as MonoBehaviour;
        if (enemyMb == null)
        {
            Debug.LogWarning("El enemy no es un MonoBehaviour. No se puede acceder al Rigidbody.");
            return;
        }
        Rigidbody enemyRb = enemyMb.GetComponent<Rigidbody>();
        if (enemyRb == null)
        {
            Debug.LogWarning("El enemy no tiene un Rigidbody asignado");
            return;
        }

        Vector3 direction = (enemyMb.transform.position - hitPosition).normalized;
        enemyRb.AddForce(direction * pushForce, ForceMode.Impulse);
        /*
        if (enemy.enemyData.agent != null) 
        { 
            enemy.enemyData.agent.isStopped = true;
        }
        */
    }
}
