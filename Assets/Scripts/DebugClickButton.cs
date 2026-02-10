using UnityEngine;

[DisallowMultipleComponent]
[RequireComponent(typeof(Collider))]
public sealed class DebugClickButton : MonoBehaviour
{
    public enum ActionType { NextCam, PrevCam, Reveal }

    [Header("Action")]
    [SerializeField] private ActionType action = ActionType.NextCam;

    [Header("References")]
    [SerializeField] private SecurityCameraRig cameraRig;
    [SerializeField] private RevealSystem revealSystem;

    [Header("Debug")]
    [SerializeField] private bool log = false;

    public ActionType Action => action;

    private void Reset()
    {
        if (cameraRig == null) cameraRig = FindFirstObjectByType<SecurityCameraRig>();
        if (revealSystem == null) revealSystem = FindFirstObjectByType<RevealSystem>();
    }

    /// <summary>
    /// Safe entry point for desktop + VR.
    /// Call this from ray interactor, XRI events, UI, etc.
    /// </summary>
    public void Invoke()
    {
        switch (action)
        {
            case ActionType.NextCam:
                if (cameraRig == null) { WarnMissing(nameof(SecurityCameraRig)); return; }
                cameraRig.NextCamera();
                if (log) Debug.Log($"[{nameof(DebugClickButton)}] NextCam '{name}'", this);
                break;

            case ActionType.PrevCam:
                if (cameraRig == null) { WarnMissing(nameof(SecurityCameraRig)); return; }
                cameraRig.PrevCamera();
                if (log) Debug.Log($"[{nameof(DebugClickButton)}] PrevCam '{name}'", this);
                break;

            case ActionType.Reveal:
                if (revealSystem == null) { WarnMissing(nameof(RevealSystem)); return; }
                revealSystem.Reveal();
                if (log) Debug.Log($"[{nameof(DebugClickButton)}] Reveal '{name}'", this);
                break;
        }
    }

    private void WarnMissing(string component)
    {
        Debug.LogWarning($"[{nameof(DebugClickButton)}] '{name}' cannot run '{action}' because '{component}' reference is missing.", this);
    }
}
