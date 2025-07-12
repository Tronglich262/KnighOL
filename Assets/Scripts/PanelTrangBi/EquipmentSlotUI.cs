using System.Collections;
using Unity.VisualScripting;
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
    public int itemPrice;

    [Header("Check nếu là slot của Character UI")]
    public bool isCharacterSlot; // Gán đúng ở Inspector!

    public enum ShopPanelType { None, ShopTP, ShopVK, ShopPK, Daily}
    [Header("Phân biệt panel shop (nếu dùng chung prefab slot)")]

    public ShopPanelType shopPanelType = ShopPanelType.None;

    public void SetItem(string id, Sprite icon, string type = null, int price = 0)
    {
        itemId = id;
        itemType = type;
        itemPrice = price; // <-- Lưu giá ở đây
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
                case ShopPanelType.ShopTP:
                    ToggleShoptpPanel(ShopTP.Instance, itemId, iconImage.sprite, itemType);
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
        if (ShopTP.Instance != null) ShopTP.Instance.Hide();
        if (shopvk.Instance != null) shopvk.Instance.Hide();
        if (shoppk.Instance != null) shoppk.Instance.Hide();
        if (CanvasShop.Instante.canvasDaily != null) CanvasShop.Instante.canvasDaily.SetActive(false);

    }
    public void OnSlotClicked()
    {
        if (string.IsNullOrEmpty(itemId)) return;
        switch (shopPanelType)
        {
              case ShopPanelType.ShopTP:
                ShopTP.Instance.Show(itemId, iconImage.sprite, itemType, itemPrice);
                break;
              case ShopPanelType.ShopVK:
                shopvk.Instance.Show(itemId, iconImage.sprite, itemType, itemPrice);
                break;
              case ShopPanelType.ShopPK:
                shoppk.Instance.Show(itemId, iconImage.sprite, itemType, itemPrice);
                break;
        }
        Debug.Log($"Slot clicked: {itemId}, Type: {itemType}");
    }
    private void ToggleShoptpPanel(ShopTP shopPanel, string id, Sprite icon, string type)
    {
        if (shopPanel == null)
        {
            Debug.LogError("ShopTP panel is null");
            return;
        }
        if (ItemDetailsPanel.Instance != null) ItemDetailsPanel.Instance.Hide();

        // LUÔN show, không toggle hide nữa!
        shopPanel.Show(id, icon, type, itemPrice);
        //var buyButton = FindFirstObjectByType<BuyButton>();
        //if (buyButton != null)
        //{
        //    buyButton.SetSelectedSlot(this);
        //}
        var itemDetailsUI = ItemDetailsUI.Instance;
        if (itemDetailsUI != null)
        {
            itemDetailsUI.SetCurrentItemId(id, icon, type);
        }
    }


    private void ToggleShopVKPanel(shopvk shopPanel, string id, Sprite icon, string type)
    {
        if (shopPanel == null)
        {
            Debug.LogError("ShopTP panel is null");
            return;
        }
        if (ItemDetailsPanel.Instance != null) ItemDetailsPanel.Instance.Hide();

        // LUÔN show, không toggle hide nữa!
        shopPanel.Show(id, icon, type, itemPrice);
        //var buyButton = FindFirstObjectByType<BuyButton>();
        //if (buyButton != null)
        //{
        //    buyButton.SetSelectedSlot(this);
        //}
        var itemDetailsUI = ItemDetailsUI.Instance;
        if (itemDetailsUI != null)
        {
            itemDetailsUI.SetCurrentItemId(id, icon, type);
        }
    }
    private void ToggleShopPKPanel(shoppk shopPanel, string id, Sprite icon, string type)
    {
        if (shopPanel == null)
        {
            Debug.LogError("ShopTP panel is null");
            return;
        }
        if (ItemDetailsPanel.Instance != null) ItemDetailsPanel.Instance.Hide();

        // LUÔN show, không toggle hide nữa!
        shopPanel.Show(id, icon, type, itemPrice);
        //var buyButton = FindFirstObjectByType<BuyButton>();
        //if (buyButton != null)
        //{
        //    buyButton.SetSelectedSlot(this);
        //}
        var itemDetailsUI = ItemDetailsUI.Instance;
        if (itemDetailsUI != null)
        {
            itemDetailsUI.SetCurrentItemId(id, icon, type);
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
