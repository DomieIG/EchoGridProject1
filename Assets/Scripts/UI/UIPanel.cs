// UIPanel.cs
using System.Collections;
using UnityEngine;

[DisallowMultipleComponent]
[RequireComponent(typeof(CanvasGroup))]
public sealed class UIPanel : MonoBehaviour
{
    [Header("Config (Optional)")]
    [SerializeField] private UIConfig config;

    [Header("Overrides (used if no config)")]
    [SerializeField, Min(0f)] private float duration = 0.22f;
    [SerializeField, Range(0.5f, 1f)] private float hiddenScale = 0.97f;

    [Header("Behavior")]
    [SerializeField] private bool disableGameObjectWhenHidden = true;

    private CanvasGroup cg;
    private RectTransform rt;
    private Coroutine routine;

    private void Awake()
    {
        cg = GetComponent<CanvasGroup>();
        rt = GetComponent<RectTransform>();
    }

    public void Show(bool instant = false)
    {
        if (!gameObject.activeSelf) gameObject.SetActive(true);

        SetInteractable(true);

        if (instant || IsReducedMotion())
        {
            StopRoutine();
            cg.alpha = 1f;
            rt.localScale = Vector3.one;
            return;
        }

        StartAnim(Animate(cg.alpha, 1f, rt.localScale.x, 1f, GetDuration(), null));
    }

    public void Hide(bool instant = false)
    {
        SetInteractable(false);

        if (instant || IsReducedMotion())
        {
            StopRoutine();
            cg.alpha = 0f;

            float hs = GetHiddenScale();
            rt.localScale = new Vector3(hs, hs, 1f);

            if (disableGameObjectWhenHidden) gameObject.SetActive(false);
            return;
        }

        float hs2 = GetHiddenScale();
        StartAnim(Animate(cg.alpha, 0f, rt.localScale.x, hs2, GetDuration(), () =>
        {
            if (disableGameObjectWhenHidden) gameObject.SetActive(false);
        }));
    }

    public void SetInteractable(bool on)
    {
        if (!cg) return;
        cg.interactable = on;
        cg.blocksRaycasts = on;
    }

    private void StopRoutine()
    {
        if (routine != null)
        {
            StopCoroutine(routine);
            routine = null;
        }
    }

    private void StartAnim(IEnumerator e)
    {
        StopRoutine();
        routine = StartCoroutine(e);
    }

    private IEnumerator Animate(float fromA, float toA, float fromS, float toS, float d, System.Action onDone)
    {
        float t = 0f;

        while (t < d)
        {
            t += Time.unscaledDeltaTime;
            float u = Mathf.Clamp01(t / d);
            float e = UIEase.EaseOutCubic(u);

            cg.alpha = Mathf.Lerp(fromA, toA, e);
            float s = Mathf.Lerp(fromS, toS, e);
            rt.localScale = new Vector3(s, s, 1f);

            yield return null;
        }

        cg.alpha = toA;
        rt.localScale = new Vector3(toS, toS, 1f);

        routine = null;
        onDone?.Invoke();
    }

    private float GetDuration() => config ? config.panelDuration : duration;
    private float GetHiddenScale() => config ? config.panelHiddenScale : hiddenScale;
    private bool IsReducedMotion() => config && config.reducedMotion;
}