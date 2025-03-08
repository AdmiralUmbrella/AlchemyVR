using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class Cauldron : MonoBehaviour
{
    [Header("Liquid Settings")]
    [SerializeField] private Renderer liquidRenderer;
    [SerializeField] private Color defaultColor = Color.gray;

    [Header("Effects")]
    [SerializeField] private ParticleSystem successEffect;
    [SerializeField] private ParticleSystem explosionEffect;

    [Header("Mix Settings")]
    [SerializeField] private float mixDelay = 3f;
    [SerializeField] private Transform flaskSpawnPoint;
    [SerializeField] private GameObject flaskPrefab;

    [Header("UI")]
    [SerializeField] private TextMeshProUGUI timerText;

    private List<EssenceSO> currentMix = new List<EssenceSO>();
    private Coroutine currentMixRoutine;
    private float currentTimer;

    private void OnTriggerEnter(Collider other)
    {
        DraggableEssence essence = other.GetComponent<DraggableEssence>();
        if (essence != null)
        {
            AddEssence(essence.essenceData);
            Destroy(essence.gameObject);
        }
    }

    private void AddEssence(EssenceSO essence)
    {
        if (currentMix.Count >= 3) return;

        currentMix.Add(essence);
        UpdateLiquidColor();

        if (currentMixRoutine != null) StopCoroutine(currentMixRoutine);
        currentMixRoutine = StartCoroutine(MixIngredients());
    }

    private IEnumerator MixIngredients()
    {
        currentTimer = mixDelay;
        timerText.gameObject.SetActive(true);

        while (currentTimer > 0)
        {
            timerText.text = Mathf.CeilToInt(currentTimer).ToString();
            currentTimer -= Time.deltaTime;
            yield return null;
        }

        timerText.gameObject.SetActive(false);
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
        bool validRecipeFound = false;

        // Cargar todas las recetas desde Resources/Recipes
        foreach (PotionRecipeSO recipe in Resources.LoadAll<PotionRecipeSO>("Recipes"))
        {
            if (IsRecipeValid(recipe.requiredEssences))
            {
                successEffect.Play();
                CreateFlask(recipe.resultingPotion);
                validRecipeFound = true;
                break;
            }
        }

        if (!validRecipeFound)
        {
            if (currentMix.Count >= 2)
            {
                explosionEffect.Play();
                ResetCauldron();
            }
        }
    }

    private void CreateFlask(EssenceSO potion)
    {
        GameObject newFlask = Instantiate(flaskPrefab, flaskSpawnPoint.position, Quaternion.identity);
        Flask flaskComponent = newFlask.GetComponent<Flask>();
        flaskComponent.InitializeFlask(potion);
        ResetCauldron();
    }

    private bool IsRecipeValid(EssenceSO[] required)
    {
        if (currentMix.Count != required.Length) return false;
        for (int i = 0; i < required.Length; i++)
            if (currentMix[i] != required[i]) return false;
        return true;
    }

    private void ResetCauldron()
    {
        currentMix.Clear();
        UpdateLiquidColor();
    }
}