using UnityEngine;

public class BeltFollower : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Transform head; // Main Camera (HMD)

    [Header("Offsets")]
    [Tooltip("Forward distance in metres in front of the player")]
    [SerializeField] private float forwardOffset = 0.18f;
    [Tooltip("How far BELOW the head the belt should sit")]
    [SerializeField] private float verticalOffset = 0.55f;

    [Header("Smoothing")]
    [Tooltip("Higher = snappier, Lower = smoother")]
    [SerializeField] private float positionSmoothSpeed = 10f;
    [SerializeField] private float rotationSmoothSpeed = 10f;

    private void LateUpdate()
    {
        if (head == null) return;

        // 1. Flatten the head-forward vector onto the X-Z plane
        Vector3 flatForward = Vector3.ProjectOnPlane(head.forward, Vector3.up).normalized;

        // 2. Compute desired position (in front + down from head)
        Vector3 targetPos = head.position + flatForward * forwardOffset - Vector3.up * verticalOffset;

        // 3. Smooth position & rotation
        transform.position = Vector3.Lerp(transform.position, targetPos,
            Time.deltaTime * positionSmoothSpeed);

        Quaternion targetRot = Quaternion.LookRotation(flatForward, Vector3.up);
        transform.rotation = Quaternion.Slerp(transform.rotation, targetRot,
            Time.deltaTime * rotationSmoothSpeed);
    }
}