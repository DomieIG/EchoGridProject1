using UnityEngine;
using TMPro;
using UnityEngine.Events;

public class CameraLightTriggerTimer : MonoBehaviour
{
    [Header("Timer Settings")]
    [SerializeField, Tooltip("Countdown time in minutes.")]
    private float countdownMinutes = 1f;

    [SerializeField, Tooltip("UI text object for displaying the timer.")]
    private TextMeshProUGUI timerText;

    [Header("Timer Behavior")]
    [SerializeField, Tooltip("Automatically restart when the player re-enters the trigger.")]
    private bool resetOnTrigger = true;

    [SerializeField, Tooltip("Hide timer text once finished.")]
    private bool hideOnFinish = false;

    [SerializeField, Tooltip("Enable debug logging in Console.")]
    private bool debugLogs = true;

    [Header("Events")]
    public UnityEvent onTimerFinished;

    private float remainingTime;
    private bool timerRunning = false;

    private void Start()
    {
        remainingTime = countdownMinutes * 60f;
        UpdateTimerDisplay();

        if (timerText != null)
            timerText.gameObject.SetActive(false); // hide until triggered
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player")) return;

        if (debugLogs) Debug.Log($"Trigger entered by: {other.name}");

        // Reset or ignore based on flag
        if (!timerRunning || resetOnTrigger)
        {
            remainingTime = countdownMinutes * 60f;
            timerRunning = true;

            if (timerText != null)
                timerText.gameObject.SetActive(true);

            if (debugLogs) Debug.Log("Timer started!");
        }
    }

    private void Update()
    {
        if (!timerRunning) return;

        if (remainingTime > 0)
        {
            remainingTime = Mathf.Max(0f, remainingTime - Time.deltaTime);
            UpdateTimerDisplay();
        }
        else
        {
            timerRunning = false;
            UpdateTimerDisplay();

            if (debugLogs) Debug.Log("Timer finished!");

            onTimerFinished?.Invoke();

            if (hideOnFinish && timerText != null)
                timerText.gameObject.SetActive(false);
        }
    }

    private void UpdateTimerDisplay()
    {
        if (timerText == null) return;

        int minutes = Mathf.FloorToInt(remainingTime / 60);
        int seconds = Mathf.FloorToInt(remainingTime % 60);
        timerText.text = $"{minutes:00}:{seconds:00}";
    }
}
