using UnityEngine;

/// <summary>
/// Adjunta esto a los prefabs que quieras "autodespawner".
/// Ejemplo típico: un efecto con ParticleSystem.
/// </summary>
public class PooledEffect : MonoBehaviour
{
    [Tooltip("Clave usada por el PoolManager (debe coincidir con la que uses en Spawn).")]
    public string poolKey;

    [Tooltip("Segundos tras los cuales se devuelve al pool; si es 0 usa vida del ParticleSystem.")]
    public float lifeTimeOverride = 0f;

    private float life;

    private void OnEnable()
    {
        life = lifeTimeOverride > 0f ? lifeTimeOverride :
            TryGetComponent(out ParticleSystem ps) ? ps.main.duration : 1f;
    }

    private void Update()
    {
        life -= Time.deltaTime;
        if (life <= 0f)
            PoolManager.Instance.Despawn(poolKey, gameObject);
    }
}