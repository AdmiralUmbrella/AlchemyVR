using UnityEngine;

[CreateAssetMenu(menuName = "Alchemy/Effects/PushEffect")]
public class PushEffectSO : PotionEffectSO
{
    [Header("Push Settings")]
    public float pushForce;

    public override void ApplyEffect(EnemyStateManager enemy, Vector3 hitPosition)
    {
        // Forzamos el estado "HIT" o algún método que active la animación de ser empujado
        enemy.ChangeState(enemy.HitState);

        Rigidbody enemyRb = enemy.GetComponent<Rigidbody>();
        if (enemyRb == null)
        {
            Debug.LogWarning("El enemigo no tiene un Rigidbody asignado");
        }

        Vector3 direction = (enemy.transform.position - hitPosition).normalized;
        
        enemyRb.AddForce(direction * pushForce, ForceMode.Impulse);
        /*
        if (enemy.enemyData.agent != null) 
        { 
            enemy.enemyData.agent.isStopped = true;
        }
        */
    }
}
