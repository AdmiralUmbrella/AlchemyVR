using UnityEngine;
using UnityEngine.UI;

public class TowerHealthUI : MonoBehaviour
{
    [Tooltip("Image component representing the tower's health bar.")]
    public Image healthBar;

    [Tooltip("Reference to the TowerAI that will take damage.")]
    public TowerAI towerAI;

    private void Start()
    {
        if(towerAI != null && towerAI.towerData != null && healthBar != null)
        {
            // Initialize the fill amount
            healthBar.fillAmount = (float)towerAI.towerData.currentHealth / towerAI.towerData.maxHealth;
        }
    }

    // This method can be called whenever the tower takes damage
    public void UpdateHealthBar()
    {
        if(towerAI != null && towerAI.towerData != null && healthBar != null)
        {
            float normalizedHealth = (float)towerAI.towerData.currentHealth / towerAI.towerData.maxHealth;
            healthBar.fillAmount = normalizedHealth;
        }
    }
}