using System;
using Unity.VisualScripting;
using UnityEngine;

public class PlayerEnterTower : MonoBehaviour
{
    public GameObject towerInside;
    public GameObject towerOutside;
    public GameObject inside;
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            Debug.Log("Player entro en la torre");
            other.transform.position = inside.transform.position;
            towerOutside.SetActive(false);
            towerInside.SetActive(true);
        }
    }
}