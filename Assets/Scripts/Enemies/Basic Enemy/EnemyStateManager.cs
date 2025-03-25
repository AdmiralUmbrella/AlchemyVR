using UnityEngine;
using UnityEngine.AI;

public class EnemyStateManager : MonoBehaviour
{
    public delegate void EnemyDeathHandler();
    public event EnemyDeathHandler OnEnemyDeath;

    public EnemyData enemyData;

    private EnemyBaseState currentState;

    public EnemyIdleState IdleState { get; private set; }
    public EnemyChaseState ChaseState { get; private set; }
    public EnemyAttackState AttackState { get; private set; }
    public EnemyHitState HitState { get; private set; }
    public EnemyDeadState DeadState { get; private set; }

    private void Awake()
    {
        SetupComponents();

        IdleState = new EnemyIdleState(this, enemyData);
        ChaseState = new EnemyChaseState(this, enemyData);
        AttackState = new EnemyAttackState(this, enemyData);
        HitState = new EnemyHitState(this, enemyData);
        DeadState = new EnemyDeadState(this, enemyData);

        enemyData.currentHealth = enemyData.maxHealth;
    }

    private void Start()
    {
        // Buscamos solo una vez al Player, como antes.
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            enemyData.playerTransform = player.transform;
        }

        // Iniciar en Idle
        ChangeState(IdleState);
    }

    private void Update()
    {
        if (enemyData.attackCooldownTimer > 0 && !enemyData.isDead)
        {
            enemyData.attackCooldownTimer -= Time.deltaTime;
        }

        currentState?.Update();
    }

    public void ChangeState(EnemyBaseState newState)
    {
        currentState?.Exit();
        currentState = newState;
        currentState?.Enter();
    }

    public void StopAgent()
    {
        if (enemyData.agent != null)
        {
            enemyData.agent.isStopped = true;
            enemyData.agent.velocity = Vector3.zero;
        }
    }

    public void ResumeAgent()
    {
        if (enemyData.agent != null)
        {
            enemyData.agent.isStopped = false;
        }
    }

    /// <summary>
    /// Método que busca primero una torre en el rango especificado.
    /// Si hay al menos una, la devuelve como Transform.
    /// Si no encuentra ninguna, revisa si el Player está en ese rango y devuelve el Transform del Player.
    /// Si tampoco hay Player cerca, retorna null.
    /// </summary>
    public Transform GetPriorityTarget(float range)
    {
        // 1) Buscamos TODAS las torres en la escena con la etiqueta "Tower".
        GameObject[] towers = GameObject.FindGameObjectsWithTag("Tower");

        Transform nearestTower = null;
        float nearestDistance = Mathf.Infinity;
        Vector3 myPosition = transform.position;

        // 2) Recorremos todas las torres para ver si alguna está dentro del rango y es la más cercana.
        foreach (GameObject tower in towers)
        {
            float distanceToTower = Vector3.Distance(myPosition, tower.transform.position);
            if (distanceToTower <= range && distanceToTower < nearestDistance)
            {
                nearestDistance = distanceToTower;
                nearestTower = tower.transform;
            }
        }

        // 3) Si encontramos torre en rango, la retornamos (prioridad total).
        if (nearestTower != null)
        {
            return nearestTower;
        }

        // 4) Si no hay torre, comprobamos si el Player está en rango:
        if (enemyData.playerTransform != null)
        {
            float distanceToPlayer = Vector3.Distance(myPosition, enemyData.playerTransform.position);
            if (distanceToPlayer <= range)
            {
                return enemyData.playerTransform;
            }
        }

        // 5) Si no hay torre ni Player en rango, retornamos null.
        return null;
    }

    /// <summary>
    /// Para verificar si hay algún objetivo prioritario (Tower o, si no, Player) en un rango dado.
    /// Devuelve true/false según corresponda y, si lo hay, asigna ese Transform a enemyData.playerTransform.
    /// </summary>
    public bool CheckForTargetsInRange(float range)
    {
        // 1) Usamos el nuevo método que decide a quién perseguir
        Transform target = GetPriorityTarget(range);

        // 2) Si es distinto de null, es que hay algo en rango
        if (target != null)
        {
            enemyData.playerTransform = target;
            return true;
        }
        return false;
    }

    public bool IsPlayerInRange(float range)
    {
        // (Opcional) Podrías dejar este método como estaba,
        // o hacerlo que internamente llame a CheckForTargetsInRange.
        // Ejemplo:
        return CheckForTargetsInRange(range);
    }

    public void TakeDamage(int damage, Vector3 attackerPosition, string damageType)
    {
        if (enemyData.isDead || enemyData.isStunned) return;

        float finalDamage = damage;

        switch (damageType)
        {
            case "Pyro":
                finalDamage *= (1f - enemyData.pyroResistance);
                break;
            case "Aqua":
                finalDamage *= (1f - enemyData.aquaResistance);
                break;
            case "Geo":
                finalDamage *= (1f - enemyData.geoResistance);
                break;
            case "Aero":
                finalDamage *= (1f - enemyData.aeroResistance);
                break;
        }

        enemyData.currentHealth -= Mathf.RoundToInt(finalDamage);
        Debug.Log($"Enemigo recibió {finalDamage} de daño {damageType}. Vida actual: {enemyData.currentHealth}");

        if (enemyData.currentHealth <= 0)
        {
            Die();
        }
        else if (enemyData.canBeStunned)
        {
            ChangeState(HitState);
        }
    }

    public void Die()
    {
        if (!enemyData.isDead)
        {
            OnEnemyDeath?.Invoke();
            ChangeState(DeadState);
        }
    }

    private void SetupComponents()
    {
        enemyData.agent = GetComponent<NavMeshAgent>();
        if (enemyData.agent != null)
        {
            enemyData.agent.speed = enemyData.moveSpeed;
            enemyData.agent.stoppingDistance = enemyData.attackRange;
        }

        if (enemyData.animator == null && enemyData.modelRoot != null)
        {
            enemyData.animator = enemyData.modelRoot.GetComponent<Animator>();
        }
    }

    private void OnDrawGizmosSelected()
    {
        if (enemyData == null) return;

        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, enemyData.detectionRange);

        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, enemyData.attackRange);

        Gizmos.color = Color.blue;
        Gizmos.DrawWireSphere(transform.position, enemyData.stopChaseDistance);
    }
}
