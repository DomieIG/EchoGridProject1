// AlarmEffects.cs
using UnityEngine;

[DisallowMultipleComponent]
public class AlarmEffects : MonoBehaviour
{
    [Header("Audio")]
    [SerializeField] private AudioSource alarmLoopSource;
    [SerializeField] private AudioClip alarmLoopClip;
    [SerializeField] private bool playLoopWhenAlarmActive = true;

    [Header("Lights")]
    [SerializeField] private Light[] alarmLights;
    [SerializeField, Min(0f)] private float minIntensity = 0.2f;
    [SerializeField, Min(0f)] private float maxIntensity = 2.0f;
    [SerializeField, Min(0.01f)] private float blinkSpeed = 6f;

    [Header("Debug")]
    [SerializeField] private bool logSetupIssues = true;

    private AlarmSystem _alarm;
    private bool _subscribed;
    private bool _blinking;

    private void Awake()
    {
        if (!alarmLoopSource)
            alarmLoopSource = GetComponent<AudioSource>();

        if (alarmLoopSource)
        {
            alarmLoopSource.playOnAwake = false;
            alarmLoopSource.loop = true;

            if (alarmLoopClip)
                alarmLoopSource.clip = alarmLoopClip;
        }
    }

    private void OnEnable()
    {
        // Try immediately; if Instance isn't ready yet, Update() will pick it up.
        TrySubscribe();
        ValidateSetup();
        SyncState();
    }

    private void OnDisable()
    {
        Unsubscribe();
        StopAllEffects();
    }

    private void Update()
    {
        if (!_subscribed)
        {
            TrySubscribe();
            if (_subscribed)
            {
                ValidateSetup();
                SyncState();
            }
        }

        if (!_blinking) return;

        float t = Mathf.Abs(Mathf.Sin(Time.time * blinkSpeed));
        float intensity = Mathf.Lerp(minIntensity, maxIntensity, t);

        if (alarmLights != null)
        {
            for (int i = 0; i < alarmLights.Length; i++)
            {
                var l = alarmLights[i];
                if (l) l.intensity = intensity;
            }
        }
    }

    private void TrySubscribe()
    {
        if (_subscribed) return;

        _alarm = AlarmSystem.Instance;
        if (!_alarm) return;

        _alarm.OnAlarmTriggered += HandleAlarmOn;
        _alarm.OnAlarmStopped += HandleAlarmOff;
        _alarm.OnSuppressionStarted += HandleSuppressionStarted;
        _alarm.OnSuppressionEnded += HandleSuppressionEnded;

        _subscribed = true;
    }

    private void Unsubscribe()
    {
        if (!_subscribed || !_alarm) return;

        _alarm.OnAlarmTriggered -= HandleAlarmOn;
        _alarm.OnAlarmStopped -= HandleAlarmOff;
        _alarm.OnSuppressionStarted -= HandleSuppressionStarted;
        _alarm.OnSuppressionEnded -= HandleSuppressionEnded;

        _subscribed = false;
        _alarm = null;
    }

    private void SyncState()
    {
        if (!_alarm) return;

        if (_alarm.AlarmActive && !_alarm.Suppressed)
            HandleAlarmOn();
        else
            HandleAlarmOff();
    }

    private void HandleAlarmOn()
    {
        _blinking = true;

        if (playLoopWhenAlarmActive && alarmLoopSource && alarmLoopSource.clip && !alarmLoopSource.isPlaying)
            alarmLoopSource.Play();
    }

    private void HandleAlarmOff()
    {
        StopAllEffects();
    }

    private void HandleSuppressionStarted(float _)
    {
        StopAllEffects();
    }

    private void HandleSuppressionEnded()
    {
        // Optional: do nothing; alarm does not auto-resume after suppression in your AlarmSystem.
    }

    private void StopAllEffects()
    {
        _blinking = false;

        if (alarmLights != null)
        {
            for (int i = 0; i < alarmLights.Length; i++)
            {
                var l = alarmLights[i];
                if (l) l.intensity = minIntensity;
            }
        }

        if (alarmLoopSource && alarmLoopSource.isPlaying)
            alarmLoopSource.Stop();
    }

    private void ValidateSetup()
    {
        if (!logSetupIssues) return;

        if (!alarmLoopSource)
            Debug.LogWarning($"[{nameof(AlarmEffects)}] No AudioSource assigned/found.", this);
        else if (!alarmLoopSource.clip)
            Debug.LogWarning($"[{nameof(AlarmEffects)}] AudioSource has no clip. Assign an MP3/WAV to 'Alarm Loop Clip'.", this);

        if (alarmLights == null || alarmLights.Length == 0)
            Debug.LogWarning($"[{nameof(AlarmEffects)}] No alarmLights assigned.", this);
    }
}
