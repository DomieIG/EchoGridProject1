using UnityEngine;

public class WireCutterTool : MonoBehaviour
{
    [Header("Only cut when tool is held")]
    [SerializeField] private XRGrabState grabState; // small helper (below)
    [SerializeField] private bool requireHeld = true;

    [Header("Cooldown to prevent multi-cuts in one overlap")]
    [SerializeField] private float cutCooldown = 0.25f;
    private float _nextCutTime;

    private void OnTriggerEnter(Collider other)
    {
        if (Time.time < _nextCutTime) return;
        if (requireHeld && (grabState == null || !grabState.IsHeld)) return;

        var wire = other.GetComponentInParent<WireInteractable>();
        if (!wire) return;

        // Trigger the same logic as XR "activate"
        wire.SendMessage("OnToolCut", SendMessageOptions.DontRequireReceiver);

        _nextCutTime = Time.time + cutCooldown;

        Debug.Log(wire ? $"Found wire: {wire.name}" : "No WireInteractable found on hit object/parents");

        // keep your existing logic after this
    }
}
