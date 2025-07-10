//using ApiLogin.modelAccount;
//using ApiLogin.modelAccount;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.Networking;
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

    // TRUYỀN CẢ MarketItemDto và ItemStats!
    public void SetData(MarketItemDto item, ItemStats stats)
    {
        currentMarketItem = item;
       // Debug.Log($"SetData: {item}, stats: {stats}");
        //Debug.Log($"Name: {Name}, SoLuong: {SoLuong}, Price: {Price}, StatsText: {StatsText}, Icon: {Icon}");

        if (Name == null) Debug.LogError("Name not assigned!");
        if (SoLuong == null) Debug.LogError("SoLuong not assigned!");
        if (Price == null) Debug.LogError("Price not assigned!");
        if (StatsText == null) Debug.LogError("StatsText not assigned!");
        if (Icon == null) Debug.LogError("Icon not assigned!");

        Name.text = stats != null ? stats.Name : $"ID:{item.item_ID}";
        SoLuong.text = $"Số Lượng: {item.quantity.ToString()}";
        Price.text = $"Giá: {item.price.ToString()}";
        if (Icon != null && stats != null && stats.Icon != null)
            Icon.sprite = stats.Icon;
        StatsText.text = stats != null
            ? $" Sức mạnh:{stats.Strength} \n Phòng thủ:{stats.Defense} \n Nhanh nhẹn:{stats.Agility} \n Trí tuệ:{stats.Intelligence} \n Sinh lực:{stats.Vitality}"
            : "Không có dữ liệu";
    }


    //mua click 
    public void OnClickBuy()
    {
        if (currentMarketItem == null)
        {
            ShowMessage("Chưa chọn món hàng");
            return;
        }

        int quantity = 1; // Có thể lấy từ UI input nếu bạn có
        int buyerAccountId = AuthManager.Instance.UserSession.AccountId; // hoặc InventoryManager.Instance.session.AccountId
        string token = AuthManager.Instance.UserSession.Token; // hoặc InventoryManager.Instance.session.Token

        BuyMarketItemDto dto = new BuyMarketItemDto
        {
            MarketItem_ID = currentMarketItem.marketItem_ID,
            Quantity = quantity,
            BuyerAccountId = buyerAccountId
        };

        StartCoroutine(CoBuyMarketItem(dto, token));
    }

    IEnumerator CoBuyMarketItem(BuyMarketItemDto dto, string token)
    {
        string url = "https://localhost:7124/api/Account/market/buy";

        string json = Newtonsoft.Json.JsonConvert.SerializeObject(dto);
        Debug.Log("JSON mua hàng gửi đi: " + json);

        UnityWebRequest req = new UnityWebRequest(url, "POST");
        byte[] bodyRaw = System.Text.Encoding.UTF8.GetBytes(json);
        req.uploadHandler = new UploadHandlerRaw(bodyRaw);
        req.downloadHandler = new DownloadHandlerBuffer();
        req.SetRequestHeader("Content-Type", "application/json");
        req.SetRequestHeader("Authorization", "Bearer " + token);

        yield return req.SendWebRequest();

        if (req.result == UnityWebRequest.Result.Success)
        {
            ShowMessage("Mua thành công");
            InventoryManager.Instance.LoadInventory(null);
            MarketShopUI.Instance.LoadMarketItems();
        }
        else
        {
            ShowMessage("Lỗi mua hàng: " + req.downloadHandler.text);
            Debug.LogError("Lỗi mua hàng: " + req.downloadHandler.text);
        }
    }

    private void ShowMessage(string msg)
    {
        Debug.Log(msg);
        // Hiển thị UI message nếu có
    }
}

[System.Serializable]
public class BuyMarketItemDto
{
    public int MarketItem_ID;
    public int Quantity;
    public int BuyerAccountId;
}