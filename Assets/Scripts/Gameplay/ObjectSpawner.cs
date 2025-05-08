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

    // ===== Estado interno =====
    private readonly List<GameObject> activeObjects = new();
    private bool isCooldown = false;

    private void Start()
    {
        if (spawnPoint == null) spawnPoint = transform;
        SpawnOne();                // Solo UNO al comienzo
    }

    /*-------------------------------------------------------
     *  TRIGGERS DE ÁREA
     *------------------------------------------------------*/

    // Cuando un objeto nacido aquí sale del trigger → comenzar cooldown
    private void OnTriggerExit(Collider other)
    {
        if (activeObjects.Contains(other.gameObject) &&
            !isCooldown &&
            activeObjects.Count < maxSimultaneousObjects)
        {
            StartCoroutine(SpawnCooldown());
        }
    }

    /*-------------------------------------------------------
     *  NOTIFICACIÓN DESDE LOS OBJETOS
     *------------------------------------------------------*/
    public void NotifyObjectDestroyed(GameObject obj)
    {
        activeObjects.Remove(obj);

        if (!isCooldown && activeObjects.Count < maxSimultaneousObjects)
            StartCoroutine(SpawnCooldown());
    }

    /*-------------------------------------------------------
     *  SPAWN & COOLDOWN
     *------------------------------------------------------*/
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
                cooldownUIImage.fillAmount = t / cooldownTime;   // 0→1

            yield return null;
        }

        if (cooldownUIImage != null)
            cooldownUIImage.gameObject.SetActive(false);

        SpawnOne();      // Sólo uno por ciclo
        isCooldown = false;
    }

    private void SpawnOne()
    {
        if (activeObjects.Count >= maxSimultaneousObjects) return;

        GameObject obj = Instantiate(
            objectPrefab,
            spawnPoint.position,
            spawnPoint.rotation);

        // Añadimos el notificador dinámicamente
        var notifier = obj.AddComponent<SpawnedNotifier>();
        notifier.Init(this);

        activeObjects.Add(obj);
    }
}
