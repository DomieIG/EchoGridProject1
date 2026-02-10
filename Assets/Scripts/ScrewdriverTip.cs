using UnityEngine;

[DisallowMultipleComponent]
public sealed class ScrewdriverTip : MonoBehaviour
{
    [Tooltip("Optional: only screws on these layers will react.")]
    [SerializeField] private LayerMask screwMask = ~0;

    private void OnTriggerEnter(Collider other)
    {
        // Layer filter
        if (((1 << other.gameObject.layer) & screwMask) == 0)
            return;

        // Try directly on the collider
        VentScrew screw = other.GetComponent<VentScrew>();

        // If not found, try parent
        if (!screw)
            screw = other.GetComponentInParent<VentScrew>();

        if (!screw)
            return;

        screw.TryPopOut();
    }
}
