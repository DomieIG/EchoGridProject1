// UIConfirmDialog.cs
using System;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using TMPro;

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

    private GameObject previousSelected;

    private void Reset()
    {
        panel = GetComponent<UIPanel>();
    }

    private void Awake()
    {
        if (!panel) panel = GetComponent<UIPanel>();

        if (confirmButton) confirmButton.onClick.AddListener(Confirm);
        if (cancelButton) cancelButton.onClick.AddListener(Cancel);

        // Ensure hidden immediately at boot (prevents one-frame flashes)
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
        if (!gameObject.activeSelf) gameObject.SetActive(true);

        previousSelected = EventSystem.current ? EventSystem.current.currentSelectedGameObject : null;

        onConfirm = confirm;
        onCancel = cancel;

        if (titleText) titleText.text = title;
        if (bodyText) bodyText.text = body;

        isOpen = true;
        panel.Show(instant);

        // Optionally focus confirm button for keyboard/controller
        if (EventSystem.current && confirmButton)
            EventSystem.current.SetSelectedGameObject(confirmButton.gameObject);
    }

    private void Close(bool instant = false)
    {
        isOpen = false;
        panel.Hide(instant);

        onConfirm = null;
        onCancel = null;

        // Restore selection for controller/keyboard UX
        if (EventSystem.current)
            EventSystem.current.SetSelectedGameObject(previousSelected);

        previousSelected = null;
    }

    private void Confirm()
    {
        if (!isOpen) return;
        var cb = onConfirm;
        Close();
        cb?.Invoke();
    }

    private void Cancel()
    {
        if (!isOpen) return;
        var cb = onCancel;
        Close();
        cb?.Invoke();
    }

    private void ForceHiddenImmediate()
    {
        isOpen = false;

        // If you want the dialog object to remain discoverable:
        // In the UIPanel inspector, set disableGameObjectWhenHidden = false.
        panel.Hide(instant: true);
    }
}