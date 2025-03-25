using UnityEngine;

public class EnemyHitState : EnemyBaseState
{
    public EnemyHitState(EnemyStateManager manager, EnemyData enemyData)
        : base(manager, enemyData) { }

    public override void Enter()
    {
        Debug.Log("Enemigo entró en estado: HIT");

        // Marcar que está aturdido
        enemyData.isStunned = true;
        // Asignar el tiempo de stun que durará este estado
        enemyData.currentHitStunTime = enemyData.hitStunDuration;

        // Detener movimiento
        if (enemyData.agent != null)
        {
            enemyData.agent.isStopped = true;
            enemyData.agent.velocity = Vector3.zero;
        }

        // Activar animación de golpe (si corresponde)
        if (enemyData.animator != null)
        {
            enemyData.animator.ResetTrigger("Attack");
            enemyData.animator.SetBool("IsMoving", false);
            enemyData.animator.SetTrigger("Hit");
        }
    }

    public override void Update()
    {
        // Reducimos el tiempo de stun
        enemyData.currentHitStunTime -= Time.deltaTime;

        // Aplicar knockback durante la primera parte del stun (opcional)
        if (enemyData.currentHitStunTime > enemyData.hitStunDuration * 0.5f)
        {
            ApplyKnockback();
        }

        // Cuando se acaba el stun
        if (enemyData.currentHitStunTime <= 0f)
        {
            // Salir del estado de hit
            // Podemos decidir a qué estado regresar.
            // Ejemplo: si el jugador está cerca, volvemos a Chase, si no, Idle.
            if (manager.IsPlayerInRange(enemyData.detectionRange))
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
        Debug.Log("Enemigo saliendo de estado: HIT");

        // Quitar stun
        enemyData.isStunned = false;

        // Reactivar agente, ya que salimos del estado de golpe
        if (enemyData.agent != null)
        {
            enemyData.agent.isStopped = false;
        }
    }

    /// <summary>
    /// Aplica la fuerza de knockback para empujar al enemigo
    /// </summary>
    private void ApplyKnockback()
    {
        if (enemyData.agent != null && enemyData.knockbackDirection != Vector3.zero)
        {
            Vector3 knockback = enemyData.knockbackDirection * enemyData.knockbackForce;
            enemyData.agent.Move(knockback * Time.deltaTime);
        }
    }
}
