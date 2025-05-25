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
    [SerializeField] private Image timerFillImage;

    private List<EssenceSO> currentMix = new List<EssenceSO>();
    private Coroutine currentMixRoutine;
    private float currentTimer;
    private EssenceSO resultingPotion;


    private void Start()
    {
        ResetCauldron();
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
            bool hasEssence = i < currentMix.Count;
            ingredientSlots[i].gameObject.SetActive(hasEssence);

            if (hasEssence)
                ingredientSlots[i].sprite = currentMix[i].essenceIcon;
        }
    }
    
    private IEnumerator MixIngredients()
    {
        // 1) Configura la imagen y el timer
        currentTimer = 0f;
        timerFillImage.fillAmount = 0f;          // arranca vacío (0)
        timerFillImage.gameObject.SetActive(true);

        // 2) Cuenta hacia arriba hasta llegar a mixDelay
        while (currentTimer < mixDelay)
        {
            currentTimer += Time.deltaTime;
            timerFillImage.fillAmount = currentTimer / mixDelay; // 0 → 1
            yield return null;
        }

        // 3) Oculta o reinicia
        timerFillImage.gameObject.SetActive(false);
        timerFillImage.fillAmount = 0f;          // opcional: deja vacío para próxima mezcla

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
        if (resultingPotion == null) return;

        resultSlot.gameObject.SetActive(true);
        resultSlot.sprite = resultingPotion.essenceIcon;
        resultSlot.color  = Color.white;
    }
    
    private void ResetCauldron()
    {
        currentMix.Clear();
        resultingPotion = null;
        ResetUI();

        liquidRenderer.material.SetColor("Color_F01C36BF", defaultShallowColor);
        liquidRenderer.material.SetColor("Color_7D9A58EC", defaultDeepColor);
    }

    private void ResetUI()
    {
        // Desactiva todos los slots de ingredientes
        foreach (Image slot in ingredientSlots)
        {
            slot.sprite = emptySlotSprite;
            slot.gameObject.SetActive(false);
        }

        // Desactiva el slot de resultado
        resultSlot.sprite = emptySlotSprite;
        resultSlot.color  = new Color(1,1,1,0.5f);
        resultSlot.gameObject.SetActive(false);

        // Timer
        timerFillImage.fillAmount = 0f;
        timerFillImage.gameObject.SetActive(false);
    }
}