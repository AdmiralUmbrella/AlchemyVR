using UnityEngine;

/// Avisa a su ObjectSpawner cuando este objeto se destruye.
public class SpawnedNotifier : MonoBehaviour
{
    private ObjectSpawner owner;

    public void Init(ObjectSpawner spawner) => owner = spawner;

    private void OnDestroy()
    {
        owner?.NotifyObjectDestroyed(gameObject);
    }
}