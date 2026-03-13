using UnityEngine;

public class RequireInventoryItem : MonoBehaviour, IInteractable
{
    [Header("Required Item")]
    [SerializeField] private string requiredItemId;

    [Header("Prompt Text")]
    [SerializeField] private string lockedPrompt = "Locked";
    [SerializeField] private string unlockedPrompt = "Use";

    [Header("Behavior")]
    [SerializeField] private bool consumeItem = false;

    [Header("Result")]
    [SerializeField] private GameObject objectToEnable;
    [SerializeField] private GameObject objectToDisable;

    [Header("Objective")]
    [SerializeField] private string objectiveStepId;

    private bool used;

    public string GetPrompt()
    {
        if (used)
            return "";

        if (InventoryManager.Instance == null)
            return lockedPrompt;

        bool hasItem = InventoryManager.Instance.HasItem(requiredItemId);

        return hasItem ? unlockedPrompt : lockedPrompt;
    }

    public void Interact()
    {
        if (used)
            return;

        if (InventoryManager.Instance == null)
            return;

        if (!InventoryManager.Instance.HasItem(requiredItemId))
        {
            Debug.Log("Missing required item: " + requiredItemId);
            return;
        }

        if (consumeItem)
            InventoryManager.Instance.ConsumeItem(requiredItemId);

        if (objectToEnable != null)
            objectToEnable.SetActive(true);

        if (objectToDisable != null)
            objectToDisable.SetActive(false);

        if (!string.IsNullOrEmpty(objectiveStepId) && ObjectiveManager.Instance != null)
        {
            ObjectiveManager.Instance.CompleteStep(objectiveStepId);
        }

        used = true;
    }
}