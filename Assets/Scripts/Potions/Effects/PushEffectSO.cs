using UnityEngine;

[CreateAssetMenu(menuName = "Alchemy/Effects/PushEffect")]
public class PushEffectSO : PotionEffectSO
{
    [Header("Push Settings")]
    public float pushForce;

    public override void ApplyEffect(EnemyStateManager enemy, Vector3 hitPosition)
    {
        // Aquí, como el EnemyStateManager maneja el knockback,
        // podemos setear la dirección y forzar un HitState si quieres:

        Vector3 direction = (enemy.transform.position - hitPosition).normalized;
        enemy.enemyData.knockbackDirection = direction;
        enemy.enemyData.knockbackForce = pushForce;

        // Forzamos el estado "HIT" o algún método que active la animación de ser empujado
        enemy.ChangeState(enemy.HitState);
    }
}
