using System.Collections;
using UnityEngine;
using TMPro;

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

    [Header("Behavior")]
    [SerializeField] private float messageHoldSeconds = 1.2f;
    [SerializeField] private int maxDigits = 6;

    private Coroutine _messageRoutine;

    private void Awake()
    {
        if (!keypad)
            keypad = GetComponentInParent<AlarmKeypad>();

        inputField.characterLimit = maxDigits;
        inputField.contentType = TMP_InputField.ContentType.IntegerNumber;
        inputField.onSubmit.AddListener(OnSubmit);
    }

    private void OnEnable()
    {
        SetIdle();

        if (AlarmSystem.Instance != null)
        {
            AlarmSystem.Instance.OnAlarmTriggered += HandleAlarmTriggered;
            AlarmSystem.Instance.OnAlarmStopped += HandleAlarmStopped;
        }
    }

    private void OnDisable()
    {
        if (AlarmSystem.Instance == null) return;

        AlarmSystem.Instance.OnAlarmTriggered -= HandleAlarmTriggered;
        AlarmSystem.Instance.OnAlarmStopped -= HandleAlarmStopped;
    }

    // ---------- Button Hooks ----------

    public void PressDigit(string digit)
    {
        if (!CanAcceptInput()) return;
        if (inputField.text.Length >= maxDigits) return;

        inputField.text += digit;
        inputField.caretPosition = inputField.text.Length;
    }

    public void PressDelete()
    {
        if (!CanAcceptInput()) return;
        if (inputField.text.Length == 0) return;

        inputField.text = inputField.text.Substring(0, inputField.text.Length - 1);
        inputField.caretPosition = inputField.text.Length;
    }

    public void PressEnter()
    {
        if (!CanAcceptInput()) return;
        OnSubmit(inputField.text);
    }

    // ---------- Logic ----------

    private void OnSubmit(string value)
    {
        bool success = keypad.TrySubmitCode(value);

        if (success)
        {
            ShowMessage(correctText);
            inputField.text = "";
            inputField.interactable = false; // optional lockout
        }
        else
        {
            ShowMessage(wrongText);
            inputField.text = "";
        }
    }

    private bool CanAcceptInput()
    {
        if (AlarmSystem.Instance == null) return false;
        if (keypad == null) return false;

        // Only allow input if alarm is active OR your keypad setting explicitly allows when inactive
        // (keypad.TrySubmitCode already enforces allowWhenAlarmInactive, but this keeps UI consistent)
        return AlarmSystem.Instance.AlarmActive;
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

        inputField.text = "";
        inputField.interactable = true;
        inputField.ActivateInputField();
    }

    // ---------- Alarm State ----------

    private void HandleAlarmTriggered()
    {
        SetIdle();
    }

    private void HandleAlarmStopped()
    {
        inputField.text = "";
    }
}
