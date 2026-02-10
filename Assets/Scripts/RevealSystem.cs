using System.Collections;
using UnityEngine;

[DisallowMultipleComponent]
public sealed class RevealSystem : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private SecurityCameraRig cameraRig;

    [Header("Reveal Settings")]
    [Min(0f)]
    [SerializeField] private float revealDuration = 3f;

    [Tooltip("The layer to reveal temporarily (must be included in the rig's Always Hidden Layers).")]
    [SerializeField] private string revealLayerName = "RevealLayer";

    [Header("Debug")]
    [SerializeField] private bool log = false;

    private Coroutine _routine;
    private Camera _revealedCam;
    private int _baseMask;

    public bool IsRevealing => _routine != null;

    public void Reveal()
    {
        if (cameraRig == null || cameraRig.CurrentCamera == null)
        {
            if (log) Debug.LogWarning($"[{nameof(RevealSystem)}] Reveal requested but cameraRig/current camera missing.", this);
            return;
        }

        int revealLayer = LayerMask.NameToLayer(revealLayerName);
        if (revealLayer < 0)
        {
            Debug.LogError($"[{nameof(RevealSystem)}] Layer '{revealLayerName}' not found. Create it in Project Settings > Tags and Layers.", this);
            return;
        }

        // Stop any prior reveal and restore cleanly.
        StopRevealIfRunning();

        _revealedCam = cameraRig.CurrentCamera;

        // Get the rig-enforced base mask (guaranteed to hide reveal layer by default).
        _baseMask = cameraRig.GetBaseCullingMask(_revealedCam);

        // Reveal by adding the layer bit temporarily.
        int revealBit = 1 << revealLayer;
        _revealedCam.cullingMask = _baseMask | revealBit;

        if (log)
            Debug.Log($"[{nameof(RevealSystem)}] Revealing '{revealLayerName}' for {revealDuration:0.00}s on '{_revealedCam.name}'.", this);

        _routine = StartCoroutine(RevealRoutine());
    }

    private IEnumerator RevealRoutine()
    {
        if (revealDuration > 0f)
            yield return new WaitForSeconds(revealDuration);

        Restore();
    }

    private void StopRevealIfRunning()
    {
        if (_routine != null)
            StopCoroutine(_routine);

        Restore();
    }

    private void Restore()
    {
        if (_revealedCam != null)
            _revealedCam.cullingMask = _baseMask;

        _routine = null;
        _revealedCam = null;
        _baseMask = 0;
    }

    private void OnDisable()
    {
        StopRevealIfRunning();
    }
}
