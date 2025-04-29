using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Pool;

/// <summary>
/// Singleton muy ligero para pooling de GameObjects.
/// Clave = string (por ejemplo "CasterBeam").
/// Las instancias deben tener <see cref="PooledEffect"/> para auto-devolverse.
/// </summary>
public class PoolManager : MonoBehaviour
{
    public static PoolManager Instance { get; private set; }

    private readonly Dictionary<string, Queue<GameObject>> pools = new();

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(this);      // opcional
    }

    /// <summary>Pide un objeto del pool (o lo crea si se agotó).</summary>
    public GameObject Spawn(string key, GameObject prefab, Vector3 pos, Quaternion rot)
    {
        if (!pools.TryGetValue(key, out var q) || q.Count == 0)
        {
            // Sin stock: crea uno nuevo
            GameObject go = Instantiate(prefab, pos, rot);
            Mark(go, key);
            return go;
        }

        GameObject obj = q.Dequeue();
        obj.transform.SetPositionAndRotation(pos, rot);
        obj.SetActive(true);
        return obj;
    }

    /// <summary>Devuelve un objeto al pool.</summary>
    public void Despawn(string key, GameObject obj)
    {
        obj.SetActive(false);
        if (!pools.TryGetValue(key, out var q))
            pools[key] = q = new Queue<GameObject>();
        q.Enqueue(obj);
    }

    /* -------------- Helpers -------------- */
    private void Mark(GameObject go, string key)
    {
        // Asegura que tenga un PooledObject para auto-despawn
        if (!go.TryGetComponent(out PooledEffect p))
            p = go.AddComponent<PooledEffect>();

        p.poolKey = key;
    }
}