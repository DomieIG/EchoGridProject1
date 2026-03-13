using UnityEngine;

public class ItemPickup : MonoBehaviour, IInteractable
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

        return $"{pickupPrompt} {itemData.displayName}";
    }

    public void Interact()
    {
        if (itemData == null)
        {
            Debug.LogWarning($"ItemPickup on '{gameObject.name}' is missing itemData.");
            return;
        }

        if (InventoryManager.Instance == null)
        {
            Debug.LogWarning("ItemPickup: No InventoryManager found in scene.");
            return;
        }

        bool added = InventoryManager.Instance.AddItem(itemData, amount);

        if (!added)
        {
            Debug.Log($"ItemPickup: Could not add '{itemData.itemId}'.");
            return;
        }

        if (!string.IsNullOrWhiteSpace(objectiveStepId) && ObjectiveManager.Instance != null)
        {
            ObjectiveManager.Instance.CompleteStep(objectiveStepId);
        }

        if (destroyOnPickup)
        {
            Destroy(gameObject);
        }
    }
}