using UnityEngine;

public class Wobble : MonoBehaviour
{
    [Header("Material Settings")]
    [SerializeField] private int liquidMaterialIndex = 2;

    private Material liquidMaterial;
    private Vector3 lastPos;
    private Vector3 lastRot;

    [Header("Wobble Settings")]
    public float MaxWobble  = 0.03f;
    public float WobbleSpeed = 1f;
    public float Recovery    = 1f;

    private float wobbleAmountToAddX, wobbleAmountToAddZ;
    private float wobbleAmountX,     wobbleAmountZ;
    private float pulse;
    private float time;

    // ──────────────────────────────────────────────────────────────
    void Start()
    {
        // Igual que antes: recuperar material...
        Flask flask = GetComponent<Flask>();
        if (flask != null) liquidMaterial = flask.GetLiquidMaterial();
        else
        {
            Renderer r = GetComponent<Renderer>();
            if (r && liquidMaterialIndex < r.materials.Length)
                liquidMaterial = r.materials[liquidMaterialIndex];
        }

        lastPos = transform.position;
        lastRot = transform.rotation.eulerAngles;
    }

    // ──────────────────────────────────────────────────────────────
    void Update()
    {
        if (liquidMaterial == null) return;

        // ► AHORA USAMOS TIEMPO SIN ESCALAR
        float dt = Time.unscaledDeltaTime;

        time += dt;

        // Recuperación hacia 0
        wobbleAmountToAddX = Mathf.Lerp(wobbleAmountToAddX, 0, dt * Recovery);
        wobbleAmountToAddZ = Mathf.Lerp(wobbleAmountToAddZ, 0, dt * Recovery);

        // Senoide
        pulse          = 2 * Mathf.PI * WobbleSpeed;
        wobbleAmountX  = wobbleAmountToAddX * Mathf.Sin(pulse * time);
        wobbleAmountZ  = wobbleAmountToAddZ * Mathf.Sin(pulse * time);

        // Enviarlo al shader
        liquidMaterial.SetFloat("_WobbleX", wobbleAmountX);
        liquidMaterial.SetFloat("_WobbleZ", wobbleAmountZ);

        // Velocidades (usa dt para evitar división por 0)
        Vector3 velocity        = (lastPos - transform.position) / dt;
        Vector3 angularVelocity = (transform.rotation.eulerAngles - lastRot);

        wobbleAmountToAddX += Mathf.Clamp(
            (velocity.x + angularVelocity.z * 0.2f) * MaxWobble,
            -MaxWobble,
            MaxWobble);

        wobbleAmountToAddZ += Mathf.Clamp(
            (velocity.z + angularVelocity.x * 0.2f) * MaxWobble,
            -MaxWobble,
            MaxWobble);

        lastPos = transform.position;
        lastRot = transform.rotation.eulerAngles;
    }
}
