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

    private Camera playerCamera;
    private CharacterController controller;
    private Grabbable currentGrabbed;
    private float verticalRotation;

    void Start()
    {
        controller = GetComponent<CharacterController>();
        playerCamera = GetComponentInChildren<Camera>();
        Cursor.lockState = CursorLockMode.Locked;
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
        if (Input.GetMouseButtonDown(0))
        {
            TryGrabObject();
        }

        if (Input.GetMouseButtonUp(0) && currentGrabbed != null)
        {
            currentGrabbed.Release();
            currentGrabbed = null;
        }

        if (currentGrabbed != null && currentGrabbed.IsGrabbed)
        {
            currentGrabbed.MoveTowards(holdPoint);
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