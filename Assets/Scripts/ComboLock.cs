using UnityEngine;

public class ComboLock : MonoBehaviour
{
    [Header("Dials")]
    public DialRotator dial1;
    public DialRotator dial2;
    public DialRotator dial3;
    public DialRotator dial4;

    [Header("Correct Combination")]
    public int[] correctCombination = { 1, 2, 3, 4 };

    [Header("Latch Movement")]
    public Transform latch;       // Assign latch object
    public float openHeight = 1f; // How far to move up in Y
    public float openSpeed = 2f;  // Smooth move speed

    private bool isOpen = false;
    private Vector3 closedPos;
    private Vector3 targetPos;

    void Start()
    {
        if (latch != null)
        {
            closedPos = latch.localPosition;
            targetPos = closedPos;
        }
    }

    void Update()
    {
        if (!isOpen && CheckCombination())
        {
            OpenLatch();
        }

        // Smooth movement
        if (latch != null)
        {
            latch.localPosition = Vector3.Lerp(latch.localPosition, targetPos, Time.deltaTime * openSpeed);
        }
    }

    private bool CheckCombination()
    {
        return (dial1.GetValue() == correctCombination[0] &&
                dial2.GetValue() == correctCombination[1] &&
                dial3.GetValue() == correctCombination[2] &&
                dial4.GetValue() == correctCombination[3]);
    }

    private void OpenLatch()
    {
        isOpen = true;
        targetPos = closedPos + new Vector3(0f, openHeight, 0f);
        Debug.Log("Lock opened! Latch moving up.");
    }
}
