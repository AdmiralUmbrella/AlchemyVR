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
        fireTimer = towerManager.towerData.fireInterval;
        activeTimer = towerManager.towerData.activeDuration;
    }

    public override void ExitState() { }

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
        GameObject enemy = towerManager.GetTargetEnemy();
        if (enemy != null && towerManager.activeEssence != null)
        {
            IEnemy enemyInterface = enemy.GetComponent<IEnemy>();
            if (enemyInterface != null)
            {
                // Se recorren todos los efectos definidos en el EssenceSO y se aplican
                foreach (PotionEffectSO effect in towerManager.activeEssence.effectsToApply)
                {
                    effect.ApplyEffect(enemyInterface, towerManager.transform.position);
                }

                // Instanciar el efecto de la poción (ejemplo usando 'roundFlaskEffect')
                if (towerManager.activeEssence.roundFlaskEffect != null)
                {
                    UnityEngine.Object.Instantiate(towerManager.activeEssence.roundFlaskEffect,
                        enemy.transform.position, Quaternion.identity);
                }

                Debug.Log("Disparo realizado con los efectos de: " + towerManager.activeEssence.essenceName);
            }
        }
    }


    public override void OnTriggerEnter(Collider other) { }
    public override void OnTriggerStay(Collider other) { }
    public override void OnTriggerExit(Collider other) { }
}
