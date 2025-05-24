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
    [SerializeField] private Image cooldownUIImage;   // Image tipo Filled

    // Objetos vivos (no destruidos)
    private readonly List<GameObject> activeObjects = new();

    // Objetos que siguen DENTRO del trigger
    private readonly HashSet<GameObject> objectsInside = new();

    private bool isCooldown = false;

    /*---------------------- Ciclo de vida ----------------------*/
    private void Start()
    {
        if (spawnPoint == null) spawnPoint = transform;
        SpawnOne();                     // Generamos el primero
    }

    private void OnEnable()
    {
        PurgeNulls();
        objectsInside.Clear();          // El trigger volverá a poblarla
        isCooldown = false;
    }

    /*---------------------- TRIGGER ----------------------------*/
    private void OnTriggerEnter(Collider other)
    {
        // Si pertenece a los objetos spawneados lo añadimos al set
        if (activeObjects.Contains(other.gameObject))
            objectsInside.Add(other.gameObject);
    }

    private void OnTriggerExit(Collider other)
    {
        if (!activeObjects.Contains(other.gameObject)) return;

        // Sale del área: lo quitamos del set
        objectsInside.Remove(other.gameObject);

        // ¿Se ha quedado la torre vacía?
        if (objectsInside.Count == 0 &&
            !isCooldown &&
            activeObjects.Count < maxSimultaneousObjects)
        {
            StartCoroutine(SpawnCooldown());
        }
    }

    /*----------------- Notificación de destrucción -------------*/
    public void NotifyObjectDestroyed(GameObject obj)
    {
        activeObjects.Remove(obj);
        objectsInside.Remove(obj);   // Por si muere dentro

        if (!isCooldown && activeObjects.Count < maxSimultaneousObjects)
            StartCoroutine(SpawnCooldown());
    }

    /*------------------- Cool-down & Spawn ---------------------*/
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

    private void SpawnOne()
    {
        PurgeNulls();
        if (activeObjects.Count >= maxSimultaneousObjects) return;

        GameObject obj = Instantiate(
            objectPrefab,
            spawnPoint.position,
            spawnPoint.rotation,
            transform);

        // Añadimos notificador dinámicamente
        var notifier = obj.AddComponent<SpawnedNotifier>();
        notifier.Init(this);

        activeObjects.Add(obj);
        objectsInside.Add(obj);   // Nace DENTRO del trigger
    }

    /*----------------اريات Utilidad --------------------------*/
    private void PurgeNulls() => activeObjects.RemoveAll(g => g == null);
}