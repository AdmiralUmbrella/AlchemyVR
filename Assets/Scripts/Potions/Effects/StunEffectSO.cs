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
        if (enemy is IStunnable stunTarget)
            stunTarget.ApplyStun(duration);
    }
}