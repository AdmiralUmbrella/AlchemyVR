using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class Cauldron : MonoBehaviour
{
    [Header("Liquid Settings")]
    [SerializeField] private Renderer liquidRenderer;
    [SerializeField] private Color defaultShallowColor = Color.gray;
    [SerializeField] private Color defaultDeepColor = Color.blue; // Nuevo color para profundidad

    [Header("Effects")]
    [SerializeField] private ParticleSystem successEffect;
    [SerializeField] private ParticleSystem explosionEffect;
    [SerializeField] private Transform particlesSpawnPoint;

    [Header("Mix Settings")]
    [SerializeField] private float mixDelay = 3f;

    [Header("UI - Mixing Icons")]
    [SerializeField] private Image[] ingredientSlots; // Slots para iconos de ingredientes
    [SerializeField] private Image resultSlot; // Slot para icono del resultado
    [SerializeField] private Sprite emptySlotSprite; // Sprite para slot vacío


    [Header("UI")]
    [SerializeField] private TextMeshProUGUI timerText;

    private List<EssenceSO> currentMix = new List<EssenceSO>();
    private Coroutine currentMixRoutine;
    private float currentTimer;
    private EssenceSO resultingPotion;


    private void Start()
    {
        ResetUI();
    }
    private void OnTriggerEnter(Collider other)
    {
        DraggableEssence essence = other.GetComponent<DraggableEssence>();
        if (essence != null)
        {
            AddEssence(essence.essenceData);
            Destroy(essence.gameObject);
            return;
        }

        Flask flask = other.GetComponent<Flask>();
        if (flask != null && resultingPotion != null)
        {
            TransferPotionToFlask(flask);
        }
    }
    private void Update()
    {
        //reiniciar el caldero con la letra R
        if (Input.GetKeyDown(KeyCode.R))
        {
            ResetCauldron();
        }
    }
    private void AddEssence(EssenceSO essence)
    {
        if (resultingPotion != null) return;

        if (currentMix.Count >= 3) return;

        currentMix.Add(essence);
        UpdateLiquidColor();
        UpdateIngredientIcons(); // Actualizar UI al añadir


        if (currentMixRoutine != null) StopCoroutine(currentMixRoutine);
        currentMixRoutine = StartCoroutine(MixIngredients());
    }
    private void UpdateIngredientIcons()
    {
        for (int i = 0; i < ingredientSlots.Length; i++)
        {
            ingredientSlots[i].sprite = i < currentMix.Count ?
                currentMix[i].essenceIcon :
                emptySlotSprite;
        }
    }
    private IEnumerator MixIngredients()
    {
        currentTimer = mixDelay;

        while (currentTimer > 0)
        {
            currentTimer -= Time.deltaTime;
            timerText.text = Mathf.Ceil(currentTimer).ToString();
            yield return null;
        }

        CheckForValidRecipe();
    }

    private void UpdateLiquidColor()
    {
        if (resultingPotion != null)
        {
            // Aplicar color de la poción al shader
            liquidRenderer.material.SetColor("Color_F01C36BF", resultingPotion.essenceColor);
            liquidRenderer.material.SetColor("Color_7D9A58EC", resultingPotion.essenceColor * 0.5f); // Oscurecer para profundidad
        }
        else
        {
            Color mixColor = currentMix.Count > 0 ? CalculateMixColor() : defaultShallowColor;
            liquidRenderer.material.SetColor("Color_F01C36BF", mixColor);
            liquidRenderer.material.SetColor("Color_7D9A58EC", mixColor * 0.5f); // Ajuste para profundidad
        }
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

        if (currentMix.Count == 1)
        {
            resultingPotion = currentMix[0];
            UpdateResultIcon(); // Mostrar icono resultante
            PlayEffect(successEffect);
            validRecipeFound = true;
        }
        else
        {
            foreach (PotionRecipeSO recipe in Resources.LoadAll<PotionRecipeSO>("Recipes"))
            {
                if (IsRecipeValid(recipe.requiredEssences))
                {
                    resultingPotion = recipe.resultingPotion;
                    UpdateResultIcon();
                    PlayEffect(successEffect);
                    validRecipeFound = true;
                    break;
                }
            }
        }

        if (!validRecipeFound && currentMix.Count >= 2)
        {
            PlayEffect(explosionEffect);
            ResetCauldron();
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
    private void UpdateResultIcon()
    {
        if (resultingPotion != null)
        {
            resultSlot.sprite = resultingPotion.essenceIcon;
            resultSlot.color = Color.white;
        }
    }
    private void ResetCauldron()
    {
        currentMix.Clear();
        resultingPotion = null;
        ResetUI();

        // Restaurar colores por defecto del shader
        liquidRenderer.material.SetColor("Color_F01C36BF", defaultShallowColor);
        liquidRenderer.material.SetColor("Color_7D9A58EC", defaultDeepColor);
    }
    private void ResetUI()
    {
        // Limpiar todos los slots
        foreach (Image slot in ingredientSlots)
        {
            slot.sprite = emptySlotSprite;
        }
        resultSlot.sprite = emptySlotSprite;
        resultSlot.color = new Color(1, 1, 1, 0.5f); // Hacer transparente el slot de resultado
    }
}