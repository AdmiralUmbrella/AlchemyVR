using UnityEngine;
using UnityEngine.UI;

public class TowerHealthUI : MonoBehaviour
{
    [Tooltip("Image cuyo Fill Amount representa la vida.")]
    public Image healthBar;

    [Tooltip("Referencia al TowerAI que debe notificar los cambios de vida.")]
    public TowerAI towerAI;

    private void OnEnable()
    {
        // Seguridad: si el TowerAI existe, me suscribo
        if (towerAI != null)
        {
            towerAI.OnHealthChanged += UpdateHealthBar;
        }
    }

    private void OnDisable()
    {
        if (towerAI != null)
        {
            towerAI.OnHealthChanged -= UpdateHealthBar;
        }
    }

    // Se llamará automáticamente desde TowerAI
    private void UpdateHealthBar(float normalizedHealth)
    {
        if (healthBar != null)
            healthBar.fillAmount = normalizedHealth;
    }
}