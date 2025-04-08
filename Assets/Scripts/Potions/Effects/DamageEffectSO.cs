using UnityEngine;

[CreateAssetMenu(menuName = "Alchemy/Effects/DamageEffect")]
public class DamageEffectSO : PotionEffectSO
{
    [Header("Damage Settings")]
    public EssenceSO essence;

    /// <summary>
    /// Método que aplica el efecto de daño a los enemigos.
    /// </summary>
    public override void ApplyEffect(IEnemy enemy, Vector3 hitPosition)
    {
        enemy.TakeDamage(
            Mathf.RoundToInt(essence.baseDamage),
            hitPosition,
            essence.name);
    }
}
