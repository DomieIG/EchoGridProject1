using UnityEngine;

public class PoliceCarMover_AlarmDriven : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Transform policeCar;
    [SerializeField] private Transform carMarker;

    [Header("Movement")]
    [SerializeField] private float moveSpeed = 5f;
    [SerializeField] private float stopDistance = 0.1f;

    [Header("Debug")]
    [SerializeField] private bool debugLogs = true;
    [SerializeField] private string debugTag = "[POLICE]";

    private bool _shouldMove;

    private void OnEnable()
    {
        var alarm = AlarmSystem.Instance;
        if (!alarm) return;

        alarm.OnPoliceArrived += HandlePoliceArrived;
        alarm.OnPoliceEscalationReset += HandlePoliceReset;

        // If enabled mid-game and police already arrived:
        if (alarm.PoliceArrived)
            _shouldMove = true;
    }

    private void OnDisable()
    {
        var alarm = AlarmSystem.Instance;
        if (!alarm) return;

        alarm.OnPoliceArrived -= HandlePoliceArrived;
        alarm.OnPoliceEscalationReset -= HandlePoliceReset;
    }

    private void Update()
    {
        if (!_shouldMove) return;
        if (!policeCar || !carMarker) return;

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

        if (Vector3.Distance(policeCar.position, targetPosition) <= stopDistance)
        {
            _shouldMove = false;
            Log("Police car reached marker; movement stopped.");
        }
    }

    private void HandlePoliceArrived()
    {
        _shouldMove = true;
        Log("Received PoliceArrived event; beginning movement.");
    }

    private void HandlePoliceReset()
    {
        _shouldMove = false;
        Log("Received PoliceEscalationReset event; movement stopped/reset.");
    }

    private void Log(string msg)
    {
        if (!debugLogs) return;
        Debug.Log($"{debugTag} {msg}", this);
    }
}
