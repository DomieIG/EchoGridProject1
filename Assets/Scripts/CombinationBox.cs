using UnityEngine;
using UnityEngine.InputSystem;
using System.Linq;

public class CombinationBox : MonoBehaviour
{
    [Header("Dials")]
    [SerializeField] private Transform dial1;
    [SerializeField] private Transform dial2;
    [SerializeField] private Transform dial3;
    [SerializeField, Tooltip("Each dial has 9 numbers, values 0–8")]
    private int[] correctCombination = { 3, 5, 7 };
    [SerializeField, Range(1f, 360f)]
    private float dialStepAngle = 40f; // 360 / 9
    [SerializeField, Tooltip("1 = clockwise, -1 = counter-clockwise")]
    private int dialRotationDirection = 1;

    [Header("Latch")]
    [SerializeField] private Transform latchObject;
    [SerializeField] private Transform latchPivot;
    [SerializeField] private float latchOpenAngle = -45f;
    [SerializeField] private float latchOpenSpeed = 3f;

    [Header("Box Lid")]
    [SerializeField] private Transform boxTopObject;
    [SerializeField] private Transform boxTopHinge;
    [SerializeField] private float boxTopOpenAngle = -90f;
    [SerializeField] private float boxTopOpenSpeed = 2f;
    [SerializeField, Tooltip("Automatically open the box after the latch swings up")]
    private bool autoOpenBox = false;

    [Header("Box Lid Slight Open")]
    [SerializeField, Tooltip("Angle to slightly open the box when combination is solved")]
    private float boxTopSlightOpenAngle = -15f;

    // State
    private readonly int[] currentCombination = { 0, 0, 0 };
    private bool latchUnlocked = false;
    private bool boxOpen = false;
    private bool boxSlightlyOpen = false;

    private Quaternion latchClosedRot;
    private Quaternion latchOpenRot;
    private Quaternion boxClosedRot;
    private Quaternion boxOpenRot;
    private Quaternion boxSlightlyOpenRot;

    private void Start()
    {
        CacheRotations();
    }

    private void Update()
    {
        HandleClick();
        AnimateLatch();
        AnimateBox();
    }

    #region Initialization
    private void CacheRotations()
    {
        if (latchPivot != null)
        {
            latchClosedRot = latchPivot.localRotation;
            latchOpenRot = latchClosedRot * Quaternion.Euler(0, latchOpenAngle, 0);
        }

        if (boxTopHinge != null)
        {
            boxClosedRot = boxTopHinge.localRotation;
            boxOpenRot = boxClosedRot * Quaternion.Euler(boxTopOpenAngle, 0, 0);
            boxSlightlyOpenRot = boxClosedRot * Quaternion.Euler(boxTopSlightOpenAngle, 0, 0);
        }
    }
    #endregion

    #region Input
    private void HandleClick()
    {
        if (Mouse.current == null || Camera.main == null) return;
        if (!Mouse.current.leftButton.wasPressedThisFrame) return;

        Ray ray = Camera.main.ScreenPointToRay(Mouse.current.position.ReadValue());
        if (!Physics.Raycast(ray, out RaycastHit hit)) return;

        if (hit.transform == dial1) RotateDial(0, dial1);
        else if (hit.transform == dial2) RotateDial(1, dial2);
        else if (hit.transform == dial3) RotateDial(2, dial3);
        else if (latchUnlocked && !boxOpen && !autoOpenBox &&
                 (hit.transform == latchObject || hit.transform == boxTopObject))
        {
            OnOpenBoxClicked();
        }
    }
    #endregion

    #region Dial
    private void RotateDial(int index, Transform dial)
    {
        if (dial == null) return;

        currentCombination[index] = (currentCombination[index] + 1) % 9;
        float angle = currentCombination[index] * dialStepAngle * dialRotationDirection;
        dial.localRotation = Quaternion.Euler(angle, 0, 0);

        CheckCombination();
    }

    private void CheckCombination()
    {
        if (currentCombination.SequenceEqual(correctCombination) && !latchUnlocked)
        {
            latchUnlocked = true;
            OnUnlocked();
        }
    }

    private void OnUnlocked()
    {
        Debug.Log("✅ Combination correct, latch unlocked!");

        if (autoOpenBox)
        {
            // Wait for latch to swing, then AnimateLatch will auto-open
            return;
        }

        // Normal mode → slight open only
        boxSlightlyOpen = true;
    }
    #endregion

    #region Box Open
    private void OnOpenBoxClicked()
    {
        boxOpen = true;
        Debug.Log("📦 Box fully opening (manual click)!");
    }

    private void AnimateLatch()
    {
        if (!latchUnlocked || latchPivot == null) return;

        Quaternion target = latchOpenRot;
        latchPivot.localRotation = Quaternion.Slerp(latchPivot.localRotation, target, Time.deltaTime * latchOpenSpeed);

        // Auto open mode → trigger once latch is basically open
        if (autoOpenBox && !boxOpen && Quaternion.Angle(latchPivot.localRotation, latchOpenRot) < 1f)
        {
            boxOpen = true;
            Debug.Log("📦 Auto-open triggered after latch opened!");
        }
    }

    private void AnimateBox()
    {
        if (boxTopHinge == null) return;

        // Slight open
        if (boxSlightlyOpen && !boxOpen)
        {
            boxTopHinge.localRotation = Quaternion.Slerp(boxTopHinge.localRotation, boxSlightlyOpenRot, Time.deltaTime * boxTopOpenSpeed);
            if (Quaternion.Angle(boxTopHinge.localRotation, boxSlightlyOpenRot) < 0.1f)
            {
                boxSlightlyOpen = false;
            }
        }
        // Full open
        if (boxOpen)
        {
            boxTopHinge.localRotation = Quaternion.Slerp(boxTopHinge.localRotation, boxOpenRot, Time.deltaTime * boxTopOpenSpeed);
        }
    }
    #endregion
}
