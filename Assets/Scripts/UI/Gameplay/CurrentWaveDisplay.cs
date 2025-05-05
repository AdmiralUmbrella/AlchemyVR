using UnityEngine;
using TMPro;

public class CurrentWaveDisplay : MonoBehaviour
{
    public TextMeshProUGUI waveText;
    
    public WaveManager waveManager;
    
    void Update()
    {
        waveText.text = (waveManager.CurrentWaveIndex + 1).ToString();
    }
}