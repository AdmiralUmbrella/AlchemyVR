using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Área que aplica todos los efectos de la Essence cada cierto “tick”.
/// </summary>
public class AoEArea : MonoBehaviour
{
    [Tooltip("Essence que contiene los PotionEffectSO a aplicar")]
    public EssenceSO essence;

    [Header("Timings")]
    public float lifeTime = 4f;   // Se destruye solo
    public float tickRate = 0.5f; // Frecuencia de aplicación

    private readonly HashSet<IEnemy> _inside = new();
    private float _timer;

    void Update()
    {
        _timer += Time.deltaTime;
        if (_timer >= tickRate)
        {
            foreach (var enemy in _inside)
                ApplyAllEffects(enemy);

            _timer = 0f;
        }

        lifeTime -= Time.deltaTime;
        if (lifeTime <= 0f) Destroy(gameObject);
    }

    void OnTriggerEnter(Collider other)
    {
        var enemy = other.GetComponent<IEnemy>();
        if (enemy != null) _inside.Add(enemy);
    }

    void OnTriggerExit(Collider other)
    {
        var enemy = other.GetComponent<IEnemy>();
        if (enemy != null) _inside.Remove(enemy);
    }

    void ApplyAllEffects(IEnemy enemy)
    {
        if (essence == null) return;
        foreach (var fx in essence.effectsToApply)
            fx.ApplyEffect(enemy, transform.position);
    }
}