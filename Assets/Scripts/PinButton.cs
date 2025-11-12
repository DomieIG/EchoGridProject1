using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Collider))]
public class PinButton : MonoBehaviour
{
    public int buttonNumber;
    public PinPadController pinPad;

    private void Update()
    {
        // Detect click/tap using Input System
        if (Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame)
        {
            CheckClick();
        }

        if (Touchscreen.current != null && Touchscreen.current.primaryTouch.press.wasPressedThisFrame)
        {
            CheckClick();
        }
    }

    private void CheckClick()
    {
        Ray ray = Camera.main.ScreenPointToRay(Mouse.current != null
            ? Mouse.current.position.ReadValue()
            : Touchscreen.current.primaryTouch.position.ReadValue());

        if (Physics.Raycast(ray, out RaycastHit hit))
        {
            if (hit.collider.gameObject == gameObject)
            {
                pinPad.PressButton(buttonNumber);
            }
        }
    }
}
