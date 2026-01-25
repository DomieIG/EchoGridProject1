using UnityEngine;

public class DebugClickButton : MonoBehaviour
{
    public enum ActionType { NextCam, PrevCam, Reveal }

    [Header("What this button should do (desktop test)")]
    public ActionType action;

    [Header("References")]
    public SecurityCameraRig cameraRig;
    public RevealSystem revealSystem;

    [Header("Desktop Testing")]
    public bool desktopTestingEnabled = true;

    void OnMouseDown()
    {
        if (!desktopTestingEnabled) return;

        // Only works if you have a Camera + Physics Raycaster setup OR a collider hit by the scene view.
        switch (action)
        {
            case ActionType.NextCam:
                cameraRig?.NextCamera();
                break;
            case ActionType.PrevCam:
                cameraRig?.PrevCamera();
                break;
            case ActionType.Reveal:
                revealSystem?.Reveal();
                break;
        }
    }
}
