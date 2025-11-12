using UnityEngine;
using TMPro;

public class PoliceCarMover : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private GameObject policeCar;   // Police Car object
    [SerializeField] private Transform carMarker;    // CarMarker target
    [SerializeField] private TextMeshProUGUI timerText; // Timer text

    [Header("Settings")]
    [SerializeField] private float moveSpeed = 5f; // Move speed

    private bool shouldMove = false;

    private void Update()
    {
        float timeRemaining = GetTimeFromText();

        if (timeRemaining <= 0 && !shouldMove)
        {
            shouldMove = true; // Trigger movement when timer hits 0
        }

        if (shouldMove && policeCar != null && carMarker != null)
        {
            Vector3 targetPosition = new Vector3(
                carMarker.position.x,
                policeCar.transform.position.y, // keep Y
                carMarker.position.z
            );

            policeCar.transform.position = Vector3.MoveTowards(
                policeCar.transform.position,
                targetPosition,
                moveSpeed * Time.deltaTime
            );

            if (Vector3.Distance(policeCar.transform.position, targetPosition) < 0.1f)
            {
                shouldMove = false; // stop when reached
            }
        }
    }

    // Helper: correctly parse "mm:ss" or "ss" formats
    private float GetTimeFromText()
    {
        string text = timerText.text;

        if (text.Contains(":"))
        {
            string[] parts = text.Split(':');
            if (parts.Length == 2 &&
                int.TryParse(parts[0], out int minutes) &&
                int.TryParse(parts[1], out int seconds))
            {
                return minutes * 60 + seconds;
            }
        }
        else if (float.TryParse(text, out float result))
        {
            return result;
        }

        return 0f; // fallback
    }
}
