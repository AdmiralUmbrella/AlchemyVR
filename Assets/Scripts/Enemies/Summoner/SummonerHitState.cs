using UnityEngine;
using UnityEngine.AI;

/// <summary>
/// Estado de golpe/aturdimiento para el Summoner.
/// Equivalente a EnemyHitState, con knockback.
/// </summary>
public class SummonerHitState : SummonerBaseState
{
    public SummonerHitState(SummonerStateManager manager, SummonerData data)
        : base(manager, data) { }

    public override void Enter()
    {
        Debug.Log("Summoner entró en estado: HIT");

        summonerData.isStunned = true;
        summonerData.currentStunTime = summonerData.stunDuration;

        manager.StopAgent();

        // Animación de golpe
        if (summonerData.animator != null)
        {
            summonerData.animator.ResetTrigger("Summon");
            summonerData.animator.SetBool("IsMoving", false);
            summonerData.animator.SetTrigger("Hit");
        }
    }

    public override void Update()
    {
        // Reducimos el tiempo de stun
        summonerData.currentStunTime -= Time.deltaTime;

        // Aplicar knockback durante la primera mitad del stun
        if (summonerData.currentStunTime > summonerData.stunDuration * 0.5f)
        {
            ApplyKnockback();
        }

        // Si acaba el stun, elegimos a dónde ir
        if (summonerData.currentStunTime <= 0f)
        {
            bool inRange = manager.CheckForTargetsInRange(summonerData.detectionRange);
            if (inRange)
            {
                manager.ChangeState(manager.ChaseState);
            }
            else
            {
                manager.ChangeState(manager.IdleState);
            }
        }
    }

    public override void Exit()
    {
        Debug.Log("Summoner saliendo de estado: HIT");
        summonerData.isStunned = false;
    }

    private void ApplyKnockback()
    {
        NavMeshAgent navAgent = manager.GetComponent<NavMeshAgent>();
        if (navAgent != null && summonerData.knockbackDirection != Vector3.zero)
        {
            Vector3 knock = summonerData.knockbackDirection * summonerData.knockbackForce;
            navAgent.Move(knock * Time.deltaTime);
        }
    }
}
