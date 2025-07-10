using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

/// <summary>
/// Script gắn cho từng slot hiển thị trang bị, phân biệt Character/Shop.
/// </summary>
public class EquipmentSlotUI : MonoBehaviour, IPointerClickHandler
{
    public string itemId;
    public Image iconImage;
    public string itemType;

    [Header("Check nếu là slot của Character UI")]
    public bool isCharacterSlot; // Gán đúng ở Inspector!

    public enum ShopPanelType { None, ShopItem, ShopVK, ShopPK, Daily}
    [Header("Phân biệt panel shop (nếu dùng chung prefab slot)")]

    public ShopPanelType shopPanelType = ShopPanelType.None;

    public void SetItem(string id, Sprite icon, string type = null)
    {
        itemId = id;
        itemType = type;
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
            HideAllShopPanels();
            if (ItemDetailsPanel.Instance != null)
            {
                ItemDetailsPanel.Instance.Show(itemId, iconImage.sprite, itemType);
            }
        }
        else
        {
            if (ItemDetailsPanel.Instance != null) ItemDetailsPanel.Instance.Hide();

            switch (shopPanelType)
            {
                case ShopPanelType.ShopItem:
                    ToggleShopPanel(shopitem.Instance, itemId, iconImage.sprite, itemType);
                    break;
                case ShopPanelType.ShopVK:
                    ToggleShopVKPanel(shopvk.Instance, itemId, iconImage.sprite, itemType);
                    break;
                case ShopPanelType.ShopPK:
                    ToggleShopPKPanel(shoppk.Instance, itemId, iconImage.sprite, itemType);
                    break;
                case ShopPanelType.Daily:
                    ToggleDailyPanel(CanvasShop.Instante.canvasDaily, itemId, iconImage.sprite, itemType);
                    break;
                default:
                    Debug.LogWarning("Chưa gán đúng ShopPanelType cho slot!");
                    break;
            }
        }
    }

    private void HideAllShopPanels()
    {
        if (shopitem.Instance != null) shopitem.Instance.Hide();
        if (shopvk.Instance != null) shopvk.Instance.Hide();
        if (shoppk.Instance != null) shoppk.Instance.Hide();
        if (CanvasShop.Instante.canvasDaily != null) CanvasShop.Instante.canvasDaily.SetActive(false);

    }

    private void ToggleShopPanel(shopitem shopPanel, string id, Sprite icon, string type)
    {
        if (shopPanel == null) return;
        if (ItemDetailsPanel.Instance != null) ItemDetailsPanel.Instance.Hide();

        if (shopPanel.IsVisible() && shopPanel.IsShowingItem(id))
        {
            shopPanel.Hide();
        }
        else
        {
            shopPanel.Show(id, icon, type);
            var buyButton = FindFirstObjectByType<BuyButton>();
            if (buyButton != null)
            {
                buyButton.SetSelectedSlot(this);
            }
            var itemDetailsUI = ItemDetailsUI.Instance;
            if (itemDetailsUI != null)
            {
                itemDetailsUI.SetCurrentItemId(id, icon, type);
            }
        }
    }
    private void ToggleShopVKPanel(shopvk shopPanel, string id, Sprite icon, string type)
    {
        if (shopPanel == null) return;
        if (ItemDetailsPanel.Instance != null) ItemDetailsPanel.Instance.Hide();

        if (shopPanel.IsVisible1() && shopPanel.IsShowingItem(id))
        {
            shopPanel.Hide();
        }
        else
        {
            shopPanel.Show(id, icon, type);
            var buyButton = FindFirstObjectByType<BuyButton>();
            if (buyButton != null)
            {
                buyButton.SetSelectedSlot(this);
            }
            var itemDetailsUI = ItemDetailsUI.Instance;
            if (itemDetailsUI != null)
            {
                itemDetailsUI.SetCurrentItemId(id, icon, type);
            }
        }
    }
    private void ToggleShopPKPanel(shoppk shopPanel, string id, Sprite icon, string type)
    {
        if (shopPanel == null) return;
        if (ItemDetailsPanel.Instance != null) ItemDetailsPanel.Instance.Hide();

        if (shopPanel.IsVisible2() && shopPanel.IsShowingItem(id))
        {
            shopPanel.Hide();
        }
        else
        {
            shopPanel.Show(id, icon, type);
            var buyButton = FindFirstObjectByType<BuyButton>();
            if (buyButton != null)
            {
                buyButton.SetSelectedSlot(this);
            }
            var itemDetailsUI = ItemDetailsUI.Instance;
            if (itemDetailsUI != null)
            {
                itemDetailsUI.SetCurrentItemId(id, icon, type);
            }
        }
    }
    private void ToggleDailyPanel(GameObject dailyPanel, string id, Sprite icon, string type)
    {
        if (dailyPanel == null) return;

        if (!dailyPanel.activeSelf)
            dailyPanel.SetActive(true); // Chỉ mở panel nếu đang tắt

        // Khi panel đã mở, chỉ cập nhật thông tin item
        var itemDetailsUI = ItemDetailsUI.Instance;
        if (itemDetailsUI != null)
        {
            itemDetailsUI.SetCurrentItemId(id, icon, type);
        }
    }
}
