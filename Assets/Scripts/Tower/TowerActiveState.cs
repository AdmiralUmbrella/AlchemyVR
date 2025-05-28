using UnityEngine;
using System.Collections;

public class TowerActiveState : BaseState<TowerState>
{
    private TowerAI towerManager;
    private float fireTimer;
    private float activeTimer;

    public TowerActiveState(TowerAI manager) : base(TowerState.Active)
    {
        towerManager = manager;
    }

    public override void EnterState()
    {
        Debug.Log("Torre en estado ACTIVE: disparando.");
        towerManager.towerData.towerCrystal.ResetTrigger("Idle");
        towerManager.towerData.towerCrystal.SetTrigger("Active");
        fireTimer = towerManager.towerData.fireInterval;
        activeTimer = towerManager.towerData.activeDuration;
    }

    public override void ExitState()
    {
    }

    public override void UpdateState()
    {
        activeTimer -= Time.deltaTime;
        fireTimer -= Time.deltaTime;
        if (fireTimer <= 0f)
        {
            FireAtTarget();
            fireTimer = towerManager.towerData.fireInterval;
        }
    }

    public override TowerState GetNextState()
    {
        if (activeTimer <= 0)
            return TowerState.Cooldown;
        return TowerState.Active;
    }

    /// <summary>
    /// Busca al enemigo más cercano y aplica cada efecto definido en el EssenceSO activo.
    /// </summary>
    private void FireAtTarget()
    {
        towerManager.audioSource.PlayOneShot(towerManager.towerData.fireSound);
        
        GameObject enemy = towerManager.GetTargetEnemy();
        if (enemy == null || towerManager.activeEssence == null) return;

        // --- VISUAL -------------------------------------------------------
        Vector3 hitPoint = enemy.transform.position;
        float distance = Vector3.Distance(towerManager.towerData.towerCrystal.transform.position, hitPoint);
        float travelTime = distance / towerManager.towerData.projectileSpeed;

        var proj = towerManager.towerData.projectilePool.Get();
        proj.transform.SetParent(null, true);
        proj.transform.position = towerManager.towerData.towerCrystal.transform.position;
        proj.Init(hitPoint,
            towerManager.activeEssence.essenceColor,
            towerManager.towerData.projectileSpeed,
            towerManager.towerData.projectilePool);
        // -----------------------------------------------------------------

        towerManager.StartCoroutine(ApplyEffectsAfterDelay(enemy.GetComponent<IEnemy>(),
            travelTime));
    }

    private IEnumerator ApplyEffectsAfterDelay(IEnemy enemy, float delay)
    {
        yield return new WaitForSeconds(delay);

        if (enemy == null) yield break; // el enemigo pudo morir antes

        foreach (PotionEffectSO effect in towerManager.activeEssence.effectsToApply)
            effect.ApplyEffect(enemy, towerManager.transform.position);

        if (towerManager.activeEssence.roundFlaskEffect != null)
            Object.Instantiate(towerManager.activeEssence.roundFlaskEffect,
                enemy.EnemyTransform.position, Quaternion.identity);

        Debug.Log($"Disparo aplicado ({towerManager.activeEssence.essenceName})");
    }


    public override void OnTriggerEnter(Collider other)
    {
    }

    public override void OnTriggerStay(Collider other)
    {
    }

    public override void OnTriggerExit(Collider other)
    {
    }
}