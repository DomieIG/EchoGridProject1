// AlarmKeypadInputFieldUI.cs
using System.Collections;
using UnityEngine;
using TMPro;

[DisallowMultipleComponent]
public class AlarmKeypadInputFieldUI : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private AlarmKeypad keypad;
    [SerializeField] private TMP_InputField inputField;
    [SerializeField] private TMP_Text statusText;

    [Header("UI Text")]
    [SerializeField] private string idleText = "ENTER CODE";
    [SerializeField] private string wrongText = "TRY AGAIN";
    [SerializeField] private string correctText = "CORRECT";
    [SerializeField] private string lockedText = "LOCKED";

    [Header("Behavior")]
    [SerializeField, Min(0.1f)] private float messageHoldSeconds = 1.2f;
    [SerializeField, Range(1, 12)] private int maxDigits = 6;

    [Tooltip("If true, the keypad locks after a correct code until suppression ends.")]
    [SerializeField] private bool lockoutOnSuccess = true;

    [Header("Debug")]
    [SerializeField] private bool debugLogs = false;
    [SerializeField] private string debugTag = "[KEYPAD_UI]";

    private Coroutine _messageRoutine;
    private bool _suppressNextSubmitEvent;
    private bool _lockedOut;

    private void Awake()
    {
        if (!keypad)
            keypad = GetComponentInParent<AlarmKeypad>();

        if (!inputField)
            inputField = GetComponentInChildren<TMP_InputField>(true);

        if (!statusText)
            statusText = GetComponentInChildren<TMP_Text>(true);

        if (inputField)
        {
            inputField.characterLimit = maxDigits;
            inputField.contentType = TMP_InputField.ContentType.IntegerNumber;

            // TMP will fire this when pressing Enter while focused.
            inputField.onSubmit.AddListener(HandleTMPSubmit);
        }
    }

    private void OnEnable()
    {
        SubscribeAlarmEvents();
        SetIdle();
    }

    private void OnDisable()
    {
        UnsubscribeAlarmEvents();

        if (inputField)
            inputField.onSubmit.RemoveListener(HandleTMPSubmit);
    }

    // ---------- Button Hooks ----------

    public void PressDigit(string digit)
    {
        if (!CanAcceptInput()) return;
        if (!inputField) return;
        if (string.IsNullOrEmpty(digit)) return;
        if (inputField.text.Length >= maxDigits) return;

        inputField.text += digit;
        inputField.caretPosition = inputField.text.Length;
    }

    public void PressDelete()
    {
        if (!CanAcceptInput()) return;
        if (!inputField) return;
        if (inputField.text.Length == 0) return;

        inputField.text = inputField.text.Substring(0, inputField.text.Length - 1);
        inputField.caretPosition = inputField.text.Length;
    }

    public void PressEnter()
    {
        if (!CanAcceptInput()) return;
        if (!inputField) return;

        _suppressNextSubmitEvent = true;
        Submit(inputField.text);
    }

    // ---------- TMP Submit Handling ----------

    private void HandleTMPSubmit(string value)
    {
        if (_suppressNextSubmitEvent)
        {
            _suppressNextSubmitEvent = false;
            return;
        }

        Submit(value);
    }

    private void Submit(string value)
    {
        if (!keypad)
        {
            Log("Submit blocked: missing AlarmKeypad reference.");
            return;
        }

        if (!CanAcceptInput())
        {
            ShowMessage(lockedText);
            return;
        }

        bool success = keypad.TrySubmitCode(value);

        if (success)
        {
            ShowMessage(correctText);
            ClearInput();

            if (lockoutOnSuccess)
            {
                _lockedOut = true;
                UpdateInteractable();
            }
        }
        else
        {
            ShowMessage(wrongText);
            ClearInput();
        }
    }

    // ---------- Gating + Interactable Sync ----------

    private bool CanAcceptInput()
    {
        if (!keypad) return false;
        if (!inputField) return false;
        if (_lockedOut) return false;
        return keypad.CanInteractNow;
    }

    private void UpdateInteractable()
    {
        if (!inputField) return;

        bool canUse = keypad && keypad.CanInteractNow && !_lockedOut;
        inputField.interactable = canUse;

        if (canUse)
            inputField.ActivateInputField();

        Log($"UpdateInteractable -> canUse={canUse}, lockedOut={_lockedOut}");
    }

    // ---------- UI Helpers ----------

    private void ClearInput()
    {
        if (!inputField) return;
        inputField.text = "";
        inputField.caretPosition = 0;
    }

    private void ShowMessage(string msg)
    {
        if (_messageRoutine != null)
            StopCoroutine(_messageRoutine);

        _messageRoutine = StartCoroutine(MessageRoutine(msg));
    }

    private IEnumerator MessageRoutine(string msg)
    {
        if (statusText)
            statusText.text = msg;

        yield return new WaitForSeconds(messageHoldSeconds);

        SetIdle();
        _messageRoutine = null;
    }

    private void SetIdle()
    {
        if (statusText)
            statusText.text = idleText;

        ClearInput();
        UpdateInteractable();
    }

    // ---------- Alarm Events ----------

    private void SubscribeAlarmEvents()
    {
        var alarm = AlarmSystem.Instance;
        if (alarm == null) return;

        alarm.OnAlarmTriggered += HandleAlarmStateChanged;
        alarm.OnAlarmStopped += HandleAlarmStateChanged;

        alarm.OnSuppressionStarted += HandleSuppressionStarted;
        alarm.OnSuppressionEnded += HandleSuppressionEnded;
    }

    private void UnsubscribeAlarmEvents()
    {
        var alarm = AlarmSystem.Instance;
        if (alarm == null) return;

        alarm.OnAlarmTriggered -= HandleAlarmStateChanged;
        alarm.OnAlarmStopped -= HandleAlarmStateChanged;

        alarm.OnSuppressionStarted -= HandleSuppressionStarted;
        alarm.OnSuppressionEnded -= HandleSuppressionEnded;
    }

    private void HandleAlarmStateChanged()
    {
        // AlarmActive affects CanInteractNow when allowWhenAlarmInactive is false.
        SetIdle();
    }

    private void HandleSuppressionStarted(float _)
    {
        // During suppression, CanInteractNow becomes false. Keep lockout flag as-is.
        SetIdle();
    }

    private void HandleSuppressionEnded()
    {
        // Unlock after suppression ends
        _lockedOut = false;
        SetIdle();
    }

    private void Log(string msg)
    {
        if (!debugLogs) return;
        Debug.Log($"{debugTag} {msg}", this);
    }
}
