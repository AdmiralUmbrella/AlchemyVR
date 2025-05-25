using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Video;          // <‒‒‒ nuevo
using TMPro;
using UnityEngine.Events;

public class TutorialManager : MonoBehaviour
{
    /* ──────────────────────────── UI ──────────────────────────── */
    [Header("UI Elements")]
    [SerializeField] private TextMeshProUGUI headerText;
    [SerializeField] private TextMeshProUGUI bodyText;
    [SerializeField] private RawImage        preview;       // Muestra imagen o vídeo
    [SerializeField] private Button          nextButton;

    /* ───────────────────────── Datos ─────────────────────────── */
    [Header("Multimedia")]
    [SerializeField] private VideoPlayer     videoPlayer;    // <‒‒‒ asigna el único VideoPlayer de la escena
    [SerializeField] private RenderTexture   fallbackRT;     // Se crea al vuelo si ningún paso especifica uno

    [Header("Steps (configurar en Inspector)")]
    [SerializeField] private TutorialStep[] steps;

    [Header("Events")]
    public UnityEvent OnTutorialFinished;

    /* ─────────────────── Estado interno ─────────────────────── */
    private int currentStep = -1;

    /* ───────────────────── Lifecycle ─────────────────────────── */
    private void Awake()
    {
        nextButton.onClick.AddListener(HandleNext);
    }

    private void Start() => HandleNext();      // Arranca en el primer paso

    private void OnDestroy()
    {
        nextButton.onClick.RemoveListener(HandleNext);
    }

    /* ────────────────── Navegación de pasos ─────────────────── */
    private void HandleNext()
    {
        // 1) Apaga indicador del paso anterior
        if (currentStep >= 0 && currentStep < steps.Length)
        {
            var prevIndicator = steps[currentStep].indicator;
            if (prevIndicator) prevIndicator.SetActive(false);
        }

        // 2) Avanza índice
        currentStep++;

        // 3) ¿Se acabó el tutorial?
        if (currentStep >= steps.Length)
        {
            EndTutorial();
            return;
        }

        // 4) Muestra el nuevo paso
        ShowStep(steps[currentStep]);
    }

    /* ─────────────────── Mostrar un paso ────────────────────── */
    private void ShowStep(TutorialStep step)
    {
        /* Texto */
        headerText.text = step.header;
        bodyText.text   = step.body;

        /* RenderTexture (se decide aquí para vídeo o imagen) */
        RenderTexture rt = step.renderTexture;

        if (!rt)                 // Si no trae RT, usamos una genérica
        {
            if (!fallbackRT)
                fallbackRT = new RenderTexture(1920, 1080, 0);
            rt = fallbackRT;
        }

        /* Vídeo */
        if (step.videoClip)
        {
            videoPlayer.Stop();                       // Reinicia por si acaso
            videoPlayer.clip          = step.videoClip;
            videoPlayer.targetTexture = rt;
            videoPlayer.Play();

            preview.texture = rt;
            preview.gameObject.SetActive(true);
        }
        /* Imagen estática */
        else if (step.renderTexture)
        {
            videoPlayer.Stop();
            preview.texture = step.renderTexture;
            preview.gameObject.SetActive(true);
        }
        /* Sin multimedia */
        else
        {
            videoPlayer.Stop();
            preview.gameObject.SetActive(false);
        }

        /* Indicador de la escena */
        if (step.indicator) step.indicator.SetActive(true);

        /* Eventos extra del paso */
        step.onStepStart?.Invoke();
    }

    /* ─────────────────── Finalizar tutorial ─────────────────── */
    private void EndTutorial()
    {
        // Oculta UI
        headerText.gameObject.SetActive(false);
        bodyText .gameObject.SetActive(false);
        preview  .gameObject.SetActive(false);
        nextButton.gameObject.SetActive(false);

        // Detiene vídeo si hubiese
        if (videoPlayer) videoPlayer.Stop();

        // Evento global
        OnTutorialFinished?.Invoke();
    }
}
