using UnityEngine;

public class ObjectiveStepReporter : MonoBehaviour
{
    [SerializeField] private string stepId;
    [SerializeField] private bool triggerOnce = true;

    private bool hasTriggered;

    public void ReportStep()
    {
        if (hasTriggered && triggerOnce)
            return;

        if (ObjectiveManager.Instance == null)
        {
            Debug.LogWarning("ObjectiveStepReporter: No ObjectiveManager found in scene.");
            return;
        }

        ObjectiveManager.Instance.CompleteStep(stepId);
        hasTriggered = true;
    }
}