using System.Collections;
using UnityEngine;

[DisallowMultipleComponent]
[RequireComponent(typeof(CanvasGroup))]
public sealed class UIPanel : MonoBehaviour
{
    [Header("Config (Optional)")]
    [SerializeField] private UIConfig config;

    [Header("Animation Overrides (used if no config)")]
    [SerializeField, Min(0.05f)] private float duration = 0.22f;
    [SerializeField] private float hiddenScale = 0.97f;

    [Header("Behavior")]
    [SerializeField] private bool disableGameObjectWhenHidden = true;

    CanvasGroup cg;
    RectTransform rt;
    Coroutine routine;
    bool isVisible;

    public bool IsVisible => isVisible;

    private void Reset()
    {
        cg = GetComponent<CanvasGroup>();
        rt = GetComponent<RectTransform>();
    }

    private void Awake()
    {
        cg = GetComponent<CanvasGroup>();
        rt = GetComponent<RectTransform>();
    }

    private void OnValidate()
    {
        if (duration < 0.05f) duration = 0.05f;
        hiddenScale = Mathf.Clamp(hiddenScale, 0.5f, 1f);
    }

    public void Show(bool instant = false)
    {
        isVisible = true;
        gameObject.SetActive(true);

        SetInteractable(true);

        float d = GetDuration();
        float hs = GetHiddenScale();
        bool reduce = ReducedMotion();

        if (instant || reduce)
        {
            cg.alpha = 1f;
            rt.localScale = Vector3.one;
            StopRoutine();
            return;
        }

        StartAnim(Animate(cg.alpha, 1f, rt.localScale.x, 1f, d, null));
    }

    public void Hide(bool instant = false)
    {
        isVisible = false;
        SetInteractable(false);

        float d = GetDuration();
        float hs = GetHiddenScale();
        bool reduce = ReducedMotion();

        if (instant || reduce)
        {
            cg.alpha = 0f;
            rt.localScale = new Vector3(hs, hs, 1f);
            StopRoutine();
            if (disableGameObjectWhenHidden) gameObject.SetActive(false);
            return;
        }

        StartAnim(Animate(cg.alpha, 0f, rt.localScale.x, hs, d, () =>
        {
            if (disableGameObjectWhenHidden) gameObject.SetActive(false);
        }));
    }

    public void SetInteractable(bool on)
    {
        cg.interactable = on;
        cg.blocksRaycasts = on;
    }

    void StopRoutine()
    {
        if (routine != null)
        {
            StopCoroutine(routine);
            routine = null;
        }
    }

    void StartAnim(IEnumerator e)
    {
        StopRoutine();
        routine = StartCoroutine(e);
    }

    IEnumerator Animate(float fromA, float toA, float fromS, float toS, float d, System.Action onDone)
    {
        float t = 0f;

        while (t < d)
        {
            t += Time.unscaledDeltaTime;
            float u = UIEase.Clamp01(t / d);
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

    float GetDuration() => config ? (config.reducedMotion ? 0f : config.panelDuration) : duration;
    float GetHiddenScale() => config ? config.panelHiddenScale : hiddenScale;
    bool ReducedMotion() => config && config.reducedMotion;
}