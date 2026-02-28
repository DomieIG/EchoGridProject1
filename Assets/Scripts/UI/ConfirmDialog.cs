using System;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.InputSystem;

[RequireComponent(typeof(UIPanel))]
public sealed class UIConfirmDialog : MonoBehaviour
{
    [SerializeField] private UIPanel panel;

    [Header("UI")]
    [SerializeField] private TMP_Text titleText;
    [SerializeField] private TMP_Text bodyText;
    [SerializeField] private Button confirmButton;
    [SerializeField] private Button cancelButton;

    private Action onConfirm;
    private Action onCancel;
    private bool isOpen;

    private void Reset()
    {
        panel = GetComponent<UIPanel>();
    }

    private void Awake()
    {
        if (!panel) panel = GetComponent<UIPanel>();

        if (confirmButton) confirmButton.onClick.AddListener(Confirm);
        if (cancelButton) cancelButton.onClick.AddListener(Cancel);

        // IMPORTANT: ensure hidden immediately at boot
        ForceHiddenImmediate();
    }

    private void Start()
    {
        // Extra safety for script execution order / one-frame flashes
        ForceHiddenImmediate();
    }

    private void Update()
    {
        if (!isOpen) return;

        // New Input System: Escape cancels
        if (Keyboard.current != null && Keyboard.current.escapeKey.wasPressedThisFrame)
            Cancel();
    }

    public void Open(string title, string body, Action confirm, Action cancel = null, bool instant = false)
    {
        // Keep it active so it can always be found
        if (!gameObject.activeSelf) gameObject.SetActive(true);

        onConfirm = confirm;
        onCancel = cancel;

        if (titleText) titleText.text = title;
        if (bodyText) bodyText.text = body;

        isOpen = true;
        panel.Show(instant);
    }

    private void Close(bool instant = false)
    {
        isOpen = false;
        panel.Hide(instant);

        onConfirm = null;
        onCancel = null;
    }

    private void Confirm()
    {
        var cb = onConfirm;
        Close();
        cb?.Invoke();
    }

    private void Cancel()
    {
        var cb = onCancel;
        Close();
        cb?.Invoke();
    }

    private void ForceHiddenImmediate()
    {
        isOpen = false;

        // Hide without disabling the GameObject (so it remains discoverable)
        // IMPORTANT: set this in the UIPanel inspector too:
        // disableGameObjectWhenHidden = false
        panel.Hide(instant: true);
    }
}