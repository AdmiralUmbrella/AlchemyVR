using UnityEngine;

/// <summary>
/// Estado Casting del CasterEnemy. Durante este estado el enemigo se queda quieto y canaliza su hechizo.
/// Al finalizar el casteo, se realiza un raycast hacia el objetivo para aplicar daño, siempre que la línea de visión esté despejada.
/// </summary>
public class CasterEnemyCastingState : BaseState<CasterEnemyState>
{
    private CasterEnemyAI manager;
    private CasterEnemyData enemyData;

    /// <summary>
    /// Constructor del estado Casting.
    /// </summary>
    public CasterEnemyCastingState(CasterEnemyState stateKey, CasterEnemyAI manager, CasterEnemyData enemyData) : base(stateKey)
    {
        this.manager = manager;
        this.enemyData = enemyData;
    }

    public override void EnterState()
    {
        Debug.Log("CasterEnemy entró en estado: CASTING");
        enemyData.isCasting = true;
        enemyData.currentCastingTime = enemyData.castingTime;
        if (enemyData.agent != null)
        {
            enemyData.agent.isStopped = true;
        }
        if (enemyData.animator != null)
        {
            enemyData.animator.SetTrigger("Cast");
        }
    }

    public override void UpdateState()
    {
        enemyData.currentCastingTime -= Time.deltaTime;
        // Dibujar la línea del raycast para depuración
        if (enemyData.targetTransform != null)
        {
            Debug.DrawLine(manager.transform.position, enemyData.targetTransform.position, Color.red);
        }
    }

    public override CasterEnemyState GetNextState()
    {
        if (enemyData.currentCastingTime <= 0f)
        {
            // Al finalizar el casteo, se lanza el raycast
            if (enemyData.targetTransform != null)
            {
                Vector3 origin = manager.transform.position;
                Vector3 direction = (enemyData.targetTransform.position - origin).normalized;

                // Se lanza el raycast dentro del rango de ataque
                if (Physics.Raycast(origin, direction, out RaycastHit hit, enemyData.attackRange))
                {
                    // Solo si el raycast impacta al objetivo se considera un ataque exitoso
                    if (hit.transform == enemyData.targetTransform)
                    {
                        Debug.Log("Casteo completado: objetivo impactado correctamente con el raycast");
                        enemyData.tower.TakeDamage(enemyData.attackDamage);
                    }
                    else
                    {
                        Debug.Log("Casteo completado: raycast impactó un obstáculo, no se aplica daño");
                    }
                }
            }
            // Se activa el cooldown y se regresa al estado Chase
            enemyData.currentCooldown = enemyData.attackCooldown;
            enemyData.isCasting = false;
            return CasterEnemyState.Chase;
        }
        return CasterEnemyState.Casting;
    }

    public override void ExitState()
    {
        Debug.Log("CasterEnemy saliendo de estado: CASTING");
    }

    public override void OnTriggerEnter(Collider other) { }
    public override void OnTriggerStay(Collider other) { }
    public override void OnTriggerExit(Collider other) { }
}
