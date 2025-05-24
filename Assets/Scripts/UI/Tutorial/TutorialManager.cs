using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class TutorialManager : MonoBehaviour
{
    [Header("UI Elements")]
    public TextMeshProUGUI tutorialText;
    public Image tutorialImage;
    public Button nextButton;

    [Header("Indicadores disponibles en la escena")]
    public GameObject[] allIndicators;

    [Header("Secuencia a reproducir")]
    public TutorialSequenceSO sequence;   // Arrastra aquí tu asset TutorialSequence

    private int _currentStep = -1;
    private float _stepStartTime;

    private void Awake() => nextButton.onClick.AddListener(OnNextPressed);

    private void Start() => GoToStep(0);

    private void OnNextPressed()
    {
        var step = sequence.steps[_currentStep];
        if (Time.time - _stepStartTime < step.minStepDuration) return;

        int next = _currentStep + 1;
        if (next < sequence.steps.Count) GoToStep(next);
        else EndTutorial();
    }

    private void GoToStep(int index)
    {
        _currentStep = index;
        _stepStartTime = Time.time;

        var step = sequence.steps[index];

        // Imagen
        tutorialImage.gameObject.SetActive(step.image != null);
        tutorialImage.sprite = step.image;

        // Texto
        bool hasText = !string.IsNullOrWhiteSpace(step.text);
        tutorialText.gameObject.SetActive(hasText);
        tutorialText.text = step.text;

        // Indicadores
        foreach (var go in allIndicators) go.SetActive(false);
        if (step.indicator != null) step.indicator.SetActive(true);

        // Botón
        nextButton.GetComponentInChildren<TMP_Text>().text =
            (_currentStep == sequence.steps.Count - 1) ? "Cerrar" : "Next";
    }

    private void EndTutorial()
    {
        tutorialImage.gameObject.SetActive(false);
        tutorialText.gameObject.SetActive(false);
        nextButton.gameObject.SetActive(false);
        foreach (var go in allIndicators) go.SetActive(false);
    }
}