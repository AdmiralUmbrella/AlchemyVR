using System.Collections;
using UnityEngine;
using UnityEngine.Serialization;

public class ObjectSpawner : MonoBehaviour
{
    [Header("Configuración del Spawner")]
    [Tooltip("Prefab de la esencia a spawnear")]
    public GameObject objectPrefab;

    [Tooltip("Tiempo de cooldown (en segundos) antes de respawnear la esencia")]
    public float cooldownTime = 10f;

    [Tooltip("Transform de referencia para el spawn. Si no se asigna, se usa la posición y rotación del objeto con este script")]
    public Transform spawnPoint;

    [Header("Parent Container")]
    [Tooltip("Donde queremos que queden parentadas las esencias. Por ejemplo el interior de la torre.")]
    public Transform spawnParent;

    private GameObject currentObject;
    private bool isCooldown = false;

    void Start()
    {
        // Si no se asigna un punto de spawn, usar este mismo objeto
        if (spawnPoint == null)
            spawnPoint = transform;

        // Si no se asigna un parent, usar el mismo spawnPoint
        if (spawnParent == null)
            spawnParent = spawnPoint;

        SpawnEssence();
    }

    void Update()
    {
        if (currentObject == null && !isCooldown)
        {
            StartCoroutine(SpawnCooldown());
        }
    }

    IEnumerator SpawnCooldown()
    {
        isCooldown = true;
        yield return new WaitForSeconds(cooldownTime);
        SpawnEssence();
        isCooldown = false;
    }

    void SpawnEssence()
    {
        currentObject = Instantiate(
            objectPrefab,
            spawnPoint.position,
            spawnPoint.rotation,
            spawnParent
        );
    }
}
