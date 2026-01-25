using UnityEngine;

public class AlarmKeypad : MonoBehaviour
{
    [Header("Code Settings")]
    [SerializeField] private string correctCode = "3917";
    [SerializeField] private int maxDigits = 6;

    [Header("Optional")]
    [SerializeField] private bool allowWhenAlarmInactive = false;

    public int MaxDigits => maxDigits;

    // Public setter if you ever want to set it from notes/loot system at runtime
    public void SetCorrectCode(string newCode)
    {
        correctCode = newCode;
    }

    public bool TrySubmitCode(string entered)
    {
        if (AlarmSystem.Instance == null) return false;

        if (!allowWhenAlarmInactive && !AlarmSystem.Instance.AlarmActive)
            return false;

        if (entered == correctCode)
        {
            AlarmSystem.Instance.StartSuppression(); // stops alarm + suppresses 5 min
            return true;
        }

        return false;
    }
}
