using System.Text;
using UnityEngine;
using UnityEngine.InputSystem;

[DisallowMultipleComponent]
public sealed class ClickRayDebug : MonoBehaviour
{
    [Header("Raycast Settings")]
    [Min(0.01f)]
    [SerializeField] private float maxDistance = 250f;

    [Tooltip("Layers the raycast can hit.")]
    [SerializeField] private LayerMask hitMask = ~0;

    [Tooltip("Whether the raycast should hit trigger colliders.")]
    [SerializeField] private QueryTriggerInteraction triggerInteraction = QueryTriggerInteraction.Ignore;

    [Header("Runtime Controls")]
    [Tooltip("Master toggle for desktop testing.")]
    [SerializeField] private bool enabledOnDesktop = true;

    [Tooltip("If true, only runs in Editor or Development Builds.")]
    [SerializeField] private bool devOnly = true;

    [Header("Camera")]
    [Tooltip("Optional. If null, uses Camera.main (cached).")]
    [SerializeField] private Camera raycastCamera;

    [Header("Debug Output")]
    [Tooltip("Logs hits and misses.")]
    [SerializeField] private bool log = true;

    [Tooltip("Draws the ray in the Scene view for a short time.")]
    [SerializeField] private bool drawDebugRay = false;

    [Min(0f)]
    [SerializeField] private float debugRayDuration = 0.2f;

    private Camera _cachedMain;

    private void Awake()
    {
        CacheMainCameraIfNeeded();
    }

    private void OnEnable()
    {
        CacheMainCameraIfNeeded();
    }

    private void Update()
    {
        if (!enabledOnDesktop) return;
        if (devOnly && !(Application.isEditor || Debug.isDebugBuild)) return;
        if (!TryGetPrimaryPressThisFrame(out Vector2 screenPos)) return;

        Camera cam = GetCamera();
        if (cam == null)
        {
            Debug.LogError($"[{nameof(ClickRayDebug)}] No camera available. Assign '{nameof(raycastCamera)}' or tag an enabled camera as MainCamera.", this);
            return;
        }

        Ray ray = cam.ScreenPointToRay(screenPos);

        if (drawDebugRay)
            Debug.DrawRay(ray.origin, ray.direction * maxDistance, Color.yellow, debugRayDuration);

        if (Physics.Raycast(ray, out RaycastHit hit, maxDistance, hitMask, triggerInteraction))
        {
            if (!log) return;

            GameObject go = hit.collider.gameObject;
            Debug.Log(BuildHitMessage(hit, go), go);
        }
        else
        {
            if (!log) return;

            Debug.Log(
                $"[{nameof(ClickRayDebug)}] Raycast hit nothing. pos={screenPos}, dist={maxDistance:0.##}, mask={hitMask.value}, triggers={triggerInteraction}",
                this
            );
        }
    }

    private Camera GetCamera()
    {
        if (raycastCamera != null && raycastCamera.isActiveAndEnabled)
            return raycastCamera;

        if (_cachedMain != null && _cachedMain.isActiveAndEnabled)
            return _cachedMain;

        // If the main camera changed after Awake, refresh once.
        _cachedMain = Camera.main;
        if (_cachedMain != null && _cachedMain.isActiveAndEnabled)
            return _cachedMain;

        return null;
    }

    private void CacheMainCameraIfNeeded()
    {
        if (raycastCamera == null)
            _cachedMain = Camera.main;
    }

    /// <summary>
    /// New Input System: returns true when a primary pointer press occurred this frame and outputs screen position.
    /// Works for mouse, touch, pen via Pointer where available, with mouse fallback.
    /// </summary>
    private static bool TryGetPrimaryPressThisFrame(out Vector2 screenPos)
    {
        // Prefer generic pointer (covers touch/pen in many setups).
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

    private static string BuildHitMessage(RaycastHit hit, GameObject go)
    {
        var sb = new StringBuilder(256);
        sb.Append('[').Append(nameof(ClickRayDebug)).Append("] Hit: ")
          .Append(GetHierarchyPath(go.transform))
          .Append(" | Layer: ").Append(LayerMask.LayerToName(go.layer))
          .Append(" | Collider: ").Append(hit.collider.GetType().Name)
          .Append(" | Point: ").Append(hit.point.ToString("F3"))
          .Append(" | Normal: ").Append(hit.normal.ToString("F3"))
          .Append(" | Dist: ").Append(hit.distance.ToString("F2"));
        return sb.ToString();
    }

    private static string GetHierarchyPath(Transform t)
    {
        // Produces "Root/Child/SubChild"
        var sb = new StringBuilder(t.name);
        while (t.parent != null)
        {
            t = t.parent;
            sb.Insert(0, '/').Insert(0, t.name);
        }
        return sb.ToString();
    }

#if UNITY_EDITOR
    private void OnValidate()
    {
        if (maxDistance < 0.01f) maxDistance = 0.01f;
        if (debugRayDuration < 0f) debugRayDuration = 0f;
    }
#endif
}
