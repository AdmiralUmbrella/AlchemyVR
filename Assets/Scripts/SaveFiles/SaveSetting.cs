using System.IO;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Audio; // <-- para AudioMixer

public class SaveSetting : MonoBehaviour
{
    [Header("Referencias de UI")] [SerializeField]
    private GameObject tunnellingVignette;

    [SerializeField] private Toggle tunnelingToggle; // si antes usabas el GameObject, basta con el Toggle
    [SerializeField] private Slider musicSlider;
    [SerializeField] private Slider sfxSlider;

    [Header("Audio")] [SerializeField] private AudioMixer mixer;
    [SerializeField] private string musicParam = "MusicVol";
    [SerializeField] private string sfxParam = "SFXVol";

    private readonly SaveData saveData = new(); 

    private string saveFilePath;

    /*──────────────────────────────────────────────────────*/
    private void Awake()
    {
        saveFilePath = Path.Combine(Application.persistentDataPath, "PlayerData.json");

        if (File.Exists(saveFilePath))
        {
            JsonUtility.FromJsonOverwrite(File.ReadAllText(saveFilePath), saveData);
        }
        else // valores por defecto si no hay archivo
        {
            saveData.tunneling = true;
            saveData.musicVolume = 0.75f;
            saveData.sfxVolume = 0.75f;
        }

        // Actualizar UI y AudioMixer con los datos cargados
        tunnelingToggle.isOn = saveData.tunneling;
        musicSlider.value = saveData.musicVolume;
        sfxSlider.value = saveData.sfxVolume;

        ApplyVolumes(); // envía a AudioMixer (dB)
    }

    /*──────────────────────────────────────────────────────*/
    public void OnToggleTunneling(bool value)
    {
        if (value == false)
        {
            tunnellingVignette.SetActive(false);
            saveData.tunneling = false;
        }
        else
        {
            tunnellingVignette.SetActive(true);
            saveData.tunneling = true;
        }
    }

    public void OnChangeMusic()
    {
        saveData.musicVolume = musicSlider.value;
        SetMixer(musicParam, sfxSlider.value);
    }

    public void OnChangeSFX()
    {
        saveData.sfxVolume = sfxSlider.value;
        SetMixer(sfxParam, sfxSlider.value);
    }

    /*──────────────────────────────────────────────────────*/
    public void SaveToFile()
    {
        File.WriteAllText(saveFilePath, JsonUtility.ToJson(saveData));
        Debug.Log("Datos guardados en " + saveFilePath);
    }

    /*──────────────────── helpers ─────────────────────────*/
    private void ApplyVolumes()
    {
        SetMixer(musicParam, saveData.musicVolume);
        SetMixer(sfxParam, saveData.sfxVolume);
    }

    /// Convierte [0-1] lineal a dB (-80 silencio, 0 máximos)
    private void SetMixer(string param, float value)
    {
        float dB = Mathf.Lerp(-80f, 0f, value <= 0.01f ? 0f : value);
        mixer.SetFloat(param, dB);
    }
}