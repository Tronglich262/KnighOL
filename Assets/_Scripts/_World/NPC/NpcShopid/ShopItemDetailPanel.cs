using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ShopItemDetailPanel : MonoBehaviour
{
    public static ShopItemDetailPanel Instance;

    [Header("UI References")]
    public GameObject panel;
    public Image icon;
    public TMP_Text nameText;
    public TMP_Text typeText;
    public TMP_Text descriptionText;
    public TMP_Text levelText;
    public TMP_Text priceText;

    [Header("Notification")]
    public GameObject notifyPanel;
    public Image notifyBg;
    public TextMeshProUGUI notifyText;

    private Coroutine notifyCoroutine;

    private void Awake()
    {
        Instance = this;
        panel.SetActive(false);
        if (notifyPanel != null) notifyPanel.SetActive(false);
    }

    public void Show(NpcShopItem item, ItemStats stats, ShopType shopType)
    {
        panel.SetActive(true);

        icon.sprite = stats.Icon;
        nameText.text = $"Tên: {stats.Name}";
        typeText.text = $"Loại: {shopType}";
        levelText.text = $"Cấp yêu cầu: {stats?.LevelRequired ?? 0}";
        priceText.text = $"Giá: {item.price}";

        descriptionText.text =
            $"Sức mạnh: {stats.Strength}\n" +
            $"Phòng thủ: {stats.Defense}\n" +
            $"Nhanh nhẹn: {stats.Agility}\n" +
            $"Trí tuệ: {stats.Intelligence}\n" +
            $"Thể lực: {stats.Vitality}";
    }

    public void Hide() => panel.SetActive(false);

    public void ShowNotify(string message) => Debug.Log($"[Shop] {message}");
}