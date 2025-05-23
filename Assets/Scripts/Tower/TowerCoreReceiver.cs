using System.Collections;
using UnityEngine;
using UnityEngine.UI;   // ← Necesario para trabajar con la UI

public class TowerCoreReceiver : MonoBehaviour
{
    [Tooltip("Referencia al TowerAI de la torre a la que pertenece este núcleo.")]
    public TowerAI towerAI;

    [Header("UI")]
    [Tooltip("Imagen que mostrará el icono y el relleno-temporizador.")]
    [SerializeField] private Image essenceImage;

    // Guarda la corrutina por si necesitamos reiniciarla
    private Coroutine essenceUICoroutine;

    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("RoundPotion")) return;

        // Intentamos obtener la esencia de la poción
        if (other.TryGetComponent(out Flask flask) && flask.currentEssence != null)
        {
            // 1. Insertamos la esencia en la torre
            towerAI.InsertEssence(flask.currentEssence);

            // 2. Actualizamos el UI
            if (essenceUICoroutine != null) StopCoroutine(essenceUICoroutine);
            essenceUICoroutine = StartCoroutine(UpdateEssenceUI(flask.currentEssence));

            // 3. Destruimos la poción
            Destroy(other.gameObject);
        }
        else
        {
            Debug.Log("La poción no posee una esencia válida.");
        }
    }

    /// <summary>
    /// Corrutina que muestra el icono y reduce el fill según la duración activa de la torre.
    /// </summary>
    private IEnumerator UpdateEssenceUI(EssenceSO essence)
    {
        // -- Configuración inicial de la imagen
        essenceImage.enabled      = true;
        var color = essenceImage.color;
        color.a = 1f;
        essenceImage.color = color;
        essenceImage.sprite       = essence.essenceIcon;
        essenceImage.type         = Image.Type.Filled;
        essenceImage.fillMethod   = Image.FillMethod.Vertical; // o la que prefieras
        essenceImage.fillOrigin   = (int)Image.OriginVertical.Bottom;   // opcional
        essenceImage.fillAmount   = 1f;

        /*  Esperamos a que la torre empiece realmente a disparar.
            Por defecto el estado Charging dura 1 s, pero
            si ese tiempo cambia, basta con ajustar este WaitUntil
            (solo funciona si StateManager expone CurrentState).
        */
        yield return new WaitUntil(() => towerAI.CurrentState is TowerActiveState);

        float duration = towerAI.towerData.activeDuration;
        float elapsed  = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            essenceImage.fillAmount = 1f - (elapsed / duration);
            yield return null;
        }

        // Vaciar y ocultar cuando termina
        essenceImage.fillAmount = 0f;
        essenceImage.enabled    = false;
    }
}
