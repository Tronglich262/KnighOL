using Assets.HeroEditor.Common.CharacterScripts;
using Assets.HeroEditor.FantasyInventory.Scripts.Data;
using HeroEditor.Common.Enums;
using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ItemDetailsUI : MonoBehaviour
{
    public static ItemDetailsUI Instance;

    [Header("UI References")]
    public GameObject panel;
    public Image icon;
    public TextMeshProUGUI nameText;
    public TextMeshProUGUI descText;
    public Button useButton;
    public Button dropButton;
    public Button closeButton;
    public Character character;
    public GameObject playerClone;     
    public GameObject PanelShop;
    [Header("Message Animation")]
    public TextMeshProUGUI equipMessageText;

    [Header("Market / Shop")]
    public TMP_InputField inputQuantity;
    public TMP_InputField inputPrice;
    public GameObject PanelDaily;

    private InventoryItem1 currentItem;
    private NpcShopItem currentShopItem;
    private Coroutine equipMessageCoroutine;
    private Vector3 equipMsgOriginPos;

    private void Awake()
    {
        Instance = this;
        if (panel != null) panel.SetActive(false);
        if (equipMessageText != null)
            equipMsgOriginPos = equipMessageText.rectTransform.anchoredPosition;
    }

    private void Start()
    {
        if (character == null && CharacterUIManager1.Instance != null)
            character = CharacterUIManager1.Instance.character;

        StartCoroutine(EquipArmorFromSavedJson());
    }

    public void Show(InventoryItem1 item)
    {
        currentItem = item;
        currentShopItem = null;

        icon.sprite = item.stats?.Icon;
        nameText.text = item.stats?.Name ?? "Không rõ";

        if (item.stats != null)
        {
            descText.text = $"<b>{item.stats.Description}</b>\n" +
                            $"<b>Yêu c?u c?p:</b> {item.stats.LevelRequired}\n" +
                            $"<b>Ch? s?:</b>\n" +
                            $"• S?c m?nh: {item.stats.Strength}\n" +
                            $"• Phòng th?: {item.stats.Defense}\n" +
                            $"• Nhanh nh?n: {item.stats.Agility}\n" +
                            $"• Trí tu?: {item.stats.Intelligence}\n" +
                            $"• Sinh l?c: {item.stats.Vitality}";
        }
        else
        {
            descText.text = $"ID: {item.itemId}\nS? lu?ng: {item.quantity}";
        }

        panel.SetActive(true);
    }

    public void UseItem()
    {
        if (!TryValidateCurrentItem(out string failMessage))
        {
            ShowEquipMessage(failMessage);
            return;
        }

        string type = currentItem.stats.Type;
        string newItemId = ItemIdUtility.Normalize(currentItem.itemId);

        var dict = CharacterJsonService.LoadDict();
        if (!TryHandleInventorySwapBeforeEquip(dict, type, newItemId, out failMessage))
        {
            ShowEquipMessage(failMessage);
            return;
        }

        bool ok = EquipmentCoordinator.Equip(currentItem, out string message);
        ShowEquipMessage(message);

        if (ok && panel != null)
            panel.SetActive(false);

        RefreshEquippedSlotUI(type, newItemId);
    }

    public void DropItem()
    {
        if (currentItem == null) return;

        string itemName = currentItem.stats?.Name ?? currentItem.itemId;
        InventoryManager.Instance.RemoveItem(currentItem.itemId, 1);

        ShowEquipMessage($"Ðã v?t {(currentItem.quantity > 1 ? "1" : "cu?i cùng")} {itemName}!");
        panel.SetActive(false);

        if (InventoryUIManager.instance != null)
            InventoryUIManager.instance.DisplayInventory(InventoryManager.Instance.playerInventory);
    }

    // ====================== NPC SHOP BUY ======================
    public void OnClickBuy()
    {
        if (currentShopItem == null)
        {
            ShowEquipMessage("Chua ch?n item shop!");
            return;
        }

        int itemId = currentShopItem.itemId;
        int accountId = SessionManager.AccountId;

        var buyData = new { AccountId = accountId, ItemId = itemId };

        StartCoroutine(ApiClientBase.GetOrCreate().Post<ShopBuyResponse>(
            "account/shop/buy",
            buyData,
            resp =>
            {
                PlayerDataHolder1.CurrentPlayerState.gold = resp.newGold;
                if (CharacterUIManager1.Instance?.gold != null)
                    CharacterUIManager1.Instance.gold.text = resp.newGold.ToString();

                ShowEquipMessage("Mua thành công!");
                if (ShopItemDetailPanel.Instance != null) ShopItemDetailPanel.Instance.Hide();
                InventoryManager.Instance.LoadInventory(null);
                panel.SetActive(false);
            },
            error => ShowEquipMessage("L?i mua: " + error)
        ));
    }

    public void SetCurrentShopItem(NpcShopItem shopItem)
    {
        currentShopItem = shopItem;
        currentItem = null;
    }

    public class ShopBuyResponse { public string message { get; set; } public int newGold { get; set; } }

    // ====================== MARKET DEPOSIT ======================
    public void OnClickDeposit()
    {
        if (currentItem == null || currentItem.stats == null)
        {
            ShowEquipMessage("Không có item d? ký g?i");
            return;
        }

        int quantity = int.TryParse(inputQuantity.text, out var q) ? q : 1;
        int price = int.TryParse(inputPrice.text, out var p) ? p : 0;

        if (quantity <= 0 || price <= 0)
        {
            ShowEquipMessage("S? lu?ng ho?c giá không h?p l?!");
            return;
        }

        var dto = new { ItemId = currentItem.stats.Item_ID, Quantity = quantity, Price = price };

        StartCoroutine(ApiClientBase.GetOrCreate().Post<object>(
            "Account/market/deposit",
            dto,
            _ =>
            {
                ShowEquipMessage("Ðã ký g?i thành công!");
                InventoryManager.Instance.LoadInventory(null);
                if (MarketShopUI.Instance != null) MarketShopUI.Instance.LoadMarketItems();
                panel.SetActive(false);
            },
            error => ShowEquipMessage("L?i ký g?i: " + error)
        ));
    }

    public void SetCurrentItemId(string id, Sprite iconSprite, string type)
    {
        currentItem = new InventoryItem1 { itemId = id };
        Itemdaily();
    }

    public void Itemdaily()
    {
        if (PanelDaily != null && PanelDaily.activeSelf)
            InventoryManager.Instance.AddItem(currentItem.itemId, 1);
    }

    // ====================== ANIMATION ======================
    public void ShowEquipMessage(string msg, float duration = 2.5f)
    {
        if (equipMessageCoroutine != null) StopCoroutine(equipMessageCoroutine);
        equipMessageCoroutine = StartCoroutine(FlyUpEquipMessage(msg, duration));
    }

    private IEnumerator FlyUpEquipMessage(string msg, float duration)
    {
        equipMessageText.text = msg;
        var rect = equipMessageText.rectTransform;
        rect.anchoredPosition = equipMsgOriginPos;
        equipMessageText.color = new Color(1, 1, 1, 0);
        equipMessageText.transform.localScale = Vector3.one * 1.15f;

        yield return StartCoroutine(Animate(0.15f, t => {
            equipMessageText.color = new Color(1, 1, 1, t);
            equipMessageText.transform.localScale = Vector3.Lerp(Vector3.one * 1.15f, Vector3.one, t);
        }));

        float moveTime = duration - 0.3f;
        yield return StartCoroutine(Animate(moveTime, t => {
            float y = Mathf.Lerp(equipMsgOriginPos.y, equipMsgOriginPos.y + 60f, t);
            rect.anchoredPosition = new Vector2(rect.anchoredPosition.x, y);
        }));

        yield return StartCoroutine(Animate(0.15f, t => {
            equipMessageText.color = new Color(1, 1, 1, 1 - t);
            equipMessageText.transform.localScale = Vector3.Lerp(Vector3.one, Vector3.one * 0.95f, t);
        }));

        equipMessageText.text = "";
        rect.anchoredPosition = equipMsgOriginPos;
    }

    private IEnumerator Animate(float duration, Action<float> onUpdate)
    {
        float t = 0f;
        while (t < 1f)
        {
            t += Time.deltaTime / duration;
            onUpdate(Mathf.Clamp01(t));
            yield return null;
        }
        onUpdate(1f);
    }

    // ====================== LOGIC CU C?A B?N (dã gi? l?i) ======================
    private bool TryValidateCurrentItem(out string message)
    {
        message = "";
        if (currentItem?.stats == null) { message = "Item b? thi?u d? li?u stats."; return false; }
        int playerLevel = PlayerDataHolder1.CurrentPlayerState?.level ?? 0;
        if (playerLevel < currentItem.stats.LevelRequired) { message = $"C?n c?p {currentItem.stats.LevelRequired} m?i m?c du?c!"; return false; }
        return true;
    }

    private bool TryHandleInventorySwapBeforeEquip(Dictionary<string, string> dict, string type, string newItemId, out string message)
    {
        message = "";
        return type == "Bow" || type.Contains("Weapon")
            ? TryHandleWeaponSwap(dict, newItemId, out message)
            : TryHandleNormalEquipSwap(dict, type, newItemId, out message);
    }

    private bool TryHandleWeaponSwap(Dictionary<string, string> dict, string newItemId, out string message)
    {
        message = "";
        // Logic vu khí t? code cu c?a b?n
        InventoryManager.Instance.RemoveItem(newItemId, 1);
        return true;
    }

    private bool TryHandleNormalEquipSwap(Dictionary<string, string> dict, string type, string newItemId, out string message)
    {
        message = "";
        string equipped = CharacterUIManager1.Instance.GetItemIdFromJson(PlayerDataHolder1.CharacterJson, type);
        if (!string.IsNullOrEmpty(equipped))
            InventoryManager.Instance.AddItem(equipped, 1);
        InventoryManager.Instance.RemoveItem(newItemId, 1);
        return true;
    }

    private void RefreshEquippedSlotUI(string type, string itemId)
    {
        if (CharacterUIManager1.Instance == null) return;
        var ui = CharacterUIManager1.Instance;

        switch (type)
        {
            case "Gloves": ui.DisplayItem(ui.ArmorSlots[2], itemId, "Gloves"); CharacterEquipHandler.EquipPartialArmorFromEntry(character, itemId, type); break;
            case "Belt": ui.DisplayItem(ui.ArmorSlots[5], itemId, "Belt"); CharacterEquipHandler.EquipPartialArmorFromEntry(character, itemId, type); break;
            case "Boots": ui.DisplayItem(ui.ArmorSlots[1], itemId, "Boots"); CharacterEquipHandler.EquipPartialArmorFromEntry(character, itemId, type); break;
            case "Vest": ui.DisplayItem1(ui.ArmorSlots[4], itemId, "Vest"); CharacterEquipHandler.EquipPartialArmorFromEntry(character, itemId, type); break;
            case "Armor": ui.DisplayItem(ui.ArmorSlots[0], itemId, "Armor"); CharacterEquipHandler.TestEquipArmor(character, itemId); break;
            case "Helmet": ui.DisplayItem1(ui.Helmetslot, itemId, "Helmet"); break;
            case "MeleeWeapon1H": ui.DisplayItem1(ui.MeleeWeapon1Hslot, itemId, "MeleeWeapon1H"); break;
            case "MeleeWeapon2H": ui.DisplayItem1(ui.MeleeWeapon2Hslot, itemId, "MeleeWeapon2H"); break;
            case "Cape": ui.DisplayItem1(ui.Capeslot, itemId, "Cape"); break;
            case "Shield": ui.DisplayItem1(ui.Shieldslot, itemId, "Shield"); break;
            case "Pauldrons": ui.DisplayItem1(ui.ArmorSlots[3], itemId, "Pauldrons"); CharacterEquipHandler.EquipPartialArmorFromEntry(character, itemId, type); break;
            case "Glasses": ui.DisplayItem1(ui.Glassesslot, itemId, "Glasses"); break;
            case "Hair": ui.DisplayItem1(ui.Hairslot, itemId, "Hair"); break;
            case "Back": ui.DisplayItem1(ui.Backslot, itemId, "Back"); break;
            case "Mask": ui.DisplayItem1(ui.Maskslot, itemId, "Mask"); break;
            case "Bow": ui.DisplayItem1(ui.Bowslot, itemId, "Bow"); CharacterEquipHandler.TestEquipBow(character, itemId); break;
        }
    }

    private IEnumerator EquipArmorFromSavedJson()
    {
        yield return null;
        if (character == null || string.IsNullOrEmpty(PlayerDataHolder1.CharacterJson)) yield break;

        var dict = JsonConvert.DeserializeObject<Dictionary<string, string>>(PlayerDataHolder1.CharacterJson);
        if (dict?.TryGetValue("Armor", out var armorId) == true && !string.IsNullOrEmpty(armorId))
            CharacterEquipHandler.TestEquipArmor(character, armorId);
    }


    public void Close() => panel?.SetActive(false);
}