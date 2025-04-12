using UnityEngine;

public class Flask : MonoBehaviour
{
    [Header("Liquid Settings")]
    [SerializeField] private int liquidMaterialIndex = 2; // Índice del material del líquido

    private Material liquidMaterialInstance; // Instancia única del material del líquido
    private Renderer flaskRenderer;


    [Header("Settings")]
    [SerializeField] private bool isRoundFlask; // Definido en el Inspector

    [Header("Explosion Settings")]
    [SerializeField] private GameObject explosionEffectPrefab;

    [HideInInspector]
    public EssenceSO currentEssence;
    
    private Rigidbody rb;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
        flaskRenderer = GetComponent<Renderer>();
        InitializeLiquidMaterial();
    }
    public Material GetLiquidMaterial() => liquidMaterialInstance;
    // Inicializar frasco con esencia (llamado por el caldero)
    public void InitializeFlask(EssenceSO essence)
    {
        currentEssence = essence;
        UpdateLiquidColor(); // Cambiar color según la esencia
    }
    private void InitializeLiquidMaterial()
    {
        if (flaskRenderer == null || liquidMaterialIndex >= flaskRenderer.materials.Length) return;

        // Crear instancia única del material del líquido
        liquidMaterialInstance = new Material(flaskRenderer.materials[liquidMaterialIndex]);
        Material[] materials = flaskRenderer.materials;
        materials[liquidMaterialIndex] = liquidMaterialInstance;
        flaskRenderer.materials = materials;
    }
    private void UpdateLiquidColor()
    {
        if (liquidMaterialInstance == null || currentEssence == null) return;

        // Modificar las propiedades del Shader Graph
        liquidMaterialInstance.SetColor("_FrontColor", currentEssence.essenceColor );    
        liquidMaterialInstance.SetColor("_BackColor", currentEssence.essenceColor * 0.8f); // Oscurecer para contraste
        liquidMaterialInstance.SetColor("_FresnelColor", currentEssence.essenceColor); // Cambiar el daño base
    }
    // Al colisionar
    private void OnCollisionEnter(Collision collision)
    {
        if (currentEssence == null)
        {
            return;
        }

        // Se chequea si colisionó con algún enemigo que implemente IEnemy.
        IEnemy enemyManager = collision.gameObject.GetComponent<IEnemy>();
        if (enemyManager != null && isRoundFlask)
        { 
            foreach (PotionEffectSO effect in currentEssence.effectsToApply)
            {
                effect.ApplyEffect(enemyManager, transform.position);
            }
        }

        //Efecto Visual
        GameObject effectPrefab = isRoundFlask ?
            currentEssence.roundFlaskEffect :
            currentEssence.squareFlaskEffect;
        Instantiate(effectPrefab, transform.position, Quaternion.identity);
        
        //Destruir objeto
        Destroy(gameObject);
    }


}