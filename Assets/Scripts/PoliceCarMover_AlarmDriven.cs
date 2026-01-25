using UnityEngine;

public class PoliceCarMover_AlarmDriven : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Transform policeCar;
    [SerializeField] private Transform carMarker;

    [Header("Escalation")]
    [Tooltip("How long the alarm must be active (NOT suppressed) before police arrive.")]
    [SerializeField] private float secondsUntilPoliceArrive = 120f;

    [Header("Movement")]
    [SerializeField] private float moveSpeed = 5f;

    private float alarmActiveUnsuppressedTime;
    private bool shouldMove;

    private void Update()
    {
        var alarm = AlarmSystem.Instance;
        if (!alarm) return;

        // Count only when alarm is actually sounding (active) AND not suppressed
        if (alarm.AlarmActive && !alarm.Suppressed)
        {
            alarmActiveUnsuppressedTime += Time.deltaTime;

            if (!shouldMove && alarmActiveUnsuppressedTime >= secondsUntilPoliceArrive)
                shouldMove = true;
        }

        // If you want suppression to "buy time", do nothing while suppressed.
        // If you want suppression to fully reset progress, uncomment below:
        // if (alarm.Suppressed) alarmActiveUnsuppressedTime = 0f;

        if (shouldMove && policeCar && carMarker)
        {
            Vector3 targetPosition = new Vector3(
                carMarker.position.x,
                policeCar.position.y,
                carMarker.position.z
            );

            policeCar.position = Vector3.MoveTowards(
                policeCar.position,
                targetPosition,
                moveSpeed * Time.deltaTime
            );

            if (Vector3.Distance(policeCar.position, targetPosition) < 0.1f)
                shouldMove = false;
        }
    }

    // Optional: call this if you ever want to reset police progress (new attempt / restart)
    public void ResetPoliceResponse()
    {
        alarmActiveUnsuppressedTime = 0f;
        shouldMove = false;
    }
}
