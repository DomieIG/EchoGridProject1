using System;
using System.Collections.Generic;
using UnityEngine;

public class InventoryManager : MonoBehaviour
{
    public static InventoryManager Instance { get; private set; }

    public event Action OnInventoryChanged;

    [Header("Runtime Inventory")]
    [SerializeField] private List<InventoryEntry> items = new List<InventoryEntry>();

    private readonly Dictionary<string, InventoryEntry> itemLookup = new Dictionary<string, InventoryEntry>();

    public IReadOnlyList<InventoryEntry> Items => items;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        RebuildLookup();
    }

    private void RebuildLookup()
    {
        itemLookup.Clear();

        for (int i = 0; i < items.Count; i++)
        {
            InventoryEntry entry = items[i];
            if (entry == null || entry.itemData == null || string.IsNullOrWhiteSpace(entry.itemData.itemId))
                continue;

            string id = entry.itemData.itemId;

            if (!itemLookup.ContainsKey(id))
                itemLookup.Add(id, entry);
        }
    }

    public bool HasItem(string itemId, int amount = 1)
    {
        if (string.IsNullOrWhiteSpace(itemId))
            return false;

        return itemLookup.TryGetValue(itemId, out InventoryEntry entry) && entry.quantity >= amount;
    }

    public int GetQuantity(string itemId)
    {
        if (string.IsNullOrWhiteSpace(itemId))
            return 0;

        return itemLookup.TryGetValue(itemId, out InventoryEntry entry) ? entry.quantity : 0;
    }

    public bool AddItem(InventoryItemData itemData, int amount = 1)
    {
        if (itemData == null)
        {
            Debug.LogWarning("InventoryManager: Tried to add a null item.");
            return false;
        }

        if (string.IsNullOrWhiteSpace(itemData.itemId))
        {
            Debug.LogWarning($"InventoryManager: Item '{itemData.name}' has no itemId.");
            return false;
        }

        if (amount <= 0)
            return false;

        if (itemLookup.TryGetValue(itemData.itemId, out InventoryEntry existingEntry))
        {
            if (!itemData.stackable)
            {
                return false;
            }

            existingEntry.quantity = Mathf.Clamp(existingEntry.quantity + amount, 0, itemData.maxStack);
            NotifyInventoryChanged();
            return true;
        }

        int startAmount = itemData.stackable ? Mathf.Clamp(amount, 1, itemData.maxStack) : 1;
        InventoryEntry newEntry = new InventoryEntry(itemData, startAmount);

        items.Add(newEntry);
        itemLookup.Add(itemData.itemId, newEntry);

        NotifyInventoryChanged();
        return true;
    }

    public bool RemoveItem(string itemId, int amount = 1)
    {
        if (string.IsNullOrWhiteSpace(itemId) || amount <= 0)
            return false;

        if (!itemLookup.TryGetValue(itemId, out InventoryEntry entry))
            return false;

        entry.quantity -= amount;

        if (entry.quantity <= 0)
        {
            items.Remove(entry);
            itemLookup.Remove(itemId);
        }

        NotifyInventoryChanged();
        return true;
    }

    public bool ConsumeItem(string itemId, int amount = 1)
    {
        return RemoveItem(itemId, amount);
    }

    public void ClearInventory()
    {
        items.Clear();
        itemLookup.Clear();
        NotifyInventoryChanged();
    }

    private void NotifyInventoryChanged()
    {
        OnInventoryChanged?.Invoke();
    }
}