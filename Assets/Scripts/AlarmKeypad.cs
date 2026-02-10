// AlarmKeypad.cs
using UnityEngine;

[DisallowMultipleComponent]
public class AlarmKeypad : MonoBehaviour
{
    [Header("Code Settings")]
    [SerializeField] private string correctCode = "3917";
    [SerializeField] private bool allowWhenAlarmInactive = false;

    [Header("Debug")]
    [SerializeField] private bool debugLogs = true;
    [SerializeField] private string debugTag = "[KEYPAD]";

    /// <summary>
    /// Single source of truth for whether the keypad is allowed to be used right now.
    /// Disabled during suppression.
    /// </summary>
    public bool CanInteractNow
    {
        get
        {
            var alarm = AlarmSystem.Instance;
            if (alarm == null) return false;
            if (alarm.Suppressed) return false;
            return allowWhenAlarmInactive || alarm.AlarmActive;
        }
    }

    public bool TrySubmitCode(string entered)
    {
        var alarm = AlarmSystem.Instance;
        if (alarm == null)
        {
            Log("TrySubmitCode failed: AlarmSystem.Instance is null.");
            return false;
        }

        if (alarm.Suppressed)
        {
            Log("TrySubmitCode blocked: alarm is suppressed.");
            return false;
        }

        if (!allowWhenAlarmInactive && !alarm.AlarmActive)
        {
            Log($"TrySubmitCode blocked: alarm inactive (allowWhenAlarmInactive={allowWhenAlarmInactive}).");
            return false;
        }

        if (entered == correctCode)
        {
            Log("Correct code entered. Starting suppression.");
            alarm.StartSuppression("Keypad code accepted");
            return true;
        }

        Log("Wrong code entered.");
        return false;
    }

    private void Log(string msg)
    {
        if (!debugLogs) return;
        Debug.Log($"{debugTag} {msg}", this);
    }
}
