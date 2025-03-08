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
    [SerializeField] private Transform particlesSpawnPoint;

    [Header("Mix Settings")]
    [SerializeField] private float mixDelay = 3f;

    [Header("UI")]
    [SerializeField] private TextMeshProUGUI timerText;

    private List<EssenceSO> currentMix = new List<EssenceSO>();
    private Coroutine currentMixRoutine;
    private float currentTimer;
    private EssenceSO resultingPotion; // Nueva variable para almacenar la poción exitosa

    private void OnTriggerEnter(Collider other)
    {
        // Manejar esencias
        DraggableEssence essence = other.GetComponent<DraggableEssence>();
        if (essence != null)
        {
            AddEssence(essence.essenceData);
            Destroy(essence.gameObject);
            return;
        }

        // Manejar frascos vacíos
        Flask flask = other.GetComponent<Flask>();
        if (flask != null && resultingPotion != null)
        {
            TransferPotionToFlask(flask);
        }
    }

    private void AddEssence(EssenceSO essence)
    {
        if (resultingPotion != null) return; // Ignorar si ya hay una poción lista

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
        liquidRenderer.material.color = resultingPotion != null ?
            resultingPotion.essenceColor :
            (currentMix.Count > 0 ? CalculateMixColor() : defaultColor);
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

        // Primero verificar si es una poción de un solo elemento
        if (currentMix.Count == 1)
        {
            resultingPotion = currentMix[0]; // Usar el elemento directamente
            PlayEffect(successEffect);
            validRecipeFound = true;
        }
        else // Luego verificar recetas complejas
        {
            foreach (PotionRecipeSO recipe in Resources.LoadAll<PotionRecipeSO>("Recipes"))
            {
                if (IsRecipeValid(recipe.requiredEssences))
                {
                    resultingPotion = recipe.resultingPotion;
                    PlayEffect(successEffect);
                    validRecipeFound = true;
                    break;
                }
            }
        }

        if (!validRecipeFound)
        {
            // Solo explotar si hay 2+ elementos y no es válido
            if (currentMix.Count >= 2)
            {
                PlayEffect(explosionEffect);
                ResetCauldron();
            }
        }
    }

    private void TransferPotionToFlask(Flask flask)
    {
        if (resultingPotion == null) return;

        flask.InitializeFlask(resultingPotion);
        ResetCauldron();
    }

    private void PlayEffect(ParticleSystem effect)
    {
        if (effect != null && particlesSpawnPoint != null)
        {
            Instantiate(effect, particlesSpawnPoint.position, particlesSpawnPoint.rotation);
        }
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
        resultingPotion = null;
        UpdateLiquidColor();
    }
}