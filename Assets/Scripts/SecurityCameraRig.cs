using UnityEngine;

public class SecurityCameraRig : MonoBehaviour
{
    [Header("Security Cameras (enable one at a time)")]
    public Camera[] cameras;

    [Header("Monitor Material (Unlit)")]
    public Renderer monitorRenderer;               // mesh renderer of the monitor
    public string monitorTextureProperty = "_MainTex"; // URP Unlit uses _BaseMap sometimes

    public int CurrentIndex { get; private set; } = 0;
    public Camera CurrentCamera => (cameras != null && cameras.Length > 0) ? cameras[CurrentIndex] : null;

    void Start()
    {
        ActivateCamera(0);
    }

    public void NextCamera()
    {
        if (cameras == null || cameras.Length == 0) return;
        int next = (CurrentIndex + 1) % cameras.Length;
        ActivateCamera(next);
    }

    public void PrevCamera()
    {
        if (cameras == null || cameras.Length == 0) return;
        int prev = CurrentIndex - 1;
        if (prev < 0) prev = cameras.Length - 1;
        ActivateCamera(prev);
    }

    public void ActivateCamera(int index)
    {
        if (cameras == null || cameras.Length == 0) return;
        index = Mathf.Clamp(index, 0, cameras.Length - 1);

        // Disable all camera GameObjects (saves perf)
        for (int i = 0; i < cameras.Length; i++)
            if (cameras[i] != null) cameras[i].gameObject.SetActive(i == index);

        CurrentIndex = index;

        // Push that camera feed to the monitor material
        var cam = cameras[CurrentIndex];
        if (cam != null && cam.targetTexture != null && monitorRenderer != null)
        {
            var mat = monitorRenderer.material;
            if (mat.HasProperty(monitorTextureProperty))
                mat.SetTexture(monitorTextureProperty, cam.targetTexture);
            else if (mat.HasProperty("_BaseMap"))
                mat.SetTexture("_BaseMap", cam.targetTexture); // URP fallback
        }
    }
}
