using UnityEngine;

public interface IEnemy
{
    // Permite cambiar el estado del enemigo; se espera que el manager interprete el parámetro correctamente.
    void ChangeState(object newState);
    
    // Permite que el enemigo reciba daño.
    void TakeDamage(int damage, Vector3 hitPosition, string damageSource);
    
    // Se puede agregar propiedades o métodos comunes, por ejemplo, la posición del enemigo.
    Transform EnemyTransform { get; }
}