using TMPro;
using UnityEngine;

[DisallowMultipleComponent]
public class CameraAlarmTriggerAndEtaUI : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private CameraArcController arcController;
    [SerializeField] private TextMeshProUGUI timerText;

    [Header("Behavior")]
    [Tooltip("If true, trigger the alarm when player enters the red light.")]
    [SerializeField] private bool triggerAlarmOnEnter = true;

    [Tooltip("If true, call StopAlarm when player exits the red light. (Most games leave alarms latched; keep false unless you want it.)")]
    [SerializeField] private bool stopAlarmOnExit = false;

    [Tooltip("If true, camera will switch to follow mode when player enters the red light.")]
    [SerializeField] private bool followPlayerOnEnter = true;

    [Tooltip("If true, hide the timer UI when alarm is not active.")]
    [SerializeField] private bool hideTimerWhenAlarmInactive = true;

    [Tooltip("If true, hide the timer UI once police have arrived.")]
    [SerializeField] private bool hideTimerWhenPoliceArrive = false;

    [Header("Debug")]
    [SerializeField] private bool debugLogs = true;
    [SerializeField] private string debugTag = "[CAM_ALARM]";

    private AlarmSystem _alarm;
    private bool _playerInside;

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
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player")) return;

        _playerInside = true;
        Log($"Player entered red light: {other.name}");

        if (_alarm == null)
        {
            Log("No AlarmSystem found; cannot trigger alarm.");
            return;
        }

        if (followPlayerOnEnter && arcController != null)
        {
            arcController.TriggerFollow(other.transform);
            Log("Camera follow triggered.");
        }

        if (triggerAlarmOnEnter)
        {
            _alarm.TriggerAlarm("Camera red light detection");
        }

        RefreshTimerVisibility();
        ForceUpdateTimerText();
    }

    private void OnTriggerExit(Collider other)
    {
        if (!other.CompareTag("Player")) return;

        _playerInside = false;
        Log($"Player exited red light: {other.name}");

        if (_alarm == null) return;

        if (stopAlarmOnExit)
        {
            _alarm.StopAlarm("Player left camera red light");
        }

        RefreshTimerVisibility();
    }

    private void HandleAlarmTriggered()
    {
        Log("Alarm triggered (event).");
        RefreshTimerVisibility();
        ForceUpdateTimerText();
    }

    private void HandleAlarmStopped()
    {
        Log("Alarm stopped (event).");
        RefreshTimerVisibility();
    }

    private void HandlePoliceArrived()
    {
        Log("Police arrived (event).");
        if (hideTimerWhenPoliceArrive && timerText != null)
            timerText.gameObject.SetActive(false);
    }

    private void HandlePoliceEtaChanged(float remaining, float total)
    {
        if (timerText == null) return;

        // If you only want to show ETA when player is inside the red light, uncomment:
        // if (!_playerInside) return;

        if (hideTimerWhenPoliceArrive && _alarm != null && _alarm.PoliceArrived)
            return;

        // Format mm:ss
        int minutes = Mathf.FloorToInt(remaining / 60f);
        int seconds = Mathf.FloorToInt(remaining % 60f);
        timerText.text = $"{minutes:00}:{seconds:00}";

        RefreshTimerVisibility();
    }

    private void RefreshTimerVisibility()
    {
        if (timerText == null) return;

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

        // Show timer only when alarm is active OR player is inside, depending on what you want:
        // Option A (strict): only show when alarm active
        timerText.gameObject.SetActive(_alarm.AlarmActive);

        // Option B (show while player is inside trigger):
        // timerText.gameObject.SetActive(_playerInside);
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
