using System.Collections.Generic;
using UnityEngine;

public class Cauldron : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Renderer liquidRenderer; // Renderer del l�quido
    [SerializeField] private Color defaultColor = Color.gray;

    private List<EssenceSO> currentMix = new List<EssenceSO>();
    private Color currentColor;

    private void Start() => ResetCauldron();

    // A�adir esencia al caldero
    public void AddEssence(EssenceSO essence)
    {
        if (currentMix.Count >= 3) return; // L�mite de 3 esencias

        currentMix.Add(essence);
        UpdateLiquidColor();
    }

    // Actualizar color del l�quido (mezcla de colores)
    private void UpdateLiquidColor()
    {
        if (currentMix.Count == 0)
        {
            currentColor = defaultColor;
        }
        else
        {
            Color mixColor = Color.black;
            foreach (EssenceSO essence in currentMix)
                mixColor += essence.essenceColor;

            currentColor = mixColor / currentMix.Count;
        }

        liquidRenderer.material.color = currentColor;
    }

    // Transferir mezcla a un frasco
    public bool TransferToFlask(Flask flask)
    {
        EssenceSO resultingPotion = CheckRecipes();

        if (resultingPotion != null)
        {
            flask.InitializeFlask(resultingPotion);
            ResetCauldron();
            return true;
        }
        else
        {
            flask.InitializeFlask(null); // Mezcla inv�lida = explosi�n
            ResetCauldron();
            return false;
        }
    }

    // Verificar recetas v�lidas
    private EssenceSO CheckRecipes()
    {
        foreach (PotionRecipeSO recipe in Resources.LoadAll<PotionRecipeSO>("Recipes"))
        {
            if (IsRecipeValid(recipe.requiredEssences))
                return recipe.resultingPotion;
        }
        return null;
    }

    private bool IsRecipeValid(EssenceSO[] required)
    {
        if (currentMix.Count != required.Length) return false;

        for (int i = 0; i < required.Length; i++)
        {
            if (currentMix[i] != required[i]) return false;
        }

        return true;
    }

    private void ResetCauldron()
    {
        currentMix.Clear();
        UpdateLiquidColor();
    }
}