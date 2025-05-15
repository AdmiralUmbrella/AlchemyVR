using UnityEngine;
using UnityEngine.InputSystem;

public class PortalSpawner : MonoBehaviour
{
    [Header("Player")]
    [SerializeField] private Transform playerHead;   // XR Origin/Camera (arrástrala)

    [Header("Input")]
    [SerializeField] private InputActionReference primaryButton;

    [Header("Scene Portal")]
    [SerializeField] private PortalController portal;   // arrástralo desde la jerarquía
    [SerializeField] private Transform rayOrigin;       // mano derecha

    [Header("Raycast")]
    [SerializeField] private LayerMask groundMask = 1 << 3;
    [SerializeField] private float maxDistance = 20f;

    private void OnEnable()
    {
        primaryButton.action.Enable();
        primaryButton.action.performed += OnPressed;
    }
    private void OnDisable()
    {
        primaryButton.action.performed -= OnPressed;
        primaryButton.action.Disable();
    }

    private void OnPressed(InputAction.CallbackContext ctx)
    {
        // 1. Raycast
        if (!Physics.Raycast(rayOrigin.position, rayOrigin.forward,
                out var hit, maxDistance, groundMask)) return;

        Vector3 pos = hit.point + hit.normal * 0.01f;          // ligera separación

        // 2. Dirección hacia el jugador
        Vector3 toPlayer = playerHead.position - pos;

        // 3. Proyecta sobre el plano del suelo para que no apunte “hacia arriba”
        Vector3 projected = Vector3.ProjectOnPlane(toPlayer, hit.normal).normalized;

        // Evita LookRotation con vector cero si estás demasiado cerca
        if (projected.sqrMagnitude < 0.001f) projected = -rayOrigin.forward;

        // 4. Rotación final: up = normal del suelo, forward = hacia jugador
        Quaternion rot = Quaternion.LookRotation(projected, hit.normal);

        // 5. Activa portal
        portal.ActivateAt(pos, rot);
    }

}