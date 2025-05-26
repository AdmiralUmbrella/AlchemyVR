using UnityEngine;

public class CanvasBillboard : MonoBehaviour
{
    [SerializeField] private Camera targetCamera;
    [SerializeField] private bool   yAxisOnly  = true;

    private void Awake()
    {
        if (targetCamera == null)
            targetCamera = Camera.main;
    }

    private void LateUpdate()
    {
        if (targetCamera == null) return;

        // Vector que va de la cámara → al canvas
        Vector3 forward = transform.position - targetCamera.transform.position;

        if (yAxisOnly)
            forward.y = 0f;                 // opcional: fija el tilt

        if (forward.sqrMagnitude > 0.0001f)
            transform.rotation = Quaternion.LookRotation(forward); // ya apunta correctamente
    }
}