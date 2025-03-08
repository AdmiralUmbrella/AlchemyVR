using UnityEngine;

[CreateAssetMenu(fileName = "NewRecipe", menuName = "Alchemy/Recipe")]
public class PotionRecipeSO : ScriptableObject
{
    public EssenceSO[] requiredEssences; // Orden exacto de esencias
    public EssenceSO resultingPotion;    // Esencias resultantes (ej: Frostium)
}