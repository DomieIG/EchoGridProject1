using System;
using UnityEngine;

public class AlarmSystem : MonoBehaviour
{
    public static AlarmSystem Instance { get; private set; }

    public bool AlarmActive { get; private set; }
    public bool Suppressed => _suppressedUntilTime > Time.time;

    public event Action OnAlarmTriggered;
    public event Action OnAlarmStopped;
    public event Action<float> OnSuppressionStarted; // seconds
    public event Action OnSuppressionEnded;

    [Header("Suppression")]
    [SerializeField] private float suppressionDurationSeconds = 300f; // 5 minutes

    private float _suppressedUntilTime = -1f;

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
    }

    private void Update()
    {
        if (_suppressedUntilTime > 0f && Time.time >= _suppressedUntilTime)
        {
            _suppressedUntilTime = -1f;
            OnSuppressionEnded?.Invoke();
        }
    }

    public void TriggerAlarm()
    {
        // If suppressed, we don’t re-sound it during the suppression window
        if (Suppressed) return;

        if (AlarmActive) return;

        AlarmActive = true;
        OnAlarmTriggered?.Invoke();
    }

    public void StopAlarm()
    {
        if (!AlarmActive) return;

        AlarmActive = false;
        OnAlarmStopped?.Invoke();
    }

    public void StartSuppression()
    {
        _suppressedUntilTime = Time.time + suppressionDurationSeconds;

        // Stop current alarm sound/state, but don’t prevent future triggers beyond suppression window
        StopAlarm();

        OnSuppressionStarted?.Invoke(suppressionDurationSeconds);
    }

    public float GetSuppressionRemaining()
    {
        if (!Suppressed) return 0f;
        return Mathf.Max(0f, _suppressedUntilTime - Time.time);
    }
}
