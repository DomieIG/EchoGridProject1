using System.Collections;
using UnityEngine;
using TMPro; // If using TextMeshPro. If using legacy Text, I included notes below.

public class AlarmKeypadUI : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private AlarmKeypad keypad;
    [SerializeField] private TMP_Text displayText;

    [Header("Display")]
    [SerializeField] private string idleText = "ENTER CODE";
    [SerializeField] private bool maskInput = true;
    [SerializeField] private char maskChar = '*';

    [Header("Feedback")]
    [SerializeField] private float messageHoldSeconds = 1.0f;
    [SerializeField] private string wrongText = "TRY AGAIN";
    [SerializeField] private string correctText = "CORRECT";

    private string _buffer = "";
    private Coroutine _messageRoutine;

    private void Awake()
    {
        if (!keypad) keypad = GetComponentInParent<AlarmKeypad>();
    }

    private void OnEnable()
    {
        if (AlarmSystem.Instance != null)
        {
            AlarmSystem.Instance.OnAlarmTriggered += HandleAlarmTriggered;
            AlarmSystem.Instance.OnAlarmStopped += HandleAlarmStopped;
            AlarmSystem.Instance.OnSuppressionStarted += HandleSuppressionStarted;
        }

        SetIdle();
    }

    private void OnDisable()
    {
        if (AlarmSystem.Instance == null) return;

        AlarmSystem.Instance.OnAlarmTriggered -= HandleAlarmTriggered;
        AlarmSystem.Instance.OnAlarmStopped -= HandleAlarmStopped;
        AlarmSystem.Instance.OnSuppressionStarted -= HandleSuppressionStarted;
    }

    // ----- Button hooks -----

    public void PressDigit(int digit)
    {
        // optional guard: only accept digits while alarm active (unless keypad allows otherwise)
        if (!CanAcceptInput()) return;

        if (_buffer.Length >= keypad.MaxDigits) return;

        _buffer += digit.ToString();
        RenderBuffer();
    }

    public void PressDelete()
    {
        if (!CanAcceptInput()) return;

        if (_buffer.Length <= 0) return;
        _buffer = _buffer.Substring(0, _buffer.Length - 1);
        RenderBuffer();
    }

    public void PressClear()
    {
        if (!CanAcceptInput()) return;

        _buffer = "";
        RenderBuffer();
    }

    public void PressEnter()
    {
        if (!CanAcceptInput()) return;

        bool ok = keypad.TrySubmitCode(_buffer);

        if (ok)
        {
            ShowMessage(correctText);
            _buffer = "";
            // AlarmSystem.StartSuppression() already stops the alarm
        }
        else
        {
            ShowMessage(wrongText);
            _buffer = "";
        }
    }

    // ----- Internals -----

    private bool CanAcceptInput()
    {
        if (!keypad) return false;
        if (AlarmSystem.Instance == null) return false;

        // If keypad itself allows when inactive, let it decide. Otherwise require alarm active.
        // This keeps behavior consistent with AlarmKeypad.TrySubmitCode() rules.
        return true;
    }

    private void RenderBuffer()
    {
        if (!displayText) return;

        if (string.IsNullOrEmpty(_buffer))
        {
            displayText.text = idleText;
            return;
        }

        if (maskInput)
            displayText.text = new string(maskChar, _buffer.Length);
        else
            displayText.text = _buffer;
    }

    private void SetIdle()
    {
        if (!displayText) return;
        _buffer = "";
        displayText.text = idleText;
    }

    private void ShowMessage(string msg)
    {
        if (_messageRoutine != null) StopCoroutine(_messageRoutine);
        _messageRoutine = StartCoroutine(MessageRoutine(msg));
    }

    private IEnumerator MessageRoutine(string msg)
    {
        if (displayText) displayText.text = msg;
        yield return new WaitForSeconds(messageHoldSeconds);
        _messageRoutine = null;
        RenderBuffer();
    }

    // ----- Optional alarm state reactions -----

    private void HandleAlarmTriggered()
    {
        // When alarm triggers, prompt for code
        SetIdle();
    }

    private void HandleAlarmStopped()
    {
        // If alarm stops (even not from keypad), show idle
        SetIdle();
    }

    private void HandleSuppressionStarted(float seconds)
    {
        // You could show something like "SUPPRESSED" here if you want
        // ShowMessage("SUPPRESSED");
    }
}
