using UnityEngine;
using TMPro;

[CreateAssetMenu(
    fileName = "TutorialStep",
    menuName = "Game/Tutorial/Step",
    order = 0)]
public class TutorialStepSO : ScriptableObject
{
    [Header("Contenido visual")]
    public Sprite image;
    [TextArea] public string text;

    [Header("Indicador a activar (opcional)")]
    public GameObject indicator;      // Prefab o referencia de escena
    public float minStepDuration = 0f;
}