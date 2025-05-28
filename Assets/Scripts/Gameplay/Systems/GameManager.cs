using UnityEngine;
using System.Collections.Generic;

public class GameManager : MonoBehaviour
{
    [Header("Torres a vigilar")]
    [Tooltip("Arrastra aquí las 3 torres desde la jerarquía")]
    [SerializeField] private List<GameObject> towers;

    [Header("Canvas de fin de juego")]
    [Tooltip("Canvas que se mostrará cuando las 3 torres estén destruidas")]
    [SerializeField] private Canvas endGameCanvas;

    // Para asegurarnos de mostrar el Canvas sólo una vez
    private bool hasGameEnded = false;

    void Start()
    {
        Time.timeScale = 1;
        // Al inicio, ocultamos el Canvas de fin de juego
        if (endGameCanvas != null)
            endGameCanvas.gameObject.SetActive(false);
        else
            Debug.LogWarning("GameManager: No hay Canvas asignado en 'endGameCanvas'.");
    }

    void Update()
    {
        if (hasGameEnded)
            return;

        // Comprobamos si todas las torres están destruidas (== null tras Destroy)
        bool allDestroyed = true;
        foreach (var tower in towers)
        {
            if (tower != null)
            {
                allDestroyed = false;
                break;
            }
        }

        if (allDestroyed)
        {
            hasGameEnded = true;
            ShowEndCanvas();
        }
    }

    private void ShowEndCanvas()
    {
        Time.timeScale = 0;
        if (endGameCanvas != null)
            endGameCanvas.gameObject.SetActive(true);
        else
            Debug.LogError("GameManager: No se pudo mostrar el Canvas de fin porque 'endGameCanvas' no está asignado.");
    }
}