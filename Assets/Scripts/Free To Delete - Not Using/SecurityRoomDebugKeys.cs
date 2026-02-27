using UnityEngine;

public class SecurityRoomDebugKeys : MonoBehaviour
{
    public SecurityCameraRig cameraRig;
    public RevealSystem revealSystem;

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.E)) cameraRig?.NextCamera();
        if (Input.GetKeyDown(KeyCode.Q)) cameraRig?.PrevCamera();
        if (Input.GetKeyDown(KeyCode.R)) revealSystem?.Reveal();
    }
}
