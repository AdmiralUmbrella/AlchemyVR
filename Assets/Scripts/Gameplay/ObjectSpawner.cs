using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ObjectSpawner : MonoBehaviour
{
    [Header("Spawner")]
    [SerializeField] private GameObject objectPrefab;
    [SerializeField] private float cooldownTime = 10f;
    [SerializeField] private int maxSimultaneousObjects = 3;
    [SerializeField] private Transform spawnPoint;

    [Header("UI (opcional)")]
    [SerializeField] private Image cooldownUIImage;

    /* ---------- Estado interno ---------- */
    private readonly List<GameObject>   activeObjects = new();
    private readonly HashSet<GameObject>objectsInside = new();

    private bool isCooldown   = false;
    private bool spawnQueued  = false;   // <- NUEVO

    // Por-objeto indicamos si YA provocó una reposición
    private readonly Dictionary<GameObject, bool> respawnDone =
        new();

    /*-------------- Ciclo ----------------*/
    private void Start()
    {
        if (spawnPoint == null) spawnPoint = transform;
        SpawnOne();
    }

    /*-------------- Trigger --------------*/
    private void OnTriggerStay(Collider other)
    {
        if (activeObjects.Contains(other.gameObject))
        {
            objectsInside.Add(other.gameObject);

            // Mientras el área NO esté vacía, cancelamos cualquier cola
            spawnQueued = false;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (!activeObjects.Contains(other.gameObject)) return;

        objectsInside.Remove(other.gameObject);

        // Si el área se quedó vacía, solicitamos respawn
        if (objectsInside.Count == 0)
            QueueSpawn();                       // (sin ‘force’)
    }

    /*------ Notificación de destrucción ------*/
    public void NotifyObjectDestroyed(GameObject obj)
    {
        activeObjects.Remove(obj);
        objectsInside.Remove(obj);

        // Sólo reponemos si el área está vacía y hay hueco
        if (objectsInside.Count == 0 &&
            activeObjects.Count < maxSimultaneousObjects)
        {
            QueueSpawn(force: true);
        }
    }

    /*------------- COLA DE RESPAWN ---------------*/
    private void QueueSpawn(bool force = false)
    {
        if (isCooldown) return;                     // ya hay temporizador

        bool spaceAvailable = activeObjects.Count < maxSimultaneousObjects;
        bool areaIsEmpty    = objectsInside.Count  == 0;

        if ((spaceAvailable && areaIsEmpty) || (force && areaIsEmpty))
        {
            spawnQueued = false;                    // se atiende ahora mismo
            StartCoroutine(SpawnCooldown());
        }
        else
        {
            spawnQueued = true;                     // esperar a que esté libre
        }
    }
    
    /*------------- Cool-down ---------------*/
    private void TryStartCooldown()
    {
        if (!isCooldown &&
            activeObjects.Count < maxSimultaneousObjects)
        {
            StartCoroutine(SpawnCooldown());
        }
    }

    private IEnumerator SpawnCooldown()
    {
        isCooldown = true;

        if (cooldownUIImage != null)
        {
            cooldownUIImage.gameObject.SetActive(true);
            cooldownUIImage.fillAmount = 0f;
        }

        float t = 0f;
        while (t < cooldownTime)
        {
            t += Time.deltaTime;
            if (cooldownUIImage != null)
                cooldownUIImage.fillAmount = t / cooldownTime;
            yield return null;
        }

        if (cooldownUIImage != null)
            cooldownUIImage.gameObject.SetActive(false);

        SpawnOne();
        isCooldown = false;
    }

    /*--------------- Spawn -----------------*/
    private void SpawnOne()
    {
        if (activeObjects.Count >= maxSimultaneousObjects) return;

        GameObject obj = Instantiate(
            objectPrefab,
            spawnPoint.position,
            spawnPoint.rotation,
            transform);

        var notifier = obj.AddComponent<SpawnedNotifier>();
        notifier.Init(this);

        activeObjects.Add(obj);
        objectsInside.Add(obj);
        respawnDone[obj] = false;   // Aún no lo hemos repuesto
    }
}