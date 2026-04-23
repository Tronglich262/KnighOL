using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

/// <summary>
/// Slot UI dùng chung cho Character + Shop
/// </summary>
public class EquipmentSlotUI : MonoBehaviour, IPointerClickHandler
{
    public string itemId;
    public Image iconImage;
    public string itemType;
    public int itemPrice;
    public NpcShopItem npcShopItemData;

    [Header("Check nếu là slot của Character UI")]
    public bool isCharacterSlot;

    public enum ShopPanelType { None, ShopTP, ShopVK, ShopPK, Daily }
    public ShopPanelType shopPanelType = ShopPanelType.None;

    public void SetItem(string id, Sprite icon, string type = null, int price = 0)
    {
        itemId = id;
        itemType = type;
        itemPrice = price;
        if (iconImage != null)
        {
            iconImage.sprite = icon;
            iconImage.color = Color.white;
        }
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (string.IsNullOrEmpty(itemId)) return;

        if (isCharacterSlot)
        {
            // Slot trong inventory/character
            if (ItemDetailsPanel.Instance != null)
                ItemDetailsPanel.Instance.Show(itemId, iconImage.sprite, itemType);
        }
        else
        {
            // Slot trong Shop
            if (ItemDetailsPanel.Instance != null)
                ItemDetailsPanel.Instance.Hide();

            // Gọi panel chi tiết mới
            if (npcShopItemData != null && ShopItemDetailPanel.Instance != null)
            {
                ItemStats stats = ItemStatDatabase.Instance.GetStats(itemId)
                               ?? ItemStatDatabase.Instance.GetStatsdtb(int.Parse(itemId));

                if (stats != null)
                {
                    ShopType shopType = GetShopTypeFromPanel();
                    ShopItemDetailPanel.Instance.Show(npcShopItemData, stats, shopType);

                    // QUAN TRỌNG: Set currentShopItem cho ItemDetailsUI để nút "Mua" hoạt động
                    if (ItemDetailsUI.Instance != null)
                        ItemDetailsUI.Instance.SetCurrentShopItem(npcShopItemData);
                }
            }
        }
    }

    private ShopType GetShopTypeFromPanel()
    {
        return shopPanelType switch
        {
            ShopPanelType.ShopVK => ShopType.Weapon,
            ShopPanelType.ShopPK => ShopType.Consumable,
            ShopPanelType.ShopTP => ShopType.Accessory,
            _ => ShopType.Other
        };
    }

    public void OnSlotClicked() => OnPointerClick(null);
}