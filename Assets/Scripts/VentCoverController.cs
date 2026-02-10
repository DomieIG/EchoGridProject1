using System.Collections.Generic;
using UnityEngine;

[DisallowMultipleComponent]
public sealed class VentCoverController : MonoBehaviour
{
    [Header("Screws to monitor")]
    [SerializeField] private List<VentScrew> screws = new();

    [Header("Cover")]
    [SerializeField] private Transform coverPivot;
    [SerializeField] private Vector3 closedLocalEuler = Vector3.zero;
    [SerializeField] private Vector3 openLocalEuler = new Vector3(-90f, 0f, 0f);
    [SerializeField] private float openDuration = 0.6f;

    private int poppedCount;
    private Coroutine routine;

    private void OnEnable()
    {
        poppedCount = 0;

        foreach (var s in screws)
        {
            if (!s) continue;
            s.OnPopped += HandleScrewPopped;
            if (s.IsPopped) poppedCount++;
        }

        // If they were already popped in editor
        if (poppedCount >= screws.Count && screws.Count > 0)
            StartOpen();
    }

    private void OnDisable()
    {
        foreach (var s in screws)
            if (s) s.OnPopped -= HandleScrewPopped;
    }

    private void HandleScrewPopped(VentScrew _)
    {
        poppedCount++;

        if (poppedCount >= screws.Count)
            StartOpen();
    }

    private void StartOpen()
    {
        if (!coverPivot) return;

        if (routine != null) StopCoroutine(routine);
        routine = StartCoroutine(OpenRoutine());
    }

    private System.Collections.IEnumerator OpenRoutine()
    {
        Quaternion a = Quaternion.Euler(closedLocalEuler);
        Quaternion b = Quaternion.Euler(openLocalEuler);

        float t = 0f;
        while (t < openDuration)
        {
            t += Time.deltaTime;
            coverPivot.localRotation = Quaternion.Slerp(a, b, Mathf.Clamp01(t / openDuration));
            yield return null;
        }

        coverPivot.localRotation = b;
        routine = null;
    }
}
