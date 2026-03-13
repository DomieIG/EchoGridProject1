using System;

[Serializable]
public class InventoryEntry
{
    public InventoryItemData itemData;
    public int quantity;

    public InventoryEntry(InventoryItemData data, int amount)
    {
        itemData = data;
        quantity = amount;
    }
}