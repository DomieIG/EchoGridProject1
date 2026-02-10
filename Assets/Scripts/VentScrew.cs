using System;
using UnityEngine;

[DisallowMultipleComponent]
public sealed class VentScrew : MonoBehaviour
{
    public event Action<VentScrew> OnPopped;

    [Header("Pop Settings")]
    [SerializeField] private Rigidbody rb;
    [SerializeField] private Collider col;

    [Tooltip("How hard the screw gets pushed out.")]
    [SerializeField] private float popForce = 1.5f;

    [Tooltip("Direction in local space the screw pops toward.")]
    [SerializeField] private Vector3 localPopDirection = Vector3.back;

    [Tooltip("Optional: require continuous contact time before popping.")]
    [SerializeField] private float requiredContactTime = 0f;

    [Tooltip("Cooldown so re-touching doesn't re-trigger anything.")]
    [SerializeField] private float popCooldown = 0.25f;

    private bool popped;
    private float contactTimer;
    private float nextAllowedTime;

    private void Reset()
    {
        rb = GetComponent<Rigidbody>();
        col = GetComponent<Collider>();
    }

    public bool IsPopped => popped;

    // Simple one-shot pop (used by tip)
    public void TryPopOut()
    {
        if (popped) return;
        if (Time.time < nextAllowedTime) return;

        if (requiredContactTime <= 0f)
        {
            PopNow();
            return;
        }

        // If you want timed contact, the tip should call Begin/End contact,
        // but to keep it simple we can treat each touch as incremental time.
        contactTimer += Time.deltaTime;
        if (contactTimer >= requiredContactTime)
            PopNow();
    }

    private void PopNow()
    {
        popped = true;
        nextAllowedTime = Time.time + popCooldown;

        if (rb)
        {
            rb.isKinematic = false;
            rb.useGravity = true;

            Vector3 worldDir = transform.TransformDirection(localPopDirection.normalized);
            rb.AddForce(worldDir * popForce, ForceMode.Impulse);
            rb.AddTorque(UnityEngine.Random.onUnitSphere * (popForce * 0.35f), ForceMode.Impulse);
        }

        OnPopped?.Invoke(this);
    }
}
