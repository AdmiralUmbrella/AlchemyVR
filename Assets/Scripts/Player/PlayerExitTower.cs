using System;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Serialization;

public class PlayerExitTower : MonoBehaviour
{
    public GameObject towerInside;
    public GameObject towerOutside;
    public GameObject outside;
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            Debug.Log("Player salió de la torre");
            other.transform.position = outside.transform.position;
            towerInside.SetActive(false);
            towerOutside.SetActive(true);
        }
    }
}