using UnityEngine;

public class CasterEnemyCastingState : BaseState<CasterEnemyState>
{
    private readonly CasterEnemyAI manager;
    private readonly CasterEnemyData enemyData;

    /* ---------- Efecto visual ---------- */
    private const string BEAM_KEY = "CasterBeam";              // ★ clave del pool

    public CasterEnemyCastingState(
        CasterEnemyState key,
        CasterEnemyAI manager,
        CasterEnemyData data
    ) : base(key)
    {
        this.manager   = manager;
        this.enemyData = data;
    }

    public override void EnterState()
    {
        Debug.Log("CASTING");
        enemyData.isCasting        = true;
        enemyData.currentCastingTime = enemyData.castingTime;

        if (enemyData.agent != null) enemyData.agent.isStopped = true;
        enemyData.animator?.SetTrigger("Cast");
    }

    public override void UpdateState()
    {
        enemyData.currentCastingTime -= Time.deltaTime;
        if (enemyData.targetTransform != null)
            Debug.DrawLine(manager.transform.position,
                           enemyData.targetTransform.position,
                           Color.red);
    }

    public override CasterEnemyState GetNextState()
    {
        if (enemyData.currentCastingTime > 0f) return CasterEnemyState.Casting;

        /* ---------- Fin del casteo ---------- */
        if (enemyData.targetTransform != null)
        {
            Vector3 origin = manager.transform.position;
            Vector3 dir    = (enemyData.targetTransform.position - origin).normalized;
            
            Quaternion rot = Quaternion.LookRotation(dir);  

            if (Physics.Raycast(origin, dir, out RaycastHit hit, enemyData.attackRange))
            {
                if (hit.transform == enemyData.targetTransform)
                {
                    // ★ Spawn del efecto vía pooling
                    PoolManager.Instance.Spawn(
                        BEAM_KEY,
                        enemyData.beamPrefab,
                        origin + dir * 1.0f,   // opcional: offset delante del caster
                        rot
                    );

                    enemyData.tower.TakeDamage(enemyData.attackDamage);
                }
            }
        }

        enemyData.currentCooldown = enemyData.attackCooldown;
        enemyData.isCasting       = false;
        return CasterEnemyState.Chase;
    }

    public override void ExitState() { }

    public override void OnTriggerEnter(Collider _) { }
    public override void OnTriggerStay(Collider _)  { }
    public override void OnTriggerExit(Collider _)  { }
}
