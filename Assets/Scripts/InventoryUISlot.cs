using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class InventoryUISlot : MonoBehaviour
{
    [SerializeField] private Image iconImage;
    [SerializeField] private TextMeshProUGUI quantityText;

    public void Bind(InventoryEntry entry)
    {
        if (entry == null || entry.itemData == null)
            return;

        if (iconImage != null)
            iconImage.sprite = entry.itemData.icon;

        if (quantityText != null)
        {
            bool showQuantity = entry.itemData.stackable && entry.quantity > 1;
            quantityText.gameObject.SetActive(showQuantity);
            quantityText.text = entry.quantity.ToString();
        }
    }
}