using UnityEngine;

public class AlarmEffects : MonoBehaviour
{
    [SerializeField] private AudioSource alarmLoopSource;
    [SerializeField] private Light[] alarmLights;
    [SerializeField] private float blinkSpeed = 6f;

    private bool _blinking;

    private void OnEnable()
    {
        AlarmSystem.Instance.OnAlarmTriggered += HandleAlarmOn;
        AlarmSystem.Instance.OnAlarmStopped += HandleAlarmOff;
        AlarmSystem.Instance.OnSuppressionEnded += HandleSuppressionEnded;
    }

    private void OnDisable()
    {
        if (!AlarmSystem.Instance) return;
        AlarmSystem.Instance.OnAlarmTriggered -= HandleAlarmOn;
        AlarmSystem.Instance.OnAlarmStopped -= HandleAlarmOff;
        AlarmSystem.Instance.OnSuppressionEnded -= HandleSuppressionEnded;
    }

    private void Update()
    {
        if (!_blinking) return;

        float t = Mathf.Abs(Mathf.Sin(Time.time * blinkSpeed));
        foreach (var l in alarmLights)
            if (l) l.intensity = Mathf.Lerp(0.2f, 2f, t);
    }

    private void HandleAlarmOn()
    {
        _blinking = true;
        if (alarmLoopSource && !alarmLoopSource.isPlaying) alarmLoopSource.Play();
    }

    private void HandleAlarmOff()
    {
        _blinking = false;
        foreach (var l in alarmLights)
            if (l) l.intensity = 0.2f;

        if (alarmLoopSource && alarmLoopSource.isPlaying) alarmLoopSource.Stop();
    }

    private void HandleSuppressionEnded()
    {
        // If something triggers alarm after suppression ends, AlarmSystem.TriggerAlarm will handle it
        // You can optionally do a “suppression ended” beep here.
    }
}
