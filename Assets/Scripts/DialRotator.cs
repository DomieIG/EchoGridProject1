using UnityEngine;
using UnityEngine.InputSystem; // New Input System
using TMPro;

public class DialRotator : MonoBehaviour
{
    [Header("Settings")]
    public float rotationStep = 36f;     // Degrees per click
    public int digitsPerDial = 10;       // 0–9

    [Header("Display")]
    public TextMeshPro displayText;      // Number shown above dial

    private int currentValue = 0;

    void Start()
    {
        if (displayText != null)
            displayText.text = currentValue.ToString();
    }

    void Update()
    {
        if (Mouse.current.leftButton.wasPressedThisFrame)
        {
            Ray ray = Camera.main.ScreenPointToRay(Mouse.current.position.ReadValue());
            if (Physics.Raycast(ray, out RaycastHit hit))
            {
                if (hit.transform == transform)
                {
                    RotateDial();
                }
            }
        }
    }

    private void RotateDial()
    {
        transform.Rotate(0f, rotationStep, 0f);
        currentValue = (currentValue + 1) % digitsPerDial;

        if (displayText != null)
            displayText.text = currentValue.ToString();

        Debug.Log($"{gameObject.name} = {currentValue}");
    }

    public int GetValue()
    {
        return currentValue;
    }
}
