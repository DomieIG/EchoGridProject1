using UnityEngine;
using UnityEngine.InputSystem;

[DisallowMultipleComponent]
public sealed class DesktopClickInteractor : MonoBehaviour
{
    [Header("Raycast")]
    [Min(0.01f)]
    [SerializeField] private float maxDistance = 250f;

    [SerializeField] private LayerMask hitMask = ~0;

    [SerializeField] private QueryTriggerInteraction triggerInteraction = QueryTriggerInteraction.Ignore;

    [Header("Camera")]
    [Tooltip("Optional. If null, uses Camera.main.")]
    [SerializeField] private Camera raycastCamera;

    [Header("Debug")]
    [SerializeField] private bool logHits = false;

    private void Update()
    {
        if (!TryGetPrimaryPressThisFrame(out Vector2 screenPos))
            return;

        Camera cam = raycastCamera != null ? raycastCamera : Camera.main;
        if (cam == null)
        {
            Debug.LogError($"[{nameof(DesktopClickInteractor)}] No camera available (assign one or tag MainCamera).", this);
            return;
        }

        Ray ray = cam.ScreenPointToRay(screenPos);

        if (!Physics.Raycast(ray, out RaycastHit hit, maxDistance, hitMask, triggerInteraction))
            return;

        // Find a DebugClickButton on the hit object OR its parents.
        var button = hit.collider.GetComponentInParent<DebugClickButton>();
        if (button == null)
        {
            if (logHits) Debug.Log($"[{nameof(DesktopClickInteractor)}] Hit '{hit.collider.name}' but no DebugClickButton found.", hit.collider);
            return;
        }

        if (logHits) Debug.Log($"[{nameof(DesktopClickInteractor)}] Invoking '{button.name}' ({button.Action})", button);
        button.Invoke();
    }

    private static bool TryGetPrimaryPressThisFrame(out Vector2 screenPos)
    {
        // Pointer covers mouse/touch/pen in many setups.
        var pointer = Pointer.current;
        if (pointer?.press != null && pointer.press.wasPressedThisFrame)
        {
            screenPos = pointer.position.ReadValue();
            return true;
        }

        // Mouse fallback.
        var mouse = Mouse.current;
        if (mouse != null && mouse.leftButton.wasPressedThisFrame)
        {
            screenPos = mouse.position.ReadValue();
            return true;
        }

        screenPos = default;
        return false;
    }
}
