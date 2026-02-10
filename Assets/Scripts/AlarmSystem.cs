// AlarmSystem.cs
using System;
using UnityEngine;

[DisallowMultipleComponent]
[DefaultExecutionOrder(-100)] // Make Instance available early for other OnEnable subscribers.
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

    public event Action<float, float> OnPoliceEtaChanged; // remaining, total
    public event Action OnPoliceArrived;
    public event Action OnPoliceEscalationReset;

    [Header("Suppression")]
    [SerializeField, Min(0f)] private float suppressionDurationSeconds = 300f;

    [Header("Police Escalation")]
    [Tooltip("How long the alarm must be active (and not suppressed) before police arrive.")]
    [SerializeField, Min(0f)] private float secondsUntilPoliceArrive = 120f;

    [Tooltip("If true, suppression resets police escalation progress to 0. If false, suppression pauses progress.")]
    [SerializeField] private bool suppressionResetsPoliceProgress = false;

    private float _suppressedUntilTime = -1f;

    // Unsuppressed active accumulation
    private float _alarmActiveUnsuppressedTime;
    private bool _policeArrived;

    // ETA broadcast throttling (prevents spamming identical values)
    private float _lastBroadcastRemaining = float.NaN;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
    }

    private void OnDestroy()
    {
        if (Instance == this)
            Instance = null;
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

        TickPoliceEscalation();
    }

    private void TickPoliceEscalation()
    {
        if (_policeArrived)
            return;

        bool counting = AlarmActive && !Suppressed;
        if (counting)
        {
            _alarmActiveUnsuppressedTime += Time.deltaTime;

            if (_alarmActiveUnsuppressedTime >= secondsUntilPoliceArrive)
            {
                _alarmActiveUnsuppressedTime = secondsUntilPoliceArrive;
                _policeArrived = true;

                BroadcastPoliceEta(force: true);
                Log($"Police ARRIVED (threshold {secondsUntilPoliceArrive:0.00}s reached).");
                OnPoliceArrived?.Invoke();
                return;
            }
        }

        BroadcastPoliceEta(force: false);
    }

    private void BroadcastPoliceEta(bool force)
    {
        float remaining = Mathf.Max(0f, secondsUntilPoliceArrive - _alarmActiveUnsuppressedTime);

        // Only broadcast if changed meaningfully or forced.
        // (Prevents redundant UI updates when alarm is inactive.)
        if (!force && !float.IsNaN(_lastBroadcastRemaining) && Mathf.Abs(remaining - _lastBroadcastRemaining) < 0.01f)
            return;

        _lastBroadcastRemaining = remaining;
        OnPoliceEtaChanged?.Invoke(remaining, secondsUntilPoliceArrive);
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

        // Ensure ETA is updated immediately when alarm starts
        BroadcastPoliceEta(force: true);
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

        // Keep UI consistent
        BroadcastPoliceEta(force: true);
    }

    public void StartSuppression(string reason = null)
    {
        _suppressedUntilTime = Time.time + suppressionDurationSeconds;

        Log($"Suppression STARTED for {suppressionDurationSeconds:0}s. Reason: {reason}");

        StopAlarm("Suppression started");

        if (suppressionResetsPoliceProgress)
            ResetPoliceEscalationInternal("Suppression reset police progress");

        OnSuppressionStarted?.Invoke(suppressionDurationSeconds);
    }

    public float GetSuppressionRemaining()
    {
        if (!Suppressed) return 0f;
        return Mathf.Max(0f, _suppressedUntilTime - Time.time);
    }

    // --- Police API ---
    public bool PoliceArrived => _policeArrived;

    public float GetPoliceRemainingSeconds()
    {
        if (_policeArrived) return 0f;
        return Mathf.Max(0f, secondsUntilPoliceArrive - _alarmActiveUnsuppressedTime);
    }

    public float GetPoliceTotalSeconds() => secondsUntilPoliceArrive;

    public void ResetPoliceEscalation(string reason = null) => ResetPoliceEscalationInternal(reason);

    private void ResetPoliceEscalationInternal(string reason)
    {
        _alarmActiveUnsuppressedTime = 0f;
        _policeArrived = false;
        _lastBroadcastRemaining = float.NaN;

        Log($"Police escalation RESET. Reason: {reason}");
        OnPoliceEscalationReset?.Invoke();
        BroadcastPoliceEta(force: true);
    }

    private void Log(string msg)
    {
        if (!debugLogs) return;
        Debug.Log($"{debugTag} {msg}", this);
    }
}
