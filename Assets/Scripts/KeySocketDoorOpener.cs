using System.Collections;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactors;

public class KeySocketDoorOpener : MonoBehaviour
{
    [Header("XR Socket")]
    [SerializeField] private XRSocketInteractor socket;

    [Header("Door Pivot (the object that rotates)")]
    [SerializeField] private Transform doorPivot;

    [Header("Door Rotation")]
    [SerializeField] private Vector3 closedLocalEuler = Vector3.zero;
    [SerializeField] private Vector3 openLocalEuler = new Vector3(0f, 90f, 0f);

    [Header("Timing")]
    [SerializeField] private float openDuration = 0.6f;
    [SerializeField] private bool closeWhenKeyRemoved = false;

    [Header("Optional: require a specific key")]
    [SerializeField] private string requiredKeyTag = "Key";

    private Coroutine routine;

    private void Reset()
    {
        socket = GetComponent<XRSocketInteractor>();
    }

    private void OnEnable()
    {
        if (!socket) return;
        socket.selectEntered.AddListener(OnSelectEntered);
        socket.selectExited.AddListener(OnSelectExited);
    }

    private void OnDisable()
    {
        if (!socket) return;
        socket.selectEntered.RemoveListener(OnSelectEntered);
        socket.selectExited.RemoveListener(OnSelectExited);
    }

    private void OnSelectEntered(SelectEnterEventArgs args)
    {
        if (!IsValidKey(args.interactableObject)) return;
        StartRotate(openLocalEuler, openDuration);
    }

    private void OnSelectExited(SelectExitEventArgs args)
    {
        if (!closeWhenKeyRemoved) return;
        if (!IsValidKey(args.interactableObject)) return;
        StartRotate(closedLocalEuler, openDuration);
    }

    private bool IsValidKey(UnityEngine.XR.Interaction.Toolkit.Interactables.IXRSelectInteractable interactable)
    {
        if (string.IsNullOrEmpty(requiredKeyTag)) return true;
        var mb = interactable.transform.GetComponent<MonoBehaviour>();
        return mb != null && mb.CompareTag(requiredKeyTag);
    }

    private void StartRotate(Vector3 targetLocalEuler, float duration)
    {
        if (!doorPivot) return;
        if (routine != null) StopCoroutine(routine);
        routine = StartCoroutine(RotateDoor(targetLocalEuler, duration));
    }

    private IEnumerator RotateDoor(Vector3 targetLocalEuler, float duration)
    {
        Quaternion start = doorPivot.localRotation;
        Quaternion end = Quaternion.Euler(targetLocalEuler);

        float t = 0f;
        while (t < duration)
        {
            t += Time.deltaTime;
            float a = Mathf.Clamp01(t / duration);
            // Smoothstep feel
            a = a * a * (3f - 2f * a);

            doorPivot.localRotation = Quaternion.Slerp(start, end, a);
            yield return null;
        }

        doorPivot.localRotation = end;
        routine = null;
    }
}
