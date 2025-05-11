using UnityEngine;
using UnityEngine.InputSystem;

public class PauseManager : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Canvas pauseCanvas;
    [SerializeField] private InputActionReference pause;
    
    // NUEVO ─ referencia al script que gestiona la persistencia
    [SerializeField] private SaveSetting settings;  

    private bool isPaused;

    private void OnEnable()
    {
        pause.action.performed += OnPause;
        pause.action.Enable();
    }
    private void OnDisable()
    {
        pause.action.performed -= OnPause;
        pause.action.Disable();
    }

    private void OnPause(InputAction.CallbackContext ctx) => TogglePause();

    public void TogglePause()
    {
        bool wasPaused = isPaused;   // guardamos el estado previo
        isPaused = !isPaused;

        Time.timeScale = isPaused ? 0f : 1f;
        pauseCanvas.gameObject.SetActive(isPaused);

        // --- si acabamos de CERRAR el menú de pausa, guardamos datos
        if (wasPaused && !isPaused && settings != null)
        {
            settings.SaveToFile();
        }
    }

    /* ---------- Botones UI ---------- */
    public void Resume()  => TogglePause();   // ya guarda al salir
    public void Restart()
    {
        if (settings != null) settings.SaveToFile();   // por si reinicia sin cerrar menú
        UnityEngine.SceneManagement.SceneManager.LoadScene(
            UnityEngine.SceneManagement.SceneManager.GetActiveScene().buildIndex);
        Time.timeScale = 1f;
    }
    public void Quit()
    {
        if (settings != null) settings.SaveToFile();
        Application.Quit();
    }
}