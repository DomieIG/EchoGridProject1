using TMPro;
using UnityEngine;

public class AlarmPoliceTimerUI : MonoBehaviour
{
    [SerializeField] private TMP_Text timerText;

    private void OnEnable()
    {
        if (AlarmSystem.Instance == null) return;
        AlarmSystem.Instance.OnPoliceEtaChanged += HandleEta;
    }

    private void OnDisable()
    {
        if (AlarmSystem.Instance == null) return;
        AlarmSystem.Instance.OnPoliceEtaChanged -= HandleEta;
    }

    private void HandleEta(float remaining, float total)
    {
        if (!timerText) return;
        timerText.text = $"POLICE ETA: {Mathf.CeilToInt(remaining)}s";
    }
}
