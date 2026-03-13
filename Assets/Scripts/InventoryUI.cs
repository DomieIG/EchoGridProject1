using System.Collections.Generic;
using UnityEngine;

public class InventoryUI : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Transform slotContainer;
    [SerializeField] private InventoryUISlot slotPrefab;

    private readonly List<InventoryUISlot> spawnedSlots = new List<InventoryUISlot>();

    private void OnEnable()
    {
        if (InventoryManager.Instance != null)
            InventoryManager.Instance.OnInventoryChanged += Refresh;
    }

    private void Start()
    {
        Refresh();
    }

    private void OnDisable()
    {
        if (InventoryManager.Instance != null)
            InventoryManager.Instance.OnInventoryChanged -= Refresh;
    }

    public void Refresh()
    {
        ClearSlots();

        if (InventoryManager.Instance == null || slotContainer == null || slotPrefab == null)
            return;

        IReadOnlyList<InventoryEntry> items = InventoryManager.Instance.Items;

        for (int i = 0; i < items.Count; i++)
        {
            InventoryEntry entry = items[i];

            if (entry == null || entry.itemData == null || !entry.itemData.showInHud)
                continue;

            InventoryUISlot slot = Instantiate(slotPrefab, slotContainer);
            slot.Bind(entry);
            spawnedSlots.Add(slot);
        }
    }

    private void ClearSlots()
    {
        for (int i = 0; i < spawnedSlots.Count; i++)
        {
            if (spawnedSlots[i] != null)
                Destroy(spawnedSlots[i].gameObject);
        }

        spawnedSlots.Clear();
    }
}