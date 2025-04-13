using UnityEngine;

[CreateAssetMenu(fileName = "NewEssence", menuName = "Alchemy/Essence")]
public class EssenceSO : ScriptableObject
{
    [Header("Basic Settings")]
    public string essenceName;
    public Color essenceColor = Color.white;
    public float baseDamage = 10f;

    [Header("Effect List")]
    public PotionEffectSO[] effectsToApply; // <-- Aquí se ponen la lista de efectos que queramos en la poción resultante.

    [Header("Flask Effects")]
    public GameObject roundFlaskEffect;  // Prefab para frasco redondo (ej: explosión)
    public GameObject squareFlaskEffect; // Prefab para frasco cuadrado (ej: zona persistente)

}