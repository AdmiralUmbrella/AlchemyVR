using UnityEngine;

[CreateAssetMenu(fileName = "NewEssence", menuName = "Alchemy/Essence")]
public class EssenceSO : ScriptableObject
{
    [Header("Basic Settings")]
    public string essenceName;
    public Color essenceColor = Color.white;
    public float baseDamage = 10f;

    [Header("Flask Effects")]
    public GameObject roundFlaskEffect;  // Prefab para frasco redondo (ej: explosi�n)
    public GameObject squareFlaskEffect; // Prefab para frasco cuadrado (ej: zona persistente)

}