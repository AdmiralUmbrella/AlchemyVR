using UnityEngine;

public class EnemyAttackState : EnemyBaseState
{
    /// <summary>
    /// Constructor que llama a la clase base EnemyBaseState.
    /// </summary>
    public EnemyAttackState(EnemyStateManager manager, EnemyData enemyData)
        : base(manager, enemyData) { }

    public override void Enter()
    {
        Debug.Log("Enemigo entró en estado: ATTACK");

        // Detener el movimiento durante el ataque
        manager.StopAgent();

        // Reiniciar tiempo de duración del ataque
        enemyData.currentAttackTime = enemyData.attackDuration;

        // Asegurarnos de que aún no se haya aplicado daño
        enemyData.hasDealtDamage = false;

        // Hacer que el enemigo mire al objetivo
        LookAtTarget();

        // Iniciar animación de ataque
        if (enemyData.animator != null)
        {
            enemyData.animator.SetTrigger("Attack");
        }
    }

    public override void Update()
    {
        // Reducir el temporizador del ataque
        enemyData.currentAttackTime -= Time.deltaTime;

        // Determinar el momento en que se aplica el daño
        // Ej: si damageDelay = 0.5, entonces lo aplicamos cuando
        // currentAttackTime <= (attackDuration - 0.5)
        if (!enemyData.hasDealtDamage &&
            enemyData.currentAttackTime <= (enemyData.attackDuration - enemyData.damageDelay))
        {
            // Intentar hacer daño al objetivo
            DealDamage();
            enemyData.hasDealtDamage = true;  // Evitamos aplicarlo más de una vez
        }

        // Cuando se acaba la duración de la animación de ataque
        if (enemyData.currentAttackTime <= 0f)
        {
            // Revisamos si el enemigo sigue cerca para volver a atacar,
            // o si se alejó para regresar a Chase
            float distanceToPlayer = DistanceToTarget();

            if (distanceToPlayer <= enemyData.attackRange)
            {
                // Si el cooldown ya terminó, atacamos de nuevo
                if (enemyData.attackCooldownTimer <= 0)
                {
                    manager.ChangeState(manager.AttackState);
                }
                else
                {
                    // Si hay cooldown, mejor perseguir (o quedarse cerca)
                    manager.ChangeState(manager.ChaseState);
                }
            }
            else
            {
                // Si el jugador/torre se aleja, ir a Chase
                manager.ChangeState(manager.ChaseState);
            }
        }
    }

    public override void Exit()
    {
        Debug.Log("Enemigo saliendo de estado: ATTACK");

        // Iniciamos el cooldown de ataque
        enemyData.attackCooldownTimer = enemyData.attackCooldown;
    }

    /// <summary>
    /// Hace que el enemigo gire para mirar hacia el objetivo antes de atacar.
    /// </summary>
    private void LookAtTarget()
    {
        if (enemyData.playerTransform == null) return;

        Vector3 directionToPlayer = enemyData.playerTransform.position -
                                    enemyData.agent.transform.position;

        // Asegurarnos de girar solo en plano XZ
        directionToPlayer.y = 0;

        // Aplicar rotación
        enemyData.agent.transform.rotation = Quaternion.LookRotation(directionToPlayer);
    }

    /// <summary>
    /// Aplica el daño al objetivo, en caso de que siga en rango.
    /// </summary>
    private void DealDamage()
    {
        // Verificamos si seguimos teniendo referencia al objetivo
        if (enemyData.playerTransform == null) return;

        // Revisar si realmente está en rango
        if (DistanceToTarget() <= enemyData.attackRange)
        {
            // Aquí podríamos encontrar el script de vida del jugador/torre
            // o, como en el ejemplo, un PlayerStateManager.
            // Si es un tower defense, busca el componente "TowerHealth" o similar.

        }
    }

    /// <summary>
    /// Calcula la distancia al objetivo (jugador/torre).
    /// </summary>
    private float DistanceToTarget()
    {
        if (enemyData.playerTransform == null) return float.MaxValue;

        return Vector3.Distance(
            enemyData.agent.transform.position,
            enemyData.playerTransform.position
        );
    }
}
