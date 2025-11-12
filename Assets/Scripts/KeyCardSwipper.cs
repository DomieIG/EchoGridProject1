using UnityEngine;
using System.Collections;

public class KeyCardSwipper : MonoBehaviour
{
    [Header("Keycard Settings")]
    [SerializeField] private string correctKeycardTag = "CorrectKeyCard"; // Tag of correct card
    [SerializeField] private string genericKeycardTag = "KeyCard";        // Tag of any keycard

    [Header("Light Objects")]
    [SerializeField] private GameObject greenLightObject;
    [SerializeField] private GameObject redLightObject;

    [Header("Door Settings")]
    [SerializeField] private Transform doorPivot;
    [SerializeField] private float openAngle = 90f;
    [SerializeField] private float openSpeed = 2f;
    [SerializeField] private float doorOpenTime = 3f;

    [Header("Timing")]
    [SerializeField] private float resetDelay = 1f;

    private Quaternion closedRotation;
    private Quaternion openRotation;
    private Coroutine doorRoutine;

    private void Start()
    {
        if (greenLightObject) greenLightObject.SetActive(false);
        if (redLightObject) redLightObject.SetActive(false);

        if (doorPivot != null)
        {
            closedRotation = doorPivot.localRotation;
            openRotation = closedRotation * Quaternion.Euler(0, openAngle, 0);
        }
        else
        {
            Debug.LogWarning("⚠️ No DoorPivot assigned to KeyCardSwipper!");
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        // If it's not a card at all, ignore
        if (!other.CompareTag(genericKeycardTag) && !other.CompareTag(correctKeycardTag))
            return;

        // Correct card
        if (other.CompareTag(correctKeycardTag))
        {
            ShowLight(true);
            Debug.Log("✅ Correct keycard swiped!");

            if (doorRoutine != null) StopCoroutine(doorRoutine);
            doorRoutine = StartCoroutine(OpenDoorRoutine());
        }
        else
        {
            ShowLight(false);
            Debug.Log("❌ Wrong keycard swiped!");
        }

        // Reset lights after delay
        StartCoroutine(ResetLightsAfterDelay());
    }

    private void ShowLight(bool isCorrect)
    {
        if (greenLightObject) greenLightObject.SetActive(isCorrect);
        if (redLightObject) redLightObject.SetActive(!isCorrect);
    }

    private IEnumerator ResetLightsAfterDelay()
    {
        yield return new WaitForSeconds(resetDelay);
        if (greenLightObject) greenLightObject.SetActive(false);
        if (redLightObject) redLightObject.SetActive(false);
    }

    private IEnumerator OpenDoorRoutine()
    {
        if (doorPivot == null) yield break;

        // Rotate open
        yield return RotateDoor(closedRotation, openRotation);

        // Hold door open
        yield return new WaitForSeconds(doorOpenTime);

        // Rotate closed
        yield return RotateDoor(openRotation, closedRotation);

        doorRoutine = null;
    }

    private IEnumerator RotateDoor(Quaternion from, Quaternion to)
    {
        float t = 0f;
        while (t < 1f)
        {
            t += Time.deltaTime * openSpeed;
            doorPivot.localRotation = Quaternion.Slerp(from, to, t);
            yield return null;
        }
    }
}
