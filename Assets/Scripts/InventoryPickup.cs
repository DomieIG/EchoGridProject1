using UnityEngine;

public class InventoryPickup : MonoBehaviour, IInteractable
{
    [Header("Item")]
    [SerializeField] private InventoryItemData itemData;
    [SerializeField] private int amount = 1;

    [Header("Prompt")]
    [SerializeField] private string pickupPrompt = "Pick Up";

    [Header("Behavior")]
    [SerializeField] private bool destroyOnPickup = true;

    [Header("Optional Objective Step")]
    [SerializeField] private string objectiveStepId;

    public string GetPrompt()
    {
        if (itemData == null)
            return pickupPrompt;

        return pickupPrompt + " " + itemData.displayName;
    }

    public void Interact()
    {
        if (itemData == null)
        {
            Debug.LogWarning("InventoryPickup missing item data.");
            return;
        }

        if (InventoryManager.Instance == null)
        {
            Debug.LogWarning("No InventoryManager found.");
            return;
        }

        bool added = InventoryManager.Instance.AddItem(itemData, amount);

        if (!added)
            return;

        if (!string.IsNullOrEmpty(objectiveStepId) && ObjectiveManager.Instance != null)
        {
            ObjectiveManager.Instance.CompleteStep(objectiveStepId);
        }

        if (destroyOnPickup)
            Destroy(gameObject);
    }
}