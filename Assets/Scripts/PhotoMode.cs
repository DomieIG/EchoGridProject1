using UnityEngine;
using UnityEngine.InputSystem;

public class PhotoMode : MonoBehaviour
{
    [Header("Screenshot Settings")]
    [Range(1, 8)]
    public int superSize = 2;

    private InputAction captureAction;

    void OnEnable()
    {
        captureAction = new InputAction(
            "CaptureScreenshot",
            binding: "<Keyboard>/p"
        );

        captureAction.performed += OnCapture;
        captureAction.Enable();
    }

    void OnDisable()
    {
        captureAction.Disable();
    }

    private void OnCapture(InputAction.CallbackContext context)
    {
        string fileName = $"Screenshot_{System.DateTime.Now:yyyy-MM-dd_HH-mm-ss}.png";
        ScreenCapture.CaptureScreenshot(fileName, superSize);
        Debug.Log($"📸 Screenshot saved: {fileName}");
    }
}
