using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement;
using System.Collections.Generic;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    [Header("Game Settings")]
    [SerializeField] private List<PotionObjective> objectives;
    [SerializeField] private int maxFailures = 3;

    [Header("UI References")]
    [SerializeField] private TextMeshProUGUI objectiveText;
    [SerializeField] private TextMeshProUGUI hintText;
    [SerializeField] private TextMeshProUGUI failuresText;
    [SerializeField] private GameObject winPanel;
    //[SerializeField] private GameObject losePanel;

    private int currentFailures = 0;
    private int currentObjectiveIndex = 0;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    private void Start()
    {
        UpdateUI();
    }

    public void RegisterPotionCreation(EssenceSO potion)
    {
        if (currentObjectiveIndex >= objectives.Count) return;

        PotionObjective currentObjective = objectives[currentObjectiveIndex];

        if (potion == currentObjective.targetPotion)
        {
            currentObjective.currentCount++;
            if (currentObjective.currentCount >= currentObjective.requiredCount)
            {
                currentObjectiveIndex++;
                if (currentObjectiveIndex >= objectives.Count)
                {
                    ShowWinScreen();
                    return;
                }
            }
        }
        else
        {
            currentFailures++;
            if (currentFailures >= maxFailures)
            {
                ShowLoseScreen();
            }
                
        }

        UpdateUI();
    }

    private void UpdateUI()
    {
        if (currentObjectiveIndex < objectives.Count)
        {
            PotionObjective current = objectives[currentObjectiveIndex];
            objectiveText.text = $"{current.targetPotion.essenceName}: {current.currentCount}/{current.requiredCount}";
            hintText.text = GetRecipeHint(current.targetPotion);
        }
        failuresText.text = $"Errores: {currentFailures}/{maxFailures}";
    }

    private string GetRecipeHint(EssenceSO potion)
    {
        // Buscar la receta correspondiente
        foreach (PotionRecipeSO recipe in Resources.LoadAll<PotionRecipeSO>("Recipes"))
        {
            if (recipe.resultingPotion == potion)
            {
                string hint = "Receta: ";
                foreach (EssenceSO essence in recipe.requiredEssences)
                {
                    hint += $"{essence.essenceName} + ";
                }
                return hint.TrimEnd(' ', '+');
            }
        }
        return "Receta desconocida";
    }

    private void ShowWinScreen()
    {
        winPanel.SetActive(true);
        Cursor.lockState = CursorLockMode.None;
        Time.timeScale = 0f; // Pausar el juego
    }

    private void ShowLoseScreen()
    {
        Debug.Log("Se cerro el juego :c");
        Application.Quit();

    }

    // Llamar desde el bot�n de UI
    public void RestartGame()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }
}

[System.Serializable]
public class PotionObjective
{
    public EssenceSO targetPotion;
    public int requiredCount;
    [HideInInspector] public int currentCount;
}