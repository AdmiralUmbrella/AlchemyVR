using UnityEngine;
using System.Collections;

public class TowerDestroyedState : BaseState<TowerState>
{
    private TowerAI towerManager;
    private Animator animator;

    public TowerDestroyedState(TowerAI manager) : base(TowerState.Destroyed)
    {
        towerManager = manager;
        animator = towerManager.towerData.towerAnimator;
    }

    public override void EnterState()
    {
        Debug.Log("Torre en estado DESTROYED: reproduciendo animación de destrucción.");
        towerManager.towerData.towerCrystal.enabled = false;
        if (animator != null)
        {
            animator.ResetTrigger("Idle");
            animator.SetTrigger("Destroyed");
        }
        towerManager.StartCoroutine(DestroyTowerAfterDelay());
    }

    private IEnumerator DestroyTowerAfterDelay()
    {
        yield return new WaitForSeconds(2f); // Ajusta el delay según la duración de la animación
        GameObject.Destroy(towerManager.transform.parent.gameObject);
    }

    public override void ExitState() { }
    public override void UpdateState() { }
    public override TowerState GetNextState() => TowerState.Destroyed;
    public override void OnTriggerEnter(Collider other) { }
    public override void OnTriggerStay(Collider other) { }
    public override void OnTriggerExit(Collider other) { }
}