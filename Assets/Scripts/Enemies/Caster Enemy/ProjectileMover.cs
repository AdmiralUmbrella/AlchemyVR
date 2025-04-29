using UnityEngine;

/// <summary>
/// Hace que el objeto avance constantemente en su eje +Z (forward).
/// Usado junto con PoolManager para reciclar instancias.
/// </summary>
[RequireComponent(typeof(PooledEffect))]   // o tu clase equivalente
public class ProjectileMover : MonoBehaviour
{
    [Tooltip("Velocidad en m/s.")]
    public float speed = 20f;

    private void Update()
    {
        transform.position += transform.forward * (speed * Time.deltaTime);
    }
}