using UnityEngine;

[CreateAssetMenu(menuName = "Bank Game/Inventory/Item Data", fileName = "Item_")]
public class InventoryItemData : ScriptableObject
{
    [Header("Identity")]
    public string itemId;
    public string displayName;

    [Header("Visuals")]
    public Sprite icon;

    [Header("Behavior")]
    public bool stackable = false;
    [Min(1)] public int maxStack = 1;
    public bool showInHud = true;
}