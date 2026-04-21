using Newtonsoft.Json;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class MarketItemRowUI : MonoBehaviour
{
    public Image Icon;
    public TextMeshProUGUI Name;
    public TextMeshProUGUI SoLuong;
    public TextMeshProUGUI Price;
    public Button Mua;
    public TextMeshProUGUI StatsText;

    private MarketItemDto currentMarketItem;

    public void SetData(MarketItemDto item, ItemStats stats)
    {
        currentMarketItem = item;

        Name.text = stats != null ? stats.Name : $"ID:{item.item_ID}";
        SoLuong.text = $"Số Lượng: {item.quantity}";
        Price.text = $"Giá: {item.price}";

        if (Icon != null && stats?.Icon != null)
            Icon.sprite = stats.Icon;

        StatsText.text = stats != null
            ? $"Sức mạnh: {stats.Strength}\nPhòng thủ: {stats.Defense}\nNhanh nhẹn: {stats.Agility}\nTrí tuệ: {stats.Intelligence}\nSinh lực: {stats.Vitality}"
            : "Không có dữ liệu";
    }

    public void OnClickBuy()
    {
        if (currentMarketItem == null)
        {
            ItemDetailsUI.Instance.ShowEquipMessage("Chưa chọn món hàng");
            return;
        }

        var dto = new BuyMarketItemDto
        {
            MarketItem_ID = currentMarketItem.marketItem_ID,
            Quantity = 1,
            BuyerAccountId = SessionManager.AccountId
        };

        StartCoroutine(CoBuyMarketItem(dto));
    }

    private IEnumerator CoBuyMarketItem(BuyMarketItemDto dto)
    {
        yield return ApiClientBase.Instance.Post<object>(
            "Account/market/buy",
            dto,
            _ =>
            {
                ItemDetailsUI.Instance.ShowEquipMessage("Mua thành công!");
                InventoryManager.Instance.LoadInventory(null);
                if (MarketShopUI.Instance != null)
                    MarketShopUI.Instance.LoadMarketItems();
            },
            error => ItemDetailsUI.Instance.ShowEquipMessage("Lỗi mua hàng: " + error)
        );
    }
}

[System.Serializable]
public class BuyMarketItemDto
{
    public int MarketItem_ID;
    public int Quantity;
    public int BuyerAccountId;
}