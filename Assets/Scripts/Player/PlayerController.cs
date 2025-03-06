using UnityEngine;

public class PlayerController : MonoBehaviour
{
    [Header("Movement")]
    [SerializeField] float walkSpeed = 5f;
    [SerializeField] float mouseSensitivity = 2f;

    [Header("Grab System")]
    [SerializeField] Transform holdPoint;
    [SerializeField] float maxGrabDistance = 5f;
    [SerializeField] LayerMask grabbableLayer;

    [Header("Grab Movement")]
    [SerializeField] float scrollSensitivity = 2f;
    [SerializeField] float verticalMoveSpeed = 3f;
    [SerializeField] Vector2 holdDistanceRange = new Vector2(1f, 5f);

    [Header("Lanzamiento")]
    [SerializeField] float throwForceMultiplier = 1.5f;

    private Vector3 holdPosition;
    private float currentHoldDistance;
    private Vector3 originalHoldPosition;

    private Camera playerCamera;
    private CharacterController controller;
    private Grabbable currentGrabbed;
    private float verticalRotation;

    void Start()
    {
        controller = GetComponent<CharacterController>();
        playerCamera = GetComponentInChildren<Camera>();
        Cursor.lockState = CursorLockMode.Locked;
        originalHoldPosition = holdPoint.localPosition;

        currentHoldDistance = Vector3.Distance(holdPoint.position, playerCamera.transform.position);
        holdPosition = holdPoint.localPosition;
    }
    void ReleaseObject()
    {
        if (currentGrabbed != null)
        {
            currentGrabbed.Release();
            currentGrabbed = null;
        }
        holdPoint.localPosition = originalHoldPosition; 
        currentHoldDistance = originalHoldPosition.z; 
    }
    void Update()
    {
        HandleMovement();
        HandleMouseLook();
        HandleGrabInput();
    }

    void HandleMovement()
    {
        float horizontal = Input.GetAxis("Horizontal");
        float vertical = Input.GetAxis("Vertical");

        Vector3 move = transform.TransformDirection(
            new Vector3(horizontal, 0, vertical)) * walkSpeed;
        controller.SimpleMove(move);
    }

    void HandleMouseLook()
    {
        float mouseX = Input.GetAxis("Mouse X") * mouseSensitivity;
        float mouseY = Input.GetAxis("Mouse Y") * mouseSensitivity;

        verticalRotation -= mouseY;
        verticalRotation = Mathf.Clamp(verticalRotation, -90f, 90f);

        playerCamera.transform.localRotation = Quaternion.Euler(verticalRotation, 0, 0);
        transform.Rotate(Vector3.up * mouseX);
    }

    void HandleGrabInput()
    {
        // Click para agarrar
        if (Input.GetMouseButtonDown(0))
        {
            TryGrabObject();
        }

        // Movimiento mientras se agarra
        if (currentGrabbed != null)
        {
            // Scroll para acercar/alejar
            float scroll = Input.GetAxis("Mouse ScrollWheel");
            currentHoldDistance = Mathf.Clamp(
                currentHoldDistance - scroll * scrollSensitivity,
                holdDistanceRange.x,
                holdDistanceRange.y
            );

            // Teclas para subir/bajar (Q/E)
            float verticalInput = Input.GetKey(KeyCode.Q) ? -1 : Input.GetKey(KeyCode.E) ? 1 : 0;
            holdPosition.y += verticalInput * verticalMoveSpeed * Time.deltaTime;

            // Actualiza posición del holdPoint
            holdPoint.localPosition = new Vector3(
                0,
                holdPosition.y,
                currentHoldDistance
            );

            currentGrabbed.MoveTowards(holdPoint);
        }

        // Soltar objeto
        if (Input.GetMouseButtonUp(0) && currentGrabbed != null)
        {
            currentGrabbed.SetHover(false);
            currentGrabbed.Release();
            currentGrabbed = null;
        }

        if (Input.GetMouseButtonDown(1))
        { // Botón derecho
            if (currentGrabbed != null)
            {
                Vector3 throwDirection = playerCamera.transform.forward;
                currentGrabbed.Throw(throwDirection * throwForceMultiplier);
                ReleaseObject();
            }
        }
    }

    void TryGrabObject()
    {
        Ray ray = playerCamera.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0));
        RaycastHit hit;

        if (Physics.Raycast(ray, out hit, maxGrabDistance, grabbableLayer))
        {
            Grabbable grabbable = hit.collider.GetComponent<Grabbable>();
            if (grabbable != null)
            {
                currentGrabbed = grabbable;
                grabbable.Grab(holdPoint);
            }
        }
    }
}