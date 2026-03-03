// UIPanelAnimator.cs
using System.Collections;
using UnityEngine;

[DisallowMultipleComponent]
[RequireComponent(typeof(CanvasGroup))]
public sealed class UIPanelAnimator : MonoBehaviour
{
    [SerializeField] private CanvasGroup canvasGroup;
    [SerializeField, Min(0f)] private float fadeDuration = 0.25f;
    [SerializeField] private bool useUnscaledTime = true;

    private Coroutine routine;

    private void Reset()
    {
        canvasGroup = GetComponent<CanvasGroup>();
    }

    private void Awake()
    {
        if (!canvasGroup) canvasGroup = GetComponent<CanvasGroup>();
    }

    public void FadeIn(bool instant = false) => Fade(1f, instant);
    public void FadeOut(bool instant = false) => Fade(0f, instant);

    public void Fade(float targetAlpha, bool instant = false)
    {
        if (!canvasGroup) return;

        StopRoutine();

        if (instant || fadeDuration <= 0f)
        {
            canvasGroup.alpha = targetAlpha;
            return;
        }

        routine = StartCoroutine(FadeRoutine(targetAlpha));
    }

    private IEnumerator FadeRoutine(float targetAlpha)
    {
        float start = canvasGroup.alpha;
        float t = 0f;

        while (t < fadeDuration)
        {
            t += useUnscaledTime ? Time.unscaledDeltaTime : Time.deltaTime;
            float u = Mathf.Clamp01(t / fadeDuration);
            canvasGroup.alpha = Mathf.Lerp(start, targetAlpha, u);
            yield return null;
        }

        canvasGroup.alpha = targetAlpha;
        routine = null;
    }

    private void StopRoutine()
    {
        if (routine != null)
        {
            StopCoroutine(routine);
            routine = null;
        }
    }
}