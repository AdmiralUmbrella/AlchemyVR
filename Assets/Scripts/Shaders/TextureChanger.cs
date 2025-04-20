using UnityEngine;
using UnityEngine.UI;

public class TextureChanger : MonoBehaviour
{
    [Header("Objetos a configurar")]
    [SerializeField] private Renderer targetRenderer;          // El Renderer que usa el material del Shader Graph
    [SerializeField] private string texturePropertyName = "_MainTex";  // El Reference exacto de la propiedad en el Shader Graph
    [SerializeField] private Texture alternateTexture;         // La textura nueva que quieres aplicar

    [Header("UI")]
    [SerializeField] private Button changeTextureButton;       // El botón de la UI que disparará el cambio

    private void Awake()
    {
        // Si lo conectas por código, añade aquí el listener
        if (changeTextureButton != null)
            changeTextureButton.onClick.AddListener(ChangeTexture);
    }

    /// <summary>
    /// Llama a este método para cambiar la textura en tiempo de ejecución.
    /// </summary>
    public void ChangeTexture()
    {
        if (targetRenderer == null || alternateTexture == null)
        {
            Debug.LogWarning("Faltan referencias en TextureChanger.");
            return;
        }

        // Obtén una instancia única del material (no cambiará el original en el asset)
        Material matInstance = targetRenderer.material;
        matInstance.SetTexture(texturePropertyName, alternateTexture);
    }
}