using UnityEngine;
using UnityEngine.Events;

[System.Serializable]
public struct Mapping {
    [Tooltip("Textura que activa este destino")]
    public Texture texture;
    [Tooltip("Transform al que quieres teleportar")]
    public Transform target;
}

public class Teleporter : MonoBehaviour
{
    [Header("Tag del jugador")]
    [SerializeField] private string playerTag = "Player";

    [Header("Propiedad de textura en el material")]
    [SerializeField] private string texturePropertyName = "_MainTex";

    [Header("Mapeos textura → destino")]
    public Mapping[] mappings;
    
    [Header("Torres")]
    public GameObject[] towers;
    
    [Header("Unity Event")]
    public UnityEvent onPlayerTeleported;  

    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag(playerTag)) return;

        Renderer rend = GetComponent<Renderer>();
        if (rend == null) {
            Debug.LogWarning($"{name}: no hay Renderer.");
            return;
        }

        Texture currentTex = rend.material.GetTexture(texturePropertyName);
        if (currentTex == null) {
            Debug.LogWarning($"{name}: no se encontró textura en '{texturePropertyName}'.");
            return;
        }

        // Busca el Mapping cuyo .texture coincide con currentTex
        foreach (var m in mappings)
        {
            if (m.texture == currentTex && m.target != null)
            {
                towers[0].SetActive(false);
                towers[1].SetActive(true);
                other.transform.SetPositionAndRotation(
                    m.target.position,
                    m.target.rotation
                );
                onPlayerTeleported?.Invoke(); 
                return;
            }
        }

        Debug.LogWarning($"{name}: no hay mapping para la textura {currentTex.name}.");
    }
}