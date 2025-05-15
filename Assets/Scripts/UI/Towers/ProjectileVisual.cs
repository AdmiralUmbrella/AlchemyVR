using UnityEngine;

[RequireComponent(typeof(Renderer))]
public class ProjectileVisual : MonoBehaviour
{
    private Vector3 direction;
    private float   remainingDistance;
    private float   speed;
    private ProjectilePool pool;

    private Renderer rend;
    private MaterialPropertyBlock mpb;

    /// <summary>Se llama justo después de obtenerlo del pool.</summary>
    public void Init(Vector3 destination,
        Color   essenceColor,
        float   speed,
        ProjectilePool pool)
    {
        this.pool  = pool;
        this.speed = speed;

        if (!rend) { rend = GetComponent<Renderer>(); mpb = new MaterialPropertyBlock(); }

        mpb.SetColor("_BaseColor", essenceColor * 4f);      // Intensidad ×4
        rend.SetPropertyBlock(mpb);

        // ➊ Calcular la dirección UNA sola vez
        direction         = (destination - transform.position).normalized;
        remainingDistance = Vector3.Distance(transform.position, destination);
        
        gameObject.SetActive(true);
    }

    private void Update()
    {
        float step = speed * Time.deltaTime;
        transform.position += direction * step;
        remainingDistance  -= step;

        if (remainingDistance <= 0f)                // llegó al destino
            pool.ReturnToPool(this);
    }
}