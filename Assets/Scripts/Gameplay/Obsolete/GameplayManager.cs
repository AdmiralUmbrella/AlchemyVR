using Unity.VisualScripting;
using UnityEngine;

public class GameplayManager : MonoBehaviour
{
    public WaveManagerOLD waveManager;

    void Update()
    {
        if(waveManager.currentWaveIndex == 5)
        {
            Time.timeScale = 0f;
            Debug.Log("Ganaste");
        }
    }
}