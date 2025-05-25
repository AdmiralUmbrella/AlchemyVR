using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Video;

[System.Serializable]
public class TutorialStep
{
    [Header("Contenido UI")] 
    public string header; // Encabezado (TMPro)
    [TextArea] public string body; // Texto principal (TMPro)
    
    [Header("Multimedia")] 
    public VideoClip videoClip; // Clip opcional
    public RenderTexture renderTexture; // Dejar vacío = reutilizar uno genérico

    [Header("Escena")] 
    public GameObject indicator; // Ej. indicatorEssence
    public UnityEvent onStepStart; // Eventos opcionales
}