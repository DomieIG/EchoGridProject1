// UIButtonFX.cs
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

[DisallowMultipleComponent]
public sealed class UIButtonFX : MonoBehaviour,
    IPointerEnterHandler, IPointerExitHandler, IPointerDownHandler, IPointerUpHandler
{
    [Header("Config (Optional)")]
    [SerializeField] private UIConfig config;

    [Header("Target")]
    [SerializeField] private Transform target;

    [Header("Overrides (used if no config)")]
    [SerializeField] private float hoverScale = 1.05f;
    [SerializeField] private float pressedScale = 0.98f;
    [SerializeField] private float speed = 18f;

    [Header("Optional Audio")]
    [SerializeField] private UIAudio uiAudio;
    [SerializeField] private bool playHoverSound = true;
    [SerializeField] private bool playClickSound = true;

    [Header("Optional Input Lock Source")]
    [SerializeField] private UIManager uiManager;

    private Selectable selectable;
    private Vector3 baseScale;
    private float desired = 1f;
    private bool hovering;
    private bool pressing;

    private void Reset()
    {
        target = transform;
        selectable = GetComponent<Selectable>();
    }

    private void Awake()
    {
        if (!target) target = transform;
        selectable = GetComponent<Selectable>();
        if (!uiManager) uiManager = FindFirstObjectByType<UIManager>();

        baseScale = target.localScale;

        // Avoid divide-by-zero / weird scaling if someone set scale to 0
        if (Mathf.Approximately(baseScale.x, 0f)) baseScale.x = 1f;
        if (Mathf.Approximately(baseScale.y, 0f)) baseScale.y = 1f;
    }

    private void OnEnable()
    {
        hovering = false;
        pressing = false;
        desired = 1f;

        if (target) target.localScale = baseScale;
    }

    private void OnDisable()
    {
        hovering = false;
        pressing = false;
        desired = 1f;

        if (target) target.localScale = baseScale;
    }

    private void Update()
    {
        if (!target) return;

        float current = target.localScale.x / baseScale.x;
        float lerpSpeed = GetSpeed();

        // Smooth exponential damp (stable at any framerate)
        float next = Mathf.Lerp(current, desired, 1f - Mathf.Exp(-lerpSpeed * Time.unscaledDeltaTime));
        target.localScale = baseScale * next;
    }

    private bool CanInteract()
    {
        if (uiManager && uiManager.InputLocked) return false;
        if (selectable && !selectable.IsInteractable()) return false;
        return true;
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (!CanInteract()) return;

        hovering = true;
        Recalc();

        if (uiAudio && playHoverSound) uiAudio.PlayHover();
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        hovering = false;
        pressing = false;
        Recalc();
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        if (!CanInteract()) return;

        pressing = true;
        Recalc();

        if (uiAudio && playClickSound) uiAudio.PlayClick();
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        pressing = false;
        Recalc();
    }

    private void Recalc()
    {
        if (!CanInteract())
        {
            desired = 1f;
            return;
        }

        if (pressing) desired = GetPressedScale();
        else if (hovering) desired = GetHoverScale();
        else desired = 1f;
    }

    private float GetHoverScale() => config ? config.hoverScale : hoverScale;
    private float GetPressedScale() => config ? config.pressedScale : pressedScale;
    private float GetSpeed() => config ? config.scaleLerpSpeed : speed;
}