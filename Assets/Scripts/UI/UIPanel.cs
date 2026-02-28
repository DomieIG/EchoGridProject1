using System.Collections;
using UnityEngine;

[DisallowMultipleComponent]
[RequireComponent(typeof(CanvasGroup))]
public sealed class UIPanel : MonoBehaviour
{
    [SerializeField] private float duration = 0.22f;
    [SerializeField, Range(0.5f, 1f)] private float hiddenScale = 0.97f;
    [SerializeField] private bool disableGameObjectWhenHidden = true;

    CanvasGroup cg;
    RectTransform rt;
    Coroutine routine;

    private void Awake()
    {
        cg = GetComponent<CanvasGroup>();
        rt = GetComponent<RectTransform>();
    }

    public void Show(bool instant = false)
    {
        if (!gameObject.activeSelf) gameObject.SetActive(true);

        SetInteractable(true);

        if (instant)
        {
            cg.alpha = 1f;
            rt.localScale = Vector3.one;
            StopRoutine();
            return;
        }

        StartAnim(Animate(cg.alpha, 1f, rt.localScale.x, 1f, duration, null));
    }

    public void Hide(bool instant = false)
    {
        SetInteractable(false);

        if (instant)
        {
            cg.alpha = 0f;
            rt.localScale = new Vector3(hiddenScale, hiddenScale, 1f);
            StopRoutine();
            if (disableGameObjectWhenHidden) gameObject.SetActive(false);
            return;
        }

        StartAnim(Animate(cg.alpha, 0f, rt.localScale.x, hiddenScale, duration, () =>
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
        if (routine != null) { StopCoroutine(routine); routine = null; }
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
            float u = Mathf.Clamp01(t / d);
            float e = 1f - Mathf.Pow(1f - u, 3f);

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
}