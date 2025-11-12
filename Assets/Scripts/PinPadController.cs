using System.Collections.Generic;
using UnityEngine;

public class PinPadController : MonoBehaviour
{
    [Header("Door Settings")]
    public Transform doorPivot;                    // Assign DoorPivot (not the cube)
    public float openAngle = 90f;                  // How far it swings open
    public float openSpeed = 2f;

    [Header("Pin Code Settings")]
    public List<int> correctCode = new List<int> { 1, 2, 3, 4 };
    private List<int> enteredCode = new List<int>();

    private bool doorUnlocked = false;
    private Quaternion closedRotation;
    private Quaternion openRotation;

    private void Start()
    {
        if (doorPivot != null)
        {
            closedRotation = doorPivot.rotation;
            openRotation = doorPivot.rotation * Quaternion.Euler(0, openAngle, 0);
        }
    }

    private void Update()
    {
        if (doorUnlocked && doorPivot != null)
        {
            doorPivot.rotation = Quaternion.Lerp(
                doorPivot.rotation,
                openRotation,
                Time.deltaTime * openSpeed
            );
        }
    }

    public void PressButton(int buttonNumber)
    {
        enteredCode.Add(buttonNumber);

        if (enteredCode.Count > correctCode.Count)
        {
            enteredCode.Clear();
            return;
        }

        for (int i = 0; i < enteredCode.Count; i++)
        {
            if (enteredCode[i] != correctCode[i])
            {
                Debug.Log("Wrong code, resetting!");
                enteredCode.Clear();
                return;
            }
        }

        if (enteredCode.Count == correctCode.Count)
        {
            Debug.Log("Correct Code! Door Unlocked.");
            UnlockDoor();
        }
    }

    private void UnlockDoor()
    {
        doorUnlocked = true;
    }
}
