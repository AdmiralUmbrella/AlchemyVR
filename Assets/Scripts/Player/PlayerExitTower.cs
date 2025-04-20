using System;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Serialization;

public class PlayerExitTower : MonoBehaviour
{
    public GameObject player;
    public GameObject towerInside;
    public GameObject towerOutside;
    public GameObject outsideSpawnPoint;
    public GameObject exitPromptUI;
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag(player.tag))
        {
            exitPromptUI.SetActive(true);
        }
    }
    
    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag(player.tag))
        {
            exitPromptUI.SetActive(false);
        }
    }
    
    public void ExitTower()
    {
        player.transform.position = outsideSpawnPoint.transform.position;
        towerInside.SetActive(false);
        towerOutside.SetActive(true);
    }
}