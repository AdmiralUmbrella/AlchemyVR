using System.Collections;
using UnityEngine;
using UnityEngine.Serialization;

public class ObjectSpawner : MonoBehaviour
{
    [FormerlySerializedAs("essencePrefab")] [Header("Configuración del Spawner")] [Tooltip("Prefab de la esencia a spawnear")]
    public GameObject objectPrefab;
    
    [Tooltip("Tiempo de cooldown (en segundos) antes de respawnear la esencia")]
    public float cooldownTime = 10f;

    [Tooltip("Transform de referencia para el spawn. Si no se asigna, se usa la posición y rotación del objeto con este script")]
    public Transform spawnPoint;

    private GameObject currentObject;
    private bool isCooldown = false;

    void Start()
    {
        // Si no se asigna un spawnPoint, se usa el propio transform de este GameObject.
        if (spawnPoint == null)
        {
            spawnPoint = this.transform;
        }

        SpawnEssence();
    }

    void Update()
    {
        // Si no hay esencia en la posición y no se está en cooldown, inicia el cooldown para re-spawnear.
        if (currentObject == null && !isCooldown)
        {
            StartCoroutine(SpawnCooldown());
        }
    }

// Corrutina que espera el tiempo de cooldown antes de spawnear una nueva esencia.
    IEnumerator SpawnCooldown()
    {
        isCooldown = true;
        yield return new WaitForSeconds(cooldownTime);
        SpawnEssence();
        isCooldown = false;
    }

// Método que instancia el prefab de la esencia en la posición y rotación definidas.
    void SpawnEssence()
    {
        currentObject = Instantiate(objectPrefab, spawnPoint.position, spawnPoint.rotation);
    }

}