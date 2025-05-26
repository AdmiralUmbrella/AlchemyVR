using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Video;
using TMPro;
using UnityEngine.Events;

public class TutorialManager : MonoBehaviour
{
    /* ────────────── UI ────────────── */
    [Header("UI Elements")]
    [SerializeField] private TextMeshProUGUI headerText;
    [SerializeField] private TextMeshProUGUI bodyText;
    [SerializeField] private RawImage        preview;
    [SerializeField] private Button          nextButton;
    [SerializeField] private Button          previousButton;          // NEW

    /* ─────────── Multimedia ───────── */
    [Header("Multimedia")]
    [SerializeField] private VideoPlayer   videoPlayer;
    [SerializeField] private RenderTexture fallbackRT;

    /* ─────────── Pasos │ Datos ─────── */
    [Header("Steps (configurar en Inspector)")]
    [SerializeField] private TutorialStep[] steps;

    [Header("Events")]
    public UnityEvent OnTutorialFinished;

    /* ─────────── Estado interno ───── */
    private int currentStep = 0;

    /* ─────────── Lifecycle ────────── */
    private void Awake()
    {
        nextButton    .onClick.AddListener(HandleNext);
        previousButton.onClick.AddListener(HandlePrevious);          // NEW
    }

    private void Start()
    {
        ShowStep(steps[currentStep]);                                // arranca en 1er paso
    }

    private void OnDestroy()
    {
        nextButton    .onClick.RemoveListener(HandleNext);
        previousButton.onClick.RemoveListener(HandlePrevious);       // NEW
    }

    /* ───────── Navegación ─────────── */
    private void HandleNext()
    {
        if (currentStep >= steps.Length - 1)
        {
            EndTutorial();
            return;
        }

        ToggleIndicator(currentStep, false);   // apaga indicador actual
        currentStep++;
        ShowStep(steps[currentStep]);
    }

    private void HandlePrevious()                                             // NEW
    {
        if (currentStep <= 0) return;

        ToggleIndicator(currentStep, false);   // apaga indicador actual
        currentStep--;
        ShowStep(steps[currentStep]);
    }

    /* ──────── Mostrar paso ────────── */
    private void ShowStep(TutorialStep step)
    {
        /* Texto */
        headerText.text = step.header;
        bodyText.text   = step.body;

        /* RenderTexture */
        RenderTexture rt = step.renderTexture;
        if (!rt)
        {
            if (!fallbackRT)
                fallbackRT = new RenderTexture(1920, 1080, 0);
            rt = fallbackRT;
        }

        /* Vídeo o imagen */
        if (step.videoClip)
        {
            videoPlayer.Stop();
            videoPlayer.clip          = step.videoClip;
            videoPlayer.targetTexture = rt;
            videoPlayer.Play();

            preview.texture = rt;
            preview.gameObject.SetActive(true);
        }
        else if (step.renderTexture)
        {
            videoPlayer.Stop();
            preview.texture = step.renderTexture;
            preview.gameObject.SetActive(true);
        }
        else
        {
            videoPlayer.Stop();
            preview.gameObject.SetActive(false);
        }

        /* Indicador */
        ToggleIndicator(currentStep, true);

        /* Eventos del paso */
        step.onStepStart?.Invoke();

        /* Actualiza estado de botones */                                   // NEW
        previousButton.interactable = currentStep > 0;
        nextButton    .interactable = true;    // siempre activo (puedes añadir lógica si quieres desactivarlo en el último paso)
    }

    /* ─────── Helpers ─────────────── */
    private void ToggleIndicator(int index, bool state)
    {
        if (index >= 0 && index < steps.Length && steps[index].indicator)
            steps[index].indicator.SetActive(state);
    }

    /* ─────── Finalizar ───────────── */
    private void EndTutorial()
    {
        headerText .gameObject.SetActive(false);
        bodyText   .gameObject.SetActive(false);
        preview    .gameObject.SetActive(false);
        nextButton .gameObject.SetActive(false);
        previousButton.gameObject.SetActive(false);                      // NEW

        if (videoPlayer) videoPlayer.Stop();
        OnTutorialFinished?.Invoke();
    }
}
