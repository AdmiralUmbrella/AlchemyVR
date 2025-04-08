using UnityEngine;

public abstract class PotionEffectSO : ScriptableObject
{ 
    // M�todo que se implementa en cada tipo de afecto; ahora recibe un IEnemy.
    public abstract void ApplyEffect(IEnemy enemy, Vector3 hitPosition);
}
