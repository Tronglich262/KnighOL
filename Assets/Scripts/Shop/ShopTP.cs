
//new gỡ trang bị swap
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
// hiển thị bảng panel thông tin trang bị nhân vật
public class ShopTP : MonoBehaviour
{
    public static ShopTP Instance;

    public GameObject panelshopTP;
    public Image icon;
    public TMP_Text description;
    public TMP_Text Type;
    public TMP_Text Name;
    private string currentItemId;
    private string currentType;
    public TMP_Text Price;
    //text
    public GameObject shopNotifyPanel;      // Toàn bộ panel (cả nền và text)
    public Image shopNotifyBg;              // Image nền
    public TextMeshProUGUI shopNotifyText;  // Text để hiện thông báo
    private Coroutine notifyCoroutine;
    private void Awake()
    {
        Instance = this;
        panelshopTP.SetActive(false);
    }


    public void Show(string id, Sprite iconSprite, string type = null, int price = 0)
    {

        panelshopTP.SetActive(true);

        icon.sprite = iconSprite;

        currentItemId = id;
        currentType = type;

        string name = id.Split('.').Length > 0 ? id.Split('.').Last() : id;
        string displayType = type ?? "Không rõ loại";

        description.text = $"{displayType}\n\n{GetStatsFromId(id)}";
        Type.text = $"Loại: {displayType}";

        Name.text = $"Tên: {name}";
        Price.text = $"Giá: {price}";
    }


    public void Hide()
    {
        panelshopTP.SetActive(false);
    }
    private string GetStatsFromId(string id)
    {
        var stats = ItemStatDatabase.Instance.GetStats(id);
        if (stats == null)
            return "Không có thông tin.";

        return
            $"Sức mạnh: {stats.Strength}\n" +
            $"Phòng thủ: {stats.Defense}\n" +
            $"Nhanh nhẹn: {stats.Agility}\n" +
            $"Trí tuệ: {stats.Intelligence}\n" +
            $"Thể lực: {stats.Vitality}";
    }
    //gỡ ttrang bị 


    public bool IsVisible3()
    {
        return panelshopTP != null && panelshopTP.activeSelf;
    }

    public bool IsShowingItem(string id)
    {
        return currentItemId == id;
    }

    public void TogglePanel()
    {
        panelshopTP.SetActive(false);
    }
    //text
    public void ShowShopNotify(string message, float typeSpeed = 0.03f, float stayTime = 1.3f, float fadeTime = 0.28f)
    {
        Debug.Log($"ShowShopNotify: {message}, panel={shopNotifyPanel}, bg={shopNotifyBg}, text={shopNotifyText}");

        if (notifyCoroutine != null) StopCoroutine(notifyCoroutine);
        notifyCoroutine = StartCoroutine(ShowShopNotifyRoutine(message, typeSpeed, stayTime, fadeTime));
    }

    private IEnumerator ShowShopNotifyRoutine(string message, float typeSpeed, float stayTime, float fadeTime)
    {
        shopNotifyPanel.SetActive(true);

        // Reset opacity (ẩn panel)
        Color cBg = shopNotifyBg.color; cBg.a = 0; shopNotifyBg.color = cBg;
        Color cText = shopNotifyText.color; cText.a = 0; shopNotifyText.color = cText;
        shopNotifyText.text = "";

        // Fade in nền + text (cùng lúc)
        float t = 0f;
        while (t < fadeTime)
        {
            float a = t / fadeTime;
            cBg.a = Mathf.Lerp(0, 1, a);
            cText.a = Mathf.Lerp(0, 1, a);
            shopNotifyBg.color = cBg;
            shopNotifyText.color = cText;
            t += Time.deltaTime;
            yield return null;
        }
        cBg.a = 1; cText.a = 1;
        shopNotifyBg.color = cBg;
        shopNotifyText.color = cText;

        // Hiệu ứng chữ chạy
        shopNotifyText.text = "";
        foreach (char ch in message)
        {
            shopNotifyText.text += ch;
            yield return new WaitForSeconds(typeSpeed);
        }

        // Chờ text stayTime giây
        yield return new WaitForSeconds(stayTime);

        // Fade out
        t = 0f;
        while (t < fadeTime)
        {
            float a = 1 - t / fadeTime;
            cBg.a = Mathf.Lerp(0, 1, a);
            cText.a = Mathf.Lerp(0, 1, a);
            shopNotifyBg.color = cBg;
            shopNotifyText.color = cText;
            t += Time.deltaTime;
            yield return null;
        }
        shopNotifyPanel.SetActive(false);
    }


}