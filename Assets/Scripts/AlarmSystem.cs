using System;
using UnityEngine;

public class AlarmSystem : MonoBehaviour
{
    public static AlarmSystem Instance { get; private set; }

    [Header("Debug")]
    [SerializeField] private bool debugLogs = true;
    [SerializeField] private string debugTag = "[ALARM]";

    public bool AlarmActive { get; private set; }
    public bool Suppressed => _suppressedUntilTime > Time.time;

    // --- Events ---
    public event Action OnAlarmTriggered;
    public event Action OnAlarmStopped;

    public event Action<float> OnSuppressionStarted; // seconds
    public event Action OnSuppressionEnded;

    // Police escalation events (same timer for police + UI)
    public event Action<float, float> OnPoliceEtaChanged; // remaining, total
    public event Action OnPoliceArrived;
    public event Action OnPoliceEscalationReset;

    [Header("Suppression")]
    [SerializeField] private float suppressionDurationSeconds = 300f; // 5 minutes

    [Header("Police Escalation")]
    [Tooltip("How long the alarm must be active (and not suppressed) before police arrive.")]
    [SerializeField] private float secondsUntilPoliceArrive = 120f;

    [Tooltip("If true, suppression resets police escalation progress to 0. If false, suppression pauses progress.")]
    [SerializeField] private bool suppressionResetsPoliceProgress = false;

    private float _suppressedUntilTime = -1f;

    // “Unsuppressed active” accumulation
    private float _alarmActiveUnsuppressedTime;
    private bool _policeArrived;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
    }

    private void Update()
    {
        // Suppression end
        if (_suppressedUntilTime > 0f && Time.time >= _suppressedUntilTime)
        {
            _suppressedUntilTime = -1f;
            Log("Suppression ended.");
            OnSuppressionEnded?.Invoke();
        }

        // Police escalation timing (single source of truth)
        TickPoliceEscalation();
    }

    private void TickPoliceEscalation()
    {
        if (_policeArrived)
            return;

        // Only count when alarm is actively sounding AND not suppressed
        if (AlarmActive && !Suppressed)
        {
            _alarmActiveUnsuppressedTime += Time.deltaTime;

            float remaining = Mathf.Max(0f, secondsUntilPoliceArrive - _alarmActiveUnsuppressedTime);
            OnPoliceEtaChanged?.Invoke(remaining, secondsUntilPoliceArrive);

            if (_alarmActiveUnsuppressedTime >= secondsUntilPoliceArrive)
            {
                _policeArrived = true;
                Log($"Police ARRIVED (threshold {secondsUntilPoliceArrive:0.00}s reached).");
                OnPoliceArrived?.Invoke();
            }
        }
        else
        {
            // If alarm isn’t counting right now, still broadcast current remaining
            // (lets UI stay correct when paused/stopped/suppressed)
            float remaining = Mathf.Max(0f, secondsUntilPoliceArrive - _alarmActiveUnsuppressedTime);
            OnPoliceEtaChanged?.Invoke(remaining, secondsUntilPoliceArrive);
        }
    }

    public void TriggerAlarm(string reason = null)
    {
        if (Suppressed)
        {
            Log($"TriggerAlarm ignored (suppressed). Reason: {reason}");
            return;
        }

        if (AlarmActive)
        {
            Log($"TriggerAlarm ignored (already active). Reason: {reason}");
            return;
        }

        AlarmActive = true;
        Log($"Alarm TRIGGERED. Reason: {reason}");
        OnAlarmTriggered?.Invoke();
    }

    public void StopAlarm(string reason = null)
    {
        if (!AlarmActive)
        {
            Log($"StopAlarm ignored (already inactive). Reason: {reason}");
            return;
        }

        AlarmActive = false;
        Log($"Alarm STOPPED. Reason: {reason}");
        OnAlarmStopped?.Invoke();
    }

    public void StartSuppression(string reason = null)
    {
        _suppressedUntilTime = Time.time + suppressionDurationSeconds;

        Log($"Suppression STARTED for {suppressionDurationSeconds:0}s. Reason: {reason}");

        // Stop current alarm sound/state
        StopAlarm("Suppression started");

        if (suppressionResetsPoliceProgress)
        {
            ResetPoliceEscalationInternal("Suppression reset police progress");
        }

        OnSuppressionStarted?.Invoke(suppressionDurationSeconds);
    }

    public float GetSuppressionRemaining()
    {
        if (!Suppressed) return 0f;
        return Mathf.Max(0f, _suppressedUntilTime - Time.time);
    }

    // --- Police API (for UI / other systems) ---
    public bool PoliceArrived => _policeArrived;

    public float GetPoliceRemainingSeconds()
    {
        if (_policeArrived) return 0f;
        return Mathf.Max(0f, secondsUntilPoliceArrive - _alarmActiveUnsuppressedTime);
    }

    public float GetPoliceTotalSeconds() => secondsUntilPoliceArrive;

    public void ResetPoliceEscalation(string reason = null)
    {
        ResetPoliceEscalationInternal(reason);
    }

    private void ResetPoliceEscalationInternal(string reason)
    {
        _alarmActiveUnsuppressedTime = 0f;
        _policeArrived = false;

        Log($"Police escalation RESET. Reason: {reason}");
        OnPoliceEscalationReset?.Invoke();

        // push updated ETA immediately
        OnPoliceEtaChanged?.Invoke(secondsUntilPoliceArrive, secondsUntilPoliceArrive);
    }

    private void Log(string msg)
    {
        if (!debugLogs) return;
        Debug.Log($"{debugTag} {msg}", this);
    }
}
