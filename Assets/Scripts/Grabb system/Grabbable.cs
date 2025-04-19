using UnityEngine;

public class Grabbable : MonoBehaviour
{
    [SerializeField] float grabDistance = 3f;
    [SerializeField] float lerpSpeed = 30f;

    public bool IsGrabbed { get; set; }
    public Rigidbody Rb { get; private set; }

    [Header("Lanzamiento")]
    [SerializeField] float throwForce = 10f;


    void Awake()
    {
        Rb = GetComponent<Rigidbody>();
    }

    public void Grab(Transform holdPoint)
    {
        IsGrabbed = true;

        Rb.useGravity = false;
        Rb.freezeRotation = true;
        transform.localScale *= 0.9f;
    }

    public void Release()
    {
        IsGrabbed = false;
        Rb.useGravity = true;
        Rb.freezeRotation = false;
        transform.localScale /= 0.9f;
    }

    public void MoveTowards(Transform holdPoint)
    {
        Vector3 targetPos = holdPoint.position;
        Rb.linearVelocity = (targetPos - Rb.position) * lerpSpeed;
    }
    public void Throw(Vector3 direction)
    {
        Release(); // Asegurarse de reactivar la física
        Rb.AddForce(direction * throwForce, ForceMode.Impulse);
    }
    void Start()
    {

    }

    public void SetHover(bool hoverState)
    {

    }
}