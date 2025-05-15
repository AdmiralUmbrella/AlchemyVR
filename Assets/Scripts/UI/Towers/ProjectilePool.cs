using System.Collections.Generic;
using UnityEngine;

public class ProjectilePool : MonoBehaviour
{
    [SerializeField] private ProjectileVisual prefab;
    private readonly Queue<ProjectileVisual> pool = new();

    public ProjectileVisual Get()
    {
        return pool.Count > 0 ? pool.Dequeue() : Instantiate(prefab, transform);
    }

    public void ReturnToPool(ProjectileVisual proj)
    {
        proj.gameObject.SetActive(false);
        pool.Enqueue(proj);
    }
}