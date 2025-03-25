using UnityEngine;

/// <summary>
/// Clase abstracta base para todos los estados de la FSM del enemigo.
/// Contiene referencias al EnemyStateManager y a la informaci�n (EnemyData).
/// </summary>
public abstract class EnemyBaseState
{
    // Referencia al StateManager para poder cambiar estados o acceder a m�todos globales.
    protected EnemyStateManager manager;

    // Contiene toda la data configurable y runtime del enemigo,
    // como vida, resistencias, referencias al animator, etc.
    protected EnemyData enemyData;

    /// <summary>
    /// Constructor base. Se llama cuando creamos una instancia de un estado concreto.
    /// </summary>
    /// <param name="manager">El StateManager que maneja la FSM.</param>
    /// <param name="enemyData">Datos comunes del enemigo (vida, da�o, etc.).</param>
    public EnemyBaseState(EnemyStateManager manager, EnemyData enemyData)
    {
        this.manager = manager;
        this.enemyData = enemyData;
    }

    /// <summary>
    /// M�todo que se llama al entrar en este estado.
    /// Aqu� puedes inicializar animaciones o variables espec�ficas del estado.
    /// </summary>
    public abstract void Enter();

    /// <summary>
    /// M�todo que se llama al salir de este estado.
    /// Aqu� se limpian o resetean valores que no deban persistir.
    /// </summary>
    public abstract void Exit();

    /// <summary>
    /// M�todo que se llama cada frame (o en cada Update) mientras el enemigo est� en este estado.
    /// </summary>
    public abstract void Update();
}
