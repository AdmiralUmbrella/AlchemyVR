using System.Collections.Generic;
using UnityEngine;

public class Cauldron : MonoBehaviour
{
    [Header("Liquid Settings")]
    [SerializeField] private Renderer liquidRenderer;
    [SerializeField] private Color defaultColor = Color.gray;

    [Header("Effects")]
    [SerializeField] private ParticleSystem successEffect;
    [SerializeField] private ParticleSystem explosionEffect;

    private List<EssenceSO> currentMix = new List<EssenceSO>();

    private void OnTriggerEnter(Collider other)
    {
        DraggableEssence essence = other.GetComponent<DraggableEssence>();
        if (essence != null)
        {
            AddEssence(essence.essenceData);
            Destroy(essence.gameObject); // Destruye la esencia física
        }
    }

    private void AddEssence(EssenceSO essence)
    {
        if (currentMix.Count >= 3) return;

        Debug.Log(essence.essenceName);
        currentMix.Add(essence);
        UpdateLiquidColor();
        CheckForValidRecipe();
    }

    private void UpdateLiquidColor()
    {
        Color mixColor = currentMix.Count > 0 ? CalculateMixColor() : defaultColor;
        liquidRenderer.material.color = mixColor;
    }

    private Color CalculateMixColor()
    {
        Color mix = Color.black;
        foreach (EssenceSO essence in currentMix)
            mix += essence.essenceColor;
        return mix / currentMix.Count;
    }

    private void CheckForValidRecipe()
    {
        foreach (PotionRecipeSO recipe in Resources.LoadAll<PotionRecipeSO>("Recipes"))
        {
            if (IsRecipeValid(recipe.requiredEssences))
            {
                successEffect.Play();
                Debug.Log(recipe.resultingPotion);
                // Aquí puedes generar la poción resultante si lo deseas
                ResetCauldron();
                return;
            }
        }

        if (currentMix.Count >= 3)
        {
            explosionEffect.Play();
            ResetCauldron();
        }
    }

    private bool IsRecipeValid(EssenceSO[] required)
    {
        if (currentMix.Count != required.Length) return false;
        for (int i = 0; i < required.Length; i++)
            if (currentMix[i] != required[i]) return false;
        return true;
    }

    private void ResetCauldron() => currentMix.Clear();
}