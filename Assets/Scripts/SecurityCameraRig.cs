using UnityEngine;

[DisallowMultipleComponent]
public sealed class SecurityCameraRig : MonoBehaviour
{
    [Header("Security Cameras (enable one at a time)")]
    [SerializeField] private Camera[] cameras;

    [Header("Monitor Output (optional)")]
    [SerializeField] private Renderer monitorRenderer;
    [Tooltip("Texture property name on the monitor material. Common: _MainTex, _BaseMap")]
    [SerializeField] private string monitorTextureProperty = "_MainTex";

    [Header("Culling Mask Rules")]
    [Tooltip("Layers that should ALWAYS be hidden from security cameras unless temporarily revealed.")]
    [SerializeField] private LayerMask alwaysHiddenLayers;

    [Header("Behavior")]
    [Tooltip("If true, disables non-active camera GameObjects. If false, only enables/disables Camera components.")]
    [SerializeField] private bool disableCameraGameObjects = true;

    [Tooltip("If true, logs camera switches and warnings.")]
    [SerializeField] private bool log = false;

    public int CurrentIndex { get; private set; }
    public Camera CurrentCamera => HasCameras() && cameras[CurrentIndex] != null ? cameras[CurrentIndex] : null;

    private MaterialPropertyBlock _mpb;

    private void Awake()
    {
        _mpb = new MaterialPropertyBlock();
        ApplyBaseCullingMasksToAllCameras();
    }

    private void Start()
    {
        ActivateCamera(Mathf.Clamp(CurrentIndex, 0, (cameras?.Length ?? 1) - 1));
    }

    /// <summary>
    /// The "base" mask that all security cameras should have when NOT revealing.
    /// </summary>
    public int GetBaseCullingMask(Camera cam)
    {
        if (cam == null) return 0;
        // Remove alwaysHiddenLayers from whatever the camera currently has.
        return cam.cullingMask & ~alwaysHiddenLayers.value;
    }

    /// <summary>
    /// Enforces base masks across all cameras (keeps hidden layers hidden by default).
    /// </summary>
    public void ApplyBaseCullingMasksToAllCameras()
    {
        if (!HasCameras()) return;

        for (int i = 0; i < cameras.Length; i++)
        {
            var cam = cameras[i];
            if (cam == null) continue;
            cam.cullingMask = GetBaseCullingMask(cam);
        }

        if (log)
            Debug.Log($"[{nameof(SecurityCameraRig)}] Applied base culling masks. alwaysHiddenLayers={alwaysHiddenLayers.value}", this);
    }

    public void NextCamera()
    {
        if (!HasCameras()) return;
        int next = (CurrentIndex + 1) % cameras.Length;
        ActivateCamera(next);
    }

    public void PrevCamera()
    {
        if (!HasCameras()) return;
        int prev = CurrentIndex - 1;
        if (prev < 0) prev = cameras.Length - 1;
        ActivateCamera(prev);
    }

    public void ActivateCamera(int index)
    {
        if (!HasCameras()) return;

        index = Mathf.Clamp(index, 0, cameras.Length - 1);

        for (int i = 0; i < cameras.Length; i++)
        {
            var cam = cameras[i];
            if (cam == null) continue;

            bool active = (i == index);

            if (disableCameraGameObjects)
                cam.gameObject.SetActive(active);
            else
                cam.enabled = active;
        }

        CurrentIndex = index;

        var current = CurrentCamera;
        if (current != null)
        {
            // Ensure reveal layer is hidden by default every time we switch.
            current.cullingMask = GetBaseCullingMask(current);
        }

        if (log)
            Debug.Log($"[{nameof(SecurityCameraRig)}] Active camera index: {CurrentIndex} ('{current?.name ?? "null"}')", this);

        PushFeedToMonitor(current);
    }

    private void PushFeedToMonitor(Camera cam)
    {
        if (monitorRenderer == null || cam == null) return;

        var rt = cam.targetTexture;
        if (rt == null)
        {
            if (log)
                Debug.LogWarning($"[{nameof(SecurityCameraRig)}] Camera '{cam.name}' has no targetTexture. Monitor feed cannot display.", this);
            return;
        }

        monitorRenderer.GetPropertyBlock(_mpb);

        if (HasProperty(monitorRenderer, monitorTextureProperty))
            _mpb.SetTexture(monitorTextureProperty, rt);
        else if (HasProperty(monitorRenderer, "_BaseMap"))
            _mpb.SetTexture("_BaseMap", rt);
        else if (HasProperty(monitorRenderer, "_MainTex"))
            _mpb.SetTexture("_MainTex", rt);
        else
            Debug.LogWarning($"[{nameof(SecurityCameraRig)}] Monitor material missing texture property '{monitorTextureProperty}' (and no _BaseMap/_MainTex fallback).", monitorRenderer);

        monitorRenderer.SetPropertyBlock(_mpb);
    }

    private static bool HasProperty(Renderer r, string propertyName)
    {
        var mat = r.sharedMaterial;
        return mat != null && mat.HasProperty(propertyName);
    }

    private bool HasCameras() => cameras != null && cameras.Length > 0;

#if UNITY_EDITOR
    private void OnValidate()
    {
        if (string.IsNullOrWhiteSpace(monitorTextureProperty))
            monitorTextureProperty = "_MainTex";
    }
#endif
}
