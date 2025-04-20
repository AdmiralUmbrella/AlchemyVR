using System;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Serialization;

public class PlayerEnterTower : MonoBehaviour
{
    public GameObject player;
    public GameObject towerInside;
    public GameObject towerOutside;
    public GameObject insideSpawnPoint;
    public GameObject enterPromptUI;
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag(player.tag))
        {
            enterPromptUI.SetActive(true);
        }
    }
    
    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag(player.tag))
        {
            enterPromptUI.SetActive(false);
        }
    }
    
    public void EnterTower()
    {
        player.transform.position = insideSpawnPoint.transform.position;
        towerOutside.SetActive(false);
        towerInside.SetActive(true);
    }
}