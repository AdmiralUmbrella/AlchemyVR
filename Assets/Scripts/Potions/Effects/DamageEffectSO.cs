using UnityEngine;

[CreateAssetMenu(menuName = "Alchemy/Effects/DamageEffect")]
public class DamageEffectSO : PotionEffectSO
{
    [Header("Damage Settings")]
    public EssenceSO essence;

    /// <summary>
    /// M�todo que aplica el efecto de da�o a los enemigos.
    /// </summary>
    public override void ApplyEffect(EnemyStateManager enemy, Vector3 hitPosition)
    {
        enemy.TakeDamage(
            Mathf.RoundToInt(essence.baseDamage),
            hitPosition,
            essence.name);
    }
}
