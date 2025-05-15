using UnityEngine;
using UnityEngine.Events;

/// Colócalo en PortalRoot (padre).
public class PortalController : MonoBehaviour
{
    [Header("Child with Teleporter + visuals")]
    [SerializeField] private GameObject portalPrefab;   // arrastra el hijo

    private Teleporter teleporter;                      // se busca en el hijo

    private void Awake()
    {
        if (portalPrefab == null)
            Debug.LogError("PortalController: arrastra el PortalPrefab hijo.");

        // Encuentra el Teleporter dentro del hijo (inactivo incluido)
        teleporter = portalPrefab.GetComponent<Teleporter>()
                     ?? portalPrefab.GetComponentInChildren<Teleporter>(true);

        if (teleporter == null)
            Debug.LogError("PortalController: el hijo no tiene Teleporter.");

        // Suscribe evento (lo declaras en Teleporter.cs)
        teleporter.onPlayerTeleported.AddListener(DeactivatePortal);

        portalPrefab.SetActive(false);                  // desactivado al arrancar
    }

    public void ActivateAt(Vector3 pos, Quaternion rot)
    {
        transform.SetPositionAndRotation(pos, rot);     // mueves el padre
        portalPrefab.SetActive(true);                   // enciendes solo el hijo
    }

    private void DeactivatePortal()
    {
        // Pequeño delay para salir del Trigger sin glitches
        StartCoroutine(DisableNextFrame());
    }
    private System.Collections.IEnumerator DisableNextFrame()
    {
        yield return null;
        portalPrefab.SetActive(false);
    }
}