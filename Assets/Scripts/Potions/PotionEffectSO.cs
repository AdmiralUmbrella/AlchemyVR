using UnityEngine;

public abstract class PotionEffectSO : ScriptableObject
{ 
    // Método que se implementa en cada tipo de afecto; ahora recibe un IEnemy.
    public abstract void ApplyEffect(IEnemy enemy, Vector3 hitPosition);
}
