using UnityEngine;

public class CameraArcController : MonoBehaviour
{
    [Header("Rotation Pivot")]
    [Tooltip("The pivot object that actually rotates (e.g., CameraRotate).")]
    public Transform rotationPivot;

    [Header("Rotation Control")]
    public bool allowRotation = true;                 // Master toggle
    public bool autoSweep = true;                     // If false, camera stays fixed
    [Range(1f, 180f)] public float rotationSpeed = 30f; // Sweep speed in degrees/sec

    [Header("Arc Limits")]
    [Range(-180f, 0f)] public float leftLimit = -45f;   // Leftmost angle
    [Range(0f, 180f)] public float rightLimit = 45f;    // Rightmost angle

    [Header("Follow Player Settings")]
    public float followDuration = 2f;    // How long to follow the player
    [Range(0.1f, 20f)] public float followSpeed = 5f; // Lerp speed for follow

    private bool isFollowingPlayer = false;
    private float followTimer = 0f;
    private Transform playerTransform;

    private float sweepTime; // Time accumulator for smooth sweep

    void Start()
    {
        if (rotationPivot == null)
        {
            Debug.LogError($"{nameof(CameraArcController)}: rotationPivot not assigned!");
            enabled = false;
            return;
        }

        // Start at midpoint of the arc
        float mid = (leftLimit + rightLimit) / 2f;
        rotationPivot.rotation = Quaternion.Euler(0f, mid, 0f);
    }

    void Update()
    {
        if (!allowRotation || rotationPivot == null) return;

        if (isFollowingPlayer && playerTransform != null)
        {
            FollowPlayer();
        }
        else if (autoSweep)
        {
            SweepBetweenLimits();
        }
    }

    void SweepBetweenLimits()
    {
        float arcWidth = Mathf.Abs(rightLimit - leftLimit);
        if (arcWidth < 0.1f) return;

        // Calculate a normalized t that sweeps 0𢴎 at a speed in degrees/sec
        sweepTime += Time.deltaTime * (rotationSpeed / arcWidth);
        float t = Mathf.PingPong(sweepTime, 1f);

        float angle = Mathf.Lerp(leftLimit, rightLimit, t);
        rotationPivot.rotation = Quaternion.Euler(0f, angle, 0f);
    }


    void FollowPlayer()
    {
        followTimer -= Time.deltaTime;
        if (followTimer <= 0f)
        {
            isFollowingPlayer = false;
            return;
        }

        if (playerTransform == null) return;

        Vector3 direction = playerTransform.position - rotationPivot.position;
        direction.y = 0f; // keep flat

        if (direction.sqrMagnitude > 0.01f)
        {
            Quaternion targetRot = Quaternion.LookRotation(direction, Vector3.up);
            rotationPivot.rotation = Quaternion.Slerp(
                rotationPivot.rotation,
                targetRot,
                followSpeed * Time.deltaTime
            );
        }
    }

    public void TriggerFollow(Transform target)
    {
        if (target == null) return;
        playerTransform = target;
        isFollowingPlayer = true;
        followTimer = followDuration;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            TriggerFollow(other.transform);
            Debug.Log("CameraArcController: Player entered � switching to follow mode!");
        }
    }
}
