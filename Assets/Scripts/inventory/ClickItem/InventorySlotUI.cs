using Assets.HeroEditor.FantasyInventory.Scripts.Interface.Elements;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class InventorySlotUI : MonoBehaviour
{
    public InventoryItem1 itemData;
    public Image iconImage;
    public TextMeshProUGUI quantityText;

    public void Init(InventoryItem1 data)
    {
        itemData = data;

        if (data.stats == null)
        {
            Debug.LogError("❌ ItemStats (stats) bị null.");
            return;
        }

        if (iconImage != null && data.stats.Icon != null)
        {
            iconImage.sprite = data.stats.Icon;
            iconImage.color = Color.white;
        }
        if (quantityText != null)
            quantityText.text = data.quantity > 1 ? data.quantity.ToString() : "";
    }





    public void OnClick()
    {
        if (itemData != null && itemData.stats != null)
        {
            ItemDetailsUI.Instance.Show(itemData);
        }
    }

}
