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

    Button button;
    Vector3 baseScale;
    float desired = 1f;
    bool hovering;
    bool pressing;

    private void Reset()
    {
        target = transform;
        button = GetComponent<Button>();
    }

    private void Awake()
    {
        if (!target) target = transform;
        button = GetComponent<Button>();
        if (!uiManager) uiManager = FindFirstObjectByType<UIManager>();

        baseScale = target.localScale;
    }

    private void OnEnable()
    {
        hovering = false;
        pressing = false;
        desired = 1f;
        target.localScale = baseScale;
    }

    private void Update()
    {
        float current = target.localScale.x / baseScale.x;
        float lerpSpeed = GetSpeed();
        float next = Mathf.Lerp(current, desired, 1f - Mathf.Exp(-lerpSpeed * Time.unscaledDeltaTime));
        target.localScale = baseScale * next;
    }

    bool CanInteract()
    {
        if (uiManager && uiManager.InputLocked) return false;
        if (button && !button.interactable) return false;
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

    void Recalc()
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

    float GetHoverScale() => config ? config.hoverScale : hoverScale;
    float GetPressedScale() => config ? config.pressedScale : pressedScale;
    float GetSpeed() => config ? config.scaleLerpSpeed : speed;
}