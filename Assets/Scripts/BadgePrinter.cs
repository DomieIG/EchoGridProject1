using UnityEngine;
using System.Collections;
using System;

public class BadgePrinter : MonoBehaviour
{
    [Header("Badge Object")]
    [Tooltip("The badge mesh that will slide out of the printer.")]
    public Transform badgeObject;

    [Tooltip("Local position where the badge starts (inside the printer).")]
    public Vector3 startLocalPos;

    [Tooltip("Local position where the badge ends (fully printed).")]
    public Vector3 endLocalPos;

    [Header("Print Settings")]
    [Tooltip("How long it takes to print the badge (in seconds).")]
    public float printDuration = 2f;

    [Tooltip("Curve controlling the speed of the badge slide.")]
    public AnimationCurve easing = AnimationCurve.EaseInOut(0, 0, 1, 1);

    private Coroutine printRoutine;
    private bool isPrinting = false;

    // Optional events if you want to hook into animations or sounds
    public event Action OnPrintStart;
    public event Action OnPrintComplete;


    /// <summary>
    /// Starts the printing animation if not already printing.
    /// </summary>
    public void StartPrinting()
    {
        if (badgeObject == null)
        {
            Debug.LogWarning("[BadgePrinter] No badge object assigned!");
            return;
        }

        if (!isPrinting)
        {
            printRoutine = StartCoroutine(PrintBadge(startLocalPos, endLocalPos));
        }
    }

    /// <summary>
    /// Reverses the print motion (pulls badge back in)
    /// </summary>
    public void RetractBadge()
    {
        if (badgeObject == null)
        {
            Debug.LogWarning("[BadgePrinter] No badge object assigned!");
            return;
        }

        if (!isPrinting)
        {
            printRoutine = StartCoroutine(PrintBadge(endLocalPos, startLocalPos));
        }
    }


    private IEnumerator PrintBadge(Vector3 from, Vector3 to)
    {
        isPrinting = true;
        OnPrintStart?.Invoke();

        badgeObject.localPosition = from;

        float elapsed = 0f;

        while (elapsed < printDuration)
        {
            elapsed += Time.deltaTime;

            float normalized = Mathf.Clamp01(elapsed / printDuration);
            float curveValue = easing.Evaluate(normalized);

            badgeObject.localPosition = Vector3.LerpUnclamped(from, to, curveValue);

            yield return null;
        }

        badgeObject.localPosition = to;

        OnPrintComplete?.Invoke();
        isPrinting = false;
    }
}
