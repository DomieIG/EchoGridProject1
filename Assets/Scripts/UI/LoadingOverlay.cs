// UILoadingOverlay.cs
using System.Collections;
using TMPro;
using UnityEngine;

[RequireComponent(typeof(CanvasGroup))]
public sealed class UILoadingOverlay : MonoBehaviour
{
    [SerializeField] private UIConfig config;
    [SerializeField] private TMP_Text statusText;
    [SerializeField] private TMP_Text percentText; // optional

    private CanvasGroup cg;
    private Coroutine routine;

    private void Awake()
    {
        cg = GetComponent<CanvasGroup>();
        SetVisible(false, instant: true);
    }

    public void Show(string status = "Loading...", bool instant = false)
    {
        if (statusText) statusText.text = status;
        if (percentText) percentText.text = string.Empty;
        SetVisible(true, instant);
    }

    public void SetProgress01(float p01)
    {
        if (!percentText) return;
        p01 = Mathf.Clamp01(p01);
        int pct = Mathf.RoundToInt(p01 * 100f);
        percentText.text = pct + "%";
    }

    public void Hide(bool instant = false) => SetVisible(false, instant);

    private void SetVisible(bool visible, bool instant)
    {
        if (routine != null)
        {
            StopCoroutine(routine);
            routine = null;
        }

        float target = visible ? 1f : 0f;
        cg.blocksRaycasts = visible;
        cg.interactable = visible;

        bool reduce = config && config.reducedMotion;
        float d = config ? config.loadingFadeDuration : 0.2f;

        if (instant || reduce || d <= 0f)
        {
            cg.alpha = target;
            gameObject.SetActive(visible);
            return;
        }

        gameObject.SetActive(true);
        routine = StartCoroutine(FadeTo(target, visible, d));
    }

    private IEnumerator FadeTo(float target, bool finalVisible, float d)
    {
        float start = cg.alpha;
        float t = 0f;

        while (t < d)
        {
            t += Time.unscaledDeltaTime;
            float u = UIEase.Clamp01(t / d);
            cg.alpha = Mathf.Lerp(start, target, u);
            yield return null;
        }

        cg.alpha = target;

        if (!finalVisible) gameObject.SetActive(false);
        routine = null;
    }
}