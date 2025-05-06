using System.Collections;
using UnityEngine;
using UnityEngine.UI;   // <-- UI

public class ObjectSpawner : MonoBehaviour
{
    [Header("Spawner")]
    [Tooltip("Prefab del objeto a spawnear")]
    [SerializeField] private GameObject objectPrefab;

    [Tooltip("Tiempo de cooldown (s) antes de volver a spawnear")]
    [SerializeField] private float cooldownTime = 10f;

    [Tooltip("Máx. veces que puede generar objetos")]
    [SerializeField] private int maxSpawnCount = 3;

    [Tooltip("Punto exacto donde aparece el objeto (opcional)")]
    [SerializeField] private Transform spawnPoint;

    [Header("UI")]
    [Tooltip("Imagen con tipo 'Filled' que actúa como barra de progreso")]
    [SerializeField] private Image cooldownUIImage;

    private GameObject currentObject;
    private int spawnCount = 0;
    private bool isCooldown = false;

    private void Start()
    {
        if (spawnPoint == null) spawnPoint = transform;
        SpawnObject();

        if (cooldownUIImage != null)
            cooldownUIImage.gameObject.SetActive(false);
    }

    // Detecta cuando el objeto abandonó el área del spawner
    private void OnTriggerExit(Collider other)
    {
        if (other.gameObject == currentObject && !isCooldown && spawnCount < maxSpawnCount)
        {
            StartCoroutine(SpawnCooldown());
        }
    }

    // Corrutina de cooldown con relleno de la imagen
    private IEnumerator SpawnCooldown()
    {
        isCooldown = true;

        // Mostrar UI y reiniciar fill
        if (cooldownUIImage != null)
        {
            cooldownUIImage.gameObject.SetActive(true);
            cooldownUIImage.fillAmount = 0f;
        }

        float timer = 0f;
        while (timer < cooldownTime)
        {
            timer += Time.deltaTime;

            if (cooldownUIImage != null)
                cooldownUIImage.fillAmount = timer / cooldownTime; // 0‑1

            yield return null;
        }

        // Ocultar la UI cuando termine
        if (cooldownUIImage != null)
            cooldownUIImage.gameObject.SetActive(false);

        SpawnObject();
        isCooldown = false;
    }

    // Instancia el prefab si no se superó el límite
    private void SpawnObject()
    {
        if (spawnCount >= maxSpawnCount) return;

        currentObject = Instantiate(objectPrefab, spawnPoint.position, spawnPoint.rotation);
        spawnCount++;
    }
}
