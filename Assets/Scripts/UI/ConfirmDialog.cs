using System;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public sealed class UIConfirmDialog : MonoBehaviour
{
    [SerializeField] private UIPanel panel;

    [Header("UI")]
    [SerializeField] private TMP_Text titleText;
    [SerializeField] private TMP_Text bodyText;
    [SerializeField] private Button confirmButton;
    [SerializeField] private Button cancelButton;

    Action onConfirm;
    Action onCancel;
    bool isOpen;

    private void Reset()
    {
        panel = GetComponent<UIPanel>();
    }

    private void Awake()
    {
        if (!panel) panel = GetComponent<UIPanel>();

        if (confirmButton) confirmButton.onClick.AddListener(Confirm);
        if (cancelButton) cancelButton.onClick.AddListener(Cancel);

        panel.Hide(instant: true);
        isOpen = false;
    }

    private void Update()
    {
        if (!isOpen) return;

        // Optional: Escape cancels (PC polish)
        if (Input.GetKeyDown(KeyCode.Escape))
            Cancel();
    }

    public void Open(string title, string body, Action confirm, Action cancel = null, bool instant = false)
    {
        // Close any prior dialog cleanly
        onConfirm = confirm;
        onCancel = cancel;

        if (titleText) titleText.text = title;
        if (bodyText) bodyText.text = body;

        isOpen = true;
        panel.Show(instant);
    }

    public void Close(bool instant = false)
    {
        isOpen = false;
        panel.Hide(instant);
        onConfirm = null;
        onCancel = null;
    }

    void Confirm()
    {
        var cb = onConfirm;
        Close();
        cb?.Invoke();
    }

    void Cancel()
    {
        var cb = onCancel;
        Close();
        cb?.Invoke();
    }
}