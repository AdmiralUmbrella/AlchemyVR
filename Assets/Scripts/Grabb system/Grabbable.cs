using UnityEngine;

public class Grabbable : MonoBehaviour
{
    [SerializeField] float grabDistance = 3f;
    [SerializeField] float lerpSpeed = 30f;

    public bool IsGrabbed { get; set; }
    public Rigidbody Rb { get; private set; }

    void Awake()
    {
        Rb = GetComponent<Rigidbody>();
    }

    public void Grab(Transform holdPoint)
    {
        IsGrabbed = true;
        Rb.useGravity = false;
        Rb.freezeRotation = true;
    }

    public void Release()
    {
        IsGrabbed = false;
        Rb.useGravity = true;
        Rb.freezeRotation = false;
    }

    public void MoveTowards(Transform holdPoint)
    {
        Vector3 targetPos = holdPoint.position;
        Rb.linearVelocity = (targetPos - Rb.position) * lerpSpeed;
    }
}