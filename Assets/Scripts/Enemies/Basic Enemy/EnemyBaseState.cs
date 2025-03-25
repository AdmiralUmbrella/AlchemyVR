using UnityEngine;

/// <summary>
/// Clase abstracta base para todos los estados de la FSM del enemigo.
/// Contiene referencias al EnemyStateManager y a la información (EnemyData).
/// </summary>
public abstract class EnemyBaseState
{
    // Referencia al StateManager para poder cambiar estados o acceder a métodos globales.
    protected EnemyStateManager manager;

    // Contiene toda la data configurable y runtime del enemigo,
    // como vida, resistencias, referencias al animator, etc.
    protected EnemyData enemyData;

    /// <summary>
    /// Constructor base. Se llama cuando creamos una instancia de un estado concreto.
    /// </summary>
    /// <param name="manager">El StateManager que maneja la FSM.</param>
    /// <param name="enemyData">Datos comunes del enemigo (vida, daño, etc.).</param>
    public EnemyBaseState(EnemyStateManager manager, EnemyData enemyData)
    {
        this.manager = manager;
        this.enemyData = enemyData;
    }

    /// <summary>
    /// Método que se llama al entrar en este estado.
    /// Aquí puedes inicializar animaciones o variables específicas del estado.
    /// </summary>
    public abstract void Enter();

    /// <summary>
    /// Método que se llama al salir de este estado.
    /// Aquí se limpian o resetean valores que no deban persistir.
    /// </summary>
    public abstract void Exit();

    /// <summary>
    /// Método que se llama cada frame (o en cada Update) mientras el enemigo está en este estado.
    /// </summary>
    public abstract void Update();
}
