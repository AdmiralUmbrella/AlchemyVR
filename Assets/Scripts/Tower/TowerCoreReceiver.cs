using UnityEngine;

public class TowerCoreReceiver : MonoBehaviour
{
    [Tooltip("Referencia al TowerAI de la torre a la que pertenece este núcleo.")]
    public TowerAI towerAI;
    
    private void OnTriggerEnter(Collider other)
    {
        // Se asume que el objeto de poción tiene el tag "Potion"
        if (other.CompareTag("Potion"))
        {
            // Se busca en el objeto el script Flask para extraer el EssenceSO
            Flask flask = other.GetComponent<Flask>();
            if (flask != null && flask.currentEssence != null)
            {
                // Se inserta la esencia en la torre a través del TowerAI
                towerAI.InsertEssence(flask.currentEssence);
                // Se destruye el objeto de la poción
                Destroy(other.gameObject);
            }
            else
            {
                Debug.Log("La poción no posee una esencia válida.");
            }
        }
    }
}