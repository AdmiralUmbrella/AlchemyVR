using UnityEngine;

public class Flask : MonoBehaviour
{
    [Header("Settings")]
    [SerializeField] private bool isRoundFlask; // Definido en el Inspector

    [Header("Explosion Settings")]
    [SerializeField] private GameObject explosionEffectPrefab;

    private EssenceSO currentEssence;
    private Rigidbody rb;

    private void Awake() => rb = GetComponent<Rigidbody>();

    // Inicializar frasco con esencia (llamado por el caldero)
    public void InitializeFlask(EssenceSO essence)
    {
        currentEssence = essence;
        UpdateFlaskAppearance(); // Cambiar color según la esencia
    }

    private void UpdateFlaskAppearance()
    {
        Renderer renderer = GetComponent<Renderer>();
        renderer.material.color = currentEssence != null ?
            currentEssence.essenceColor : Color.black; // Negro = mezcla inválida
    }


    // Al colisionar
    private void OnCollisionEnter(Collision collision)
    {
        if (currentEssence == null)
        {
            return;
        }

        GameObject effectPrefab = isRoundFlask ?
            currentEssence.roundFlaskEffect :
            currentEssence.squareFlaskEffect;

        Instantiate(effectPrefab, transform.position, Quaternion.identity);
        Destroy(gameObject);
    }


}