using System;
using System.IO;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.XR.Interaction.Toolkit.Locomotion.Comfort;

public class SaveSetting : MonoBehaviour
{
    private SaveData saveData = new();
    
    public GameObject tunnelingVignetteController;

    public string saveFilePath;
    

    private void Start()
    {
        saveFilePath = Application.persistentDataPath + "/PlayerData.json";
        if (File.Exists(saveFilePath))
        {
            string loadPlayerSetting = File.ReadAllText(saveFilePath);
            saveData = JsonUtility.FromJson<SaveData>(loadPlayerSetting);
            tunnelingVignetteController.SetActive(saveData.tunneling);
            Debug.Log("Datos cargados.");
        }
        else
        {
            Debug.Log("No se ha encontrado el archivo de datos.");
        }
    }

    public void SaveToFile()
    {
        saveData.tunneling = tunnelingVignetteController.activeSelf;
        
        string savePlayerSetting = JsonUtility.ToJson(saveData);

        File.WriteAllText(saveFilePath, savePlayerSetting);

        Debug.Log("Datos guardados.");
        
    }
    
}