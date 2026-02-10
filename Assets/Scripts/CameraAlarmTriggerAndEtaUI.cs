// CameraAlarmTriggerAndEtaUI.cs
using TMPro;
using UnityEngine;

[DisallowMultipleComponent]
public class CameraAlarmTriggerAndEtaUI : MonoBehaviour
{
    private const string PlayerTag = "Player";

    [Header("References")]
    [SerializeField] private CameraArcController arcController;
    [SerializeField] private TextMeshProUGUI timerText;

    [Header("Behavior")]
    [SerializeField] private bool triggerAlarmOnEnter = true;
    [SerializeField] private bool stopAlarmOnExit = false;
    [SerializeField] private bool followPlayerOnEnter = true;
    [SerializeField] private bool hideTimerWhenAlarmInactive = true;
    [SerializeField] private bool hideTimerWhenPoliceArrive = false;

    [Header("Debug")]
    [SerializeField] private bool debugLogs = true;
    [SerializeField] private string debugTag = "[CAM_ALARM]";

    private AlarmSystem _alarm;

    private void Awake()
    {
        if (!arcController)
            arcController = GetComponentInParent<CameraArcController>();
    }

    private void OnEnable()
    {
        _alarm = AlarmSystem.Instance;
        if (_alarm == null)
        {
            Log("AlarmSystem.Instance not found. UI will not update and alarm won't trigger.");
            if (timerText) timerText.gameObject.SetActive(false);
            return;
        }

        _alarm.OnPoliceEtaChanged += HandlePoliceEtaChanged;
        _alarm.OnPoliceArrived += HandlePoliceArrived;
        _alarm.OnAlarmStopped += HandleAlarmStopped;
        _alarm.OnAlarmTriggered += HandleAlarmTriggered;

        RefreshTimerVisibility();
        ForceUpdateTimerText();
    }

    private void OnDisable()
    {
        if (_alarm == null) return;

        _alarm.OnPoliceEtaChanged -= HandlePoliceEtaChanged;
        _alarm.OnPoliceArrived -= HandlePoliceArrived;
        _alarm.OnAlarmStopped -= HandleAlarmStopped;
        _alarm.OnAlarmTriggered -= HandleAlarmTriggered;

        _alarm = null;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag(PlayerTag)) return;

        Log($"Player entered red light: {other.name}");
        if (_alarm == null) return;

        if (followPlayerOnEnter && arcController != null)
            arcController.TriggerFollow(other.transform);

        if (triggerAlarmOnEnter)
            _alarm.TriggerAlarm("Camera red light detection");

        RefreshTimerVisibility();
        ForceUpdateTimerText();
    }

    private void OnTriggerExit(Collider other)
    {
        if (!other.CompareTag(PlayerTag)) return;

        Log($"Player exited red light: {other.name}");
        if (_alarm == null) return;

        if (stopAlarmOnExit)
            _alarm.StopAlarm("Player left camera red light");

        RefreshTimerVisibility();
    }

    private void HandleAlarmTriggered()
    {
        RefreshTimerVisibility();
        ForceUpdateTimerText();
    }

    private void HandleAlarmStopped()
    {
        RefreshTimerVisibility();
    }

    private void HandlePoliceArrived()
    {
        if (hideTimerWhenPoliceArrive && timerText != null)
            timerText.gameObject.SetActive(false);
    }

    private void HandlePoliceEtaChanged(float remaining, float total)
    {
        if (!timerText || _alarm == null) return;
        if (hideTimerWhenPoliceArrive && _alarm.PoliceArrived) return;

        int minutes = Mathf.FloorToInt(remaining / 60f);
        int seconds = Mathf.FloorToInt(remaining % 60f);
        timerText.text = $"{minutes:00}:{seconds:00}";

        RefreshTimerVisibility();
    }

    private void RefreshTimerVisibility()
    {
        if (!timerText) return;

        if (_alarm == null)
        {
            timerText.gameObject.SetActive(false);
            return;
        }

        if (hideTimerWhenPoliceArrive && _alarm.PoliceArrived)
        {
            timerText.gameObject.SetActive(false);
            return;
        }

        if (!hideTimerWhenAlarmInactive)
        {
            timerText.gameObject.SetActive(true);
            return;
        }

        timerText.gameObject.SetActive(_alarm.AlarmActive);
    }

    private void ForceUpdateTimerText()
    {
        if (_alarm == null || timerText == null) return;

        float remaining = _alarm.GetPoliceRemainingSeconds();
        int minutes = Mathf.FloorToInt(remaining / 60f);
        int seconds = Mathf.FloorToInt(remaining % 60f);
        timerText.text = $"{minutes:00}:{seconds:00}";
    }

    private void Log(string msg)
    {
        if (!debugLogs) return;
        Debug.Log($"{debugTag} {msg}", this);
    }
}
