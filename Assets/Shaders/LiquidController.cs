using UnityEngine;

public class LiquidController : MonoBehaviour
{
    [Header("Shader Settings")]
    [SerializeField] private Renderer liquidRenderer;
    [SerializeField] private Color baseColor = Color.gray;
    [SerializeField] private Color potionColor = Color.white;

    private Material liquidMaterial;

    private void Start()
    {
        liquidMaterial = liquidRenderer.material;
        SetBaseColor();
    }

    public void SetBaseColor()
    {
        liquidMaterial.SetColor("_BaseColor", baseColor);
        liquidMaterial.SetColor("_PotionColor", baseColor); // Inicialmente igual
    }

    public void SetPotionColor(Color color)
    {
        liquidMaterial.SetColor("_PotionColor", color);
    }
}