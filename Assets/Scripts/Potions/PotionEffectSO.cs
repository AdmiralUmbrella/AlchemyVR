using UnityEngine;

public abstract class PotionEffectSO : ScriptableObject
{ 
    //M�todo que se implementa en cada tipo de afecto
    public abstract void ApplyEffect(EnemyStateManager enemy, Vector3 hitPosition);

}
