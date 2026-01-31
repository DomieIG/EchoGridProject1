using UnityEngine;

public class AlarmKeypad : MonoBehaviour
{
    [Header("Code Settings")]
    [SerializeField] private string correctCode = "3917";
    [SerializeField] private bool allowWhenAlarmInactive = false;

    [Header("Debug")]
    [SerializeField] private bool debugLogs = true;
    [SerializeField] private string debugTag = "[KEYPAD]";

    public bool TrySubmitCode(string entered)
    {
        if (AlarmSystem.Instance == null)
        {
            Log("TrySubmitCode failed: AlarmSystem.Instance is null.");
            return false;
        }

        if (!allowWhenAlarmInactive && !AlarmSystem.Instance.AlarmActive)
        {
            Log($"TrySubmitCode blocked: alarm inactive (allowWhenAlarmInactive={allowWhenAlarmInactive}).");
            return false;
        }

        if (entered == correctCode)
        {
            Log("Correct code entered. Starting suppression.");
            AlarmSystem.Instance.StartSuppression("Keypad code accepted");
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
