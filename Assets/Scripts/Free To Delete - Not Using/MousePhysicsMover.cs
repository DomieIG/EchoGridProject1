using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class MousePhysicsMover : MonoBehaviour
{
    [SerializeField] private Camera cam;
    [SerializeField] private float moveSpeed = 20f;
    [SerializeField] private float distanceFromCamera = 2.0f;

    private Rigidbody rb;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
        rb.isKinematic = false;     // IMPORTANT for physics triggers
        rb.useGravity = false;
        rb.interpolation = RigidbodyInterpolation.Interpolate;

        if (!cam) cam = Camera.main;
    }

    private void FixedUpdate()
    {
        if (!cam) return;

        Ray ray = cam.ScreenPointToRay(Input.mousePosition);
        Vector3 target = ray.GetPoint(distanceFromCamera);

        Vector3 newPos = Vector3.Lerp(rb.position, target, moveSpeed * Time.fixedDeltaTime);
        rb.MovePosition(newPos);
    }
}
