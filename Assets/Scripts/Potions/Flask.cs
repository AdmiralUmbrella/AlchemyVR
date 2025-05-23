using UnityEngine;

public class Flask : MonoBehaviour
{
    [Header("Liquid Settings")]
    [SerializeField] private int liquidMaterialIndex = 2; // �ndice del material del l�quido

    private Material liquidMaterialInstance; // Instancia �nica del material del l�quido
    private Renderer flaskRenderer;


    [Header("Settings")]
    [SerializeField] private bool isRoundFlask; // Definido en el Inspector
    
    [Header("AoE Settings")]
    [SerializeField] private GameObject aoeAreaPrefab; // Nuevo

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
        UpdateLiquidColor(); // Cambiar color seg�n la esencia
    }
    private void InitializeLiquidMaterial()
    {
        if (flaskRenderer == null || liquidMaterialIndex >= flaskRenderer.materials.Length) return;

        // Crear instancia �nica del material del l�quido
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
        liquidMaterialInstance.SetColor("_FresnelColor", currentEssence.essenceColor); // Cambiar el da�o base
    }
    // Al colisionar
    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("RoundPotion") || collision.gameObject.CompareTag("SquarePotion"))
        {
            return;
        }
        
        if (currentEssence == null)
        {
            return;
        }

        // Se chequea si colision� con alg�n enemigo que implemente IEnemy.
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

        if (!isRoundFlask && aoeAreaPrefab)
        {
            // Lanzamos un raycast corto hacia abajo desde el punto de impacto
            RaycastHit hit;
            Vector3 origin = transform.position;
            if (Physics.Raycast(origin, Vector3.down, out hit, 2f, 7))
                origin = hit.point;

            GameObject areaObj = Instantiate(aoeAreaPrefab, origin, Quaternion.identity);

            if (areaObj.TryGetComponent(out AoEArea area))
                area.essence = currentEssence;
        }

        //Destruir objeto
        Destroy(gameObject);
    }


}