using UnityEngine;

public class Wobble : MonoBehaviour
{
    [Header("Material Settings")]
    [SerializeField] private int liquidMaterialIndex = 2; // Índice del material del líquido

    private Material liquidMaterial; // Instancia del material del líquido
    private Vector3 lastPos;
    private Vector3 velocity;
    private Vector3 lastRot;
    private Vector3 angularVelocity;

    [Header("Wobble Settings")]
    public float MaxWobble = 0.03f;
    public float WobbleSpeed = 1f;
    public float Recovery = 1f;

    private float wobbleAmountX;
    private float wobbleAmountZ;
    private float wobbleAmountToAddX;
    private float wobbleAmountToAddZ;
    private float pulse;
    private float time = 0.5f;

    private void Start()
    {
        // Obtener referencia al material del líquido desde Flask.cs
        Flask flask = GetComponent<Flask>();
        if (flask != null)
        {
            liquidMaterial = flask.GetLiquidMaterial();
        }
        else // Fallback si no hay Flask.cs
        {
            Renderer rend = GetComponent<Renderer>();
            if (rend != null && liquidMaterialIndex < rend.materials.Length)
            {
                liquidMaterial = rend.materials[liquidMaterialIndex];
            }
        }

        lastPos = transform.position;
        lastRot = transform.rotation.eulerAngles;
    }

    private void Update()
    {
        if (liquidMaterial == null) return;

        // Cálculos de tiempo
        time += Time.deltaTime;

        // Reducir wobble gradualmente
        wobbleAmountToAddX = Mathf.Lerp(wobbleAmountToAddX, 0, Time.deltaTime * Recovery);
        wobbleAmountToAddZ = Mathf.Lerp(wobbleAmountToAddZ, 0, Time.deltaTime * Recovery);

        // Generar onda senoidal
        pulse = 2 * Mathf.PI * WobbleSpeed;
        wobbleAmountX = wobbleAmountToAddX * Mathf.Sin(pulse * time);
        wobbleAmountZ = wobbleAmountToAddZ * Mathf.Sin(pulse * time);

        // Aplicar al material del líquido
        liquidMaterial.SetFloat("_WobbleX", wobbleAmountX);
        liquidMaterial.SetFloat("_WobbleZ", wobbleAmountZ);

        // Calcular velocidad y rotación
        velocity = (lastPos - transform.position) / Time.deltaTime;
        angularVelocity = transform.rotation.eulerAngles - lastRot;

        // Añadir efecto basado en movimiento
        wobbleAmountToAddX += Mathf.Clamp(
            (velocity.x + (angularVelocity.z * 0.2f)) * MaxWobble,
            -MaxWobble,
            MaxWobble
        );

        wobbleAmountToAddZ += Mathf.Clamp(
            (velocity.z + (angularVelocity.x * 0.2f)) * MaxWobble,
            -MaxWobble,
            MaxWobble
        );

        // Guardar últimos valores
        lastPos = transform.position;
        lastRot = transform.rotation.eulerAngles;
    }
}