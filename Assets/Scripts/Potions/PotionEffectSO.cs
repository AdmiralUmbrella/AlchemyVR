using UnityEngine;

public abstract class PotionEffectSO : ScriptableObject
{ 
    //Método que se implementa en cada tipo de afecto
    public abstract void ApplyEffect(EnemyStateManager enemy, Vector3 hitPosition);

}
