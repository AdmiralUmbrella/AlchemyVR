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
    
    [Header("Referencia al Transform del jugador")]
    [Tooltip("Arrastra aquí el Transform del jugador en el Inspector")]
    [SerializeField] private Transform playerTransform;
    [SerializeField] private Transform beltTransform;

    
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
    
    /// <summary>
    /// Agranda al jugador estableciendo su escala global en 2.
    /// </summary>
    public void EnlargePlayer()
    {
        if (playerTransform == null && beltTransform == null)
        {
            Debug.LogWarning("PlayerScaler: no se ha asignado playerTransform.");
            return;
        }
        playerTransform.localScale = Vector3.one * 2f;
        beltTransform.localScale = Vector3.one * 2f;
    }

    /// <summary>
    /// Achica al jugador restableciendo su escala global en 1.
    /// </summary>
    public void ShrinkPlayer()
    {
        if (playerTransform == null && beltTransform == null)
        {
            Debug.LogWarning("PlayerScaler: no se ha asignado playerTransform.");
            return;
        }
        playerTransform.localScale = Vector3.one;
        beltTransform.localScale = Vector3.one;
    }
}