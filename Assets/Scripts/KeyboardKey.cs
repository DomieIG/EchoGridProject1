using UnityEngine;
using UnityEngine.EventSystems;

public class KeyboardKey : MonoBehaviour, IPointerClickHandler
{
    [Tooltip("The character or function this key represents")]
    public string keyValue;

    [Tooltip("Reference to the computer that receives the key presses")]
    public TerminalComputer computer;

    public void OnPointerClick(PointerEventData eventData)
    {
        if (computer == null)
        {
            Debug.LogWarning($"KeyboardKey '{gameObject.name}' has no TerminalComputer reference!");
            return;
        }

        Debug.Log("Clicked key: " + keyValue);
        computer.ReceiveKeyPress(keyValue);
    }
}
