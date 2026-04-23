using Assets.HeroEditor.Common.CharacterScripts;
using Assets.HeroEditor.FantasyInventory.Scripts.Data;
using HeroEditor.Common.Enums;
using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using Unity.VisualScripting;
using Unity.VisualScripting.Antlr3.Runtime.Misc;
using UnityEngine;
using UnityEngine.UI;
public class ItemDetailsUI : MonoBehaviour
{
    public GameObject playerClone; // Clone preview trong scene
    public static ItemDetailsUI Instance;
    public GameObject panel;
    public GameObject PanelShop;
    public Image icon;
    public TextMeshProUGUI nameText;
    public TextMeshProUGUI descText;
    public Button useButton;
    public Button dropButton;
    public Button closeButton;
    //sử dụng thông qua character ( sử dụng đồ )
    public Character character; // Gán trong Inspector
    // --- Biến phụ trợ từ code B (Tuấn Anh) ---
    private string currentItemId;
    private string currentItemType;
    private Sprite currentIcon;
    public GameObject PanelDaily;
    private InventoryItem1 currentItem;
    //text
    public TextMeshProUGUI equipMessageText;
    private Coroutine equipMessageCoroutine;
    private Vector3 equipMsgOriginPos;
    //ky gửi
    public TMP_InputField inputQuantity;
    public TMP_InputField inputPrice;
    //itembuy
    private NpcShopItem currentShopItem;
    // Cache tối ưu
    private static Dictionary<int, ItemStats> cachedShopStatsById;
    private EquipmentStatManager cachedEquipStatManager;
    private GameObject cachedPlayerObject;
    private void Start()
    {
        BuildShopStatsCacheIfNeeded();
        RefreshPlayerCache();
        if (character == null && CharacterUIManager1.Instance != null)
        {
            character = CharacterUIManager1.Instance.character;
            Debug.Log("character được gán từ CharacterUIManager1.");
        }
        StartCoroutine(EquipArmorFromSavedJson());
    }
    void Awake()
    {
        Debug.Log("Da chay awake ItemDetailsUI");
        Instance = this;
        if (panel != null)
            panel.SetActive(false);
        if (equipMessageText != null)
            equipMsgOriginPos = equipMessageText.rectTransform.anchoredPosition;
    }
    public void Show(InventoryItem1 item)
    {
        currentItem = item;
        Debug.Log($"[ItemDetailsUI] Show panel: {item.itemId} / {item.quantity}");
        icon.sprite = item.stats?.Icon;
        nameText.text = item.stats?.Name ?? "Không rõ";
        descText.text = $"ID: {item.itemId}\nSố lượng: {item.quantity}";
        if (item.stats != null)
        {
            descText.text = $"<b>{item.stats.Description}</b>\n" +
                            //$"<b>Số lượng:</b> {item.quantity}\n\n" +
                            $"<b>Yêu cầu cấp:</b> {item.stats.LevelRequired}\n" +
                            $"<b>Chỉ số:</b>\n" +
                            $"• Sức mạnh: {item.stats.Strength}\n" +
                            $"• Phòng thủ: {item.stats.Defense}\n" +
                            $"• Nhanh nhẹn: {item.stats.Agility}\n" +
                            $"• Trí tuệ: {item.stats.Intelligence}\n" +
                            $"• Sinh lực: {item.stats.Vitality}";
        }
        else
        {
            descText.text = $"ID: {item.itemId}\nSố lượng: {item.quantity}\n(stats null)";
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
        RefreshPlayerCache();
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
        if (!ok)
            return;
        if (panel != null)
            panel.SetActive(false);
    }
    private string GetEquippedWeaponId(string type)
    {
        var dict = GetCharacterJsonDict();
        switch (type)
        {
            case "Bow":
                return dict.TryGetValue(EquipKeys.Bow, out var bowId) ? bowId : null;
            case "MeleeWeapon1H":
                if (dict.TryGetValue(EquipKeys.MeleeWeapon1H, out var melee1H) && !string.IsNullOrEmpty(melee1H))
                    return melee1H;
                return dict.TryGetValue(EquipKeys.PrimaryMeleeWeapon, out var primary1H) ? primary1H : null;
            case "MeleeWeapon2H":
                if (dict.TryGetValue(EquipKeys.MeleeWeapon2H, out var melee2H) && !string.IsNullOrEmpty(melee2H))
                    return melee2H;
                return dict.TryGetValue(EquipKeys.PrimaryMeleeWeapon, out var primary2H) ? primary2H : null;
            default:
                return CharacterUIManager1.Instance.GetItemIdFromJson(PlayerDataHolder1.CharacterJson, type);
        }
    }
    public IEnumerator EquipArmorNextFrame(string itemId)
    {
        yield return null; // Đợi 1 frame để SpriteCollection sẵn sàng
        CharacterEquipHandler.TestEquipArmor(character, itemId);
    }
    public void DropItem()
    {
        if (currentItem == null)
        {
            Debug.LogWarning("Chưa chọn item để vứt.");
            return;
        }
        int quantity = currentItem.quantity;
        string itemName = currentItem.stats != null ? currentItem.stats.Name : currentItem.itemId;
        // Luôn chỉ cần gọi RemoveItem, tự xử lý quantity
        InventoryManager.Instance.RemoveItem(currentItem.itemId, 1);
        ShowEquipMessage($"Đã vứt {(quantity > 1 ? "1" : "cuối cùng")} {itemName}!");
        Debug.Log($"Đã vứt {itemName}");
        panel.SetActive(false);
        if (InventoryUIManager.instance != null)
        {
            InventoryUIManager.instance.DisplayInventory(InventoryManager.Instance.playerInventory);
        }
    }
    public void Close()
    {
        panel.SetActive(false);
    }
    private void EquipToCharacter(ItemStats stats)
    {
        Debug.Log(" ĐANG EQUIP: stats.Type = " + stats.Type + ", stats.Icon = " + (stats.Icon != null ? stats.Icon.name : "NULL"));
        string spriteName = ExtractSpriteName(stats.itemId); // Lấy từ itemId
        Sprite sprite = null;
        switch (stats.Type)
        {
            case "Helmet":
                character.Helmet = stats.Icon;
                break;
            case "Glasses":
                character.Glasses = stats.Icon;
                break;
            case "Cape":
                character.Cape = stats.Icon;
                break;
            case "Back":
                character.Back = stats.Icon;
                break;
            case "Hair":
                character.Hair = stats.Icon;
                break;
            case "Shield":
                character.Shield = stats.Icon;
                break;
            case "Armor":
                EnsureArmorListSize(0);
                break;
            case "Boots":
                break;
            case "Gloves":
                break;
            case "Pauldrons":
                break;
            case "Vest":
                break;
            case "Mask":
                break;
            case "Belt":
                break;
            // === Vũ khí ===
            case "PrimaryMeleeWeapon":
            case "MeleeWeapon1H":
                sprite = FindSpriteInCollection(spriteName, character.SpriteCollection.MeleeWeapon1H)
                         ?? stats.Icon;
                character.PrimaryMeleeWeapon = sprite;
                character.WeaponType = WeaponType.Melee1H;
                break;
            // ===== Secondary Melee (Paired / 2H) =====
            case "MeleeWeapon2H":
            case "SecondaryMeleeWeapon":
                {
                    var entry = character.SpriteCollection.MeleeWeapon2H
                        .FirstOrDefault(e => e.Id == stats.itemId);
                    if (entry == null)
                    {
                        Debug.LogError($"[ItemDetailsUI] Khong tim thay MeleeWeapon2H entry: {stats.itemId}");
                        return;
                    }
                    character.WeaponType = WeaponType.Melee2H;
                    character.Equip(entry, EquipmentPart.MeleeWeapon2H);
                    break;
                }
            case "Bow":
                {
                    var entry = character.SpriteCollection.Bow
                        .FirstOrDefault(e => e.Id == stats.itemId);
                    if (entry == null || entry.Sprites.Count < 2)
                    {
                        Debug.LogError($" Không tìm thấy Bow entry hoặc thiếu sprite: {stats.itemId}");
                        return;
                    }
                    character.WeaponType = WeaponType.Bow;
                    character.Equip(entry, EquipmentPart.Bow);
                    break;
                }
            default:
                Debug.LogWarning($" Không hỗ trợ loại trang bị: {stats.Type}");
                break;
        }
    }
    private void EnsureArmorListSize(int index)
    {
        while (character.Armor.Count <= index)
        {
            character.Armor.Add(null);
        }
    }
    private string ExtractSpriteName(string itemId)
    {
        if (string.IsNullOrEmpty(itemId)) return "";
        int lastDot = itemId.LastIndexOf('.');
        return lastDot >= 0 ? itemId.Substring(lastDot + 1) : itemId;
    }
    private Sprite FindSpriteInCollection(string spriteName, List<HeroEditor.Common.SpriteGroupEntry> groupEntries)
    {
        foreach (var entry in groupEntries)
        {
            if (entry.Sprites == null) continue;
            foreach (var sprite in entry.Sprites)
            {
                if (sprite != null && sprite.name == spriteName)
                    return sprite;
            }
        }
        Debug.LogError($" Không tìm thấy sprite có tên: {spriteName}");
        return null;
    }
    private void CleanUnsupportedEntries(Dictionary<string, string> dict, Character character)
    {
        void RemoveIfMissing(string key, List<HeroEditor.Common.SpriteGroupEntry> group)
        {
            if (dict.ContainsKey(key) && !group.Any(e => e.Id == dict[key]))
            {
                Debug.LogWarning($" Xoá '{key}' vì không tìm thấy: {dict[key]}");
                dict.Remove(key);
            }
        }
        RemoveIfMissing("PrimaryMeleeWeapon", character.SpriteCollection.MeleeWeapon1H);
        RemoveIfMissing("SecondaryMeleeWeapon", character.SpriteCollection.MeleeWeapon2H);
        RemoveIfMissing("Bow", character.SpriteCollection.Bow);
        RemoveIfMissing("Helmet", character.SpriteCollection.Helmet);
        RemoveIfMissing("Cape", character.SpriteCollection.Cape);
        RemoveIfMissing("Back", character.SpriteCollection.Back);
        RemoveIfMissing("Shield", character.SpriteCollection.Shield);
        // Có thể thêm các loại khác nếu bạn sử dụng.
    }
    private IEnumerator EquipArmorFromSavedJson()
    {
        yield return null;
        if (character == null && CharacterUIManager1.Instance != null)
            character = CharacterUIManager1.Instance.character;
        if (character == null)
            yield break;
        var json = PlayerDataHolder1.CharacterJson;
        if (string.IsNullOrEmpty(json))
            yield break;
        var dict = JsonConvert.DeserializeObject<Dictionary<string, string>>(json);
        if (dict == null)
            yield break;
        if (dict.TryGetValue("Armor", out var armorId) && !string.IsNullOrEmpty(armorId))
        {
            CharacterEquipHandler.TestEquipArmor(character, armorId);
        }
        if (dict.TryGetValue("WeaponType", out var type) && type == "Melee2H")
        {
            if (dict.TryGetValue("PrimaryMeleeWeapon", out var weaponId) && !string.IsNullOrEmpty(weaponId))
            {
                var entry = character.SpriteCollection.MeleeWeapon2H.FirstOrDefault(e => e.Id == weaponId);
                if (entry != null)
                {
                    character.WeaponType = WeaponType.Melee2H;
                    character.Equip(entry, EquipmentPart.MeleeWeapon2H);
                }
            }
        }
    }
    public void SetCurrentItemId(string id, Sprite icon, string type)
    {
        currentItemId = id;
        currentItemType = type;
        currentIcon = icon;
        Debug.Log($"[ItemDetailsUI] Đã chọn item: {id}");
        Itemdaily();
    }
    public void OnClickBuy()
    {
        if (currentShopItem == null)
        {
            ShowEquipMessage("Chưa chọn item shop!");
            return;
        }
        Debug.Log($"[OnClickBuy] currentShopItem: {currentShopItem?.itemId}, price: {currentShopItem?.price}, name: {currentShopItem?.name}");
        Debug.Log($"[OnClickBuy] currentItem: {currentItem}, currentItem.stats: {currentItem?.stats}");
        int itemId = currentItem.stats.Item_ID;
        int currentGold = PlayerDataHolder1.CurrentPlayerState.gold;
        int accountId = SessionManager.AccountId;
        string token = SessionManager.Token;
        // Bước này chỉ để check nhanh UI, không đảm bảo hoàn toàn (chủ yếu UX).
        // Server sẽ kiểm tra lại!
        int expectedPrice = currentShopItem.price;
        if (currentGold < expectedPrice)
        {
            ShowEquipMessage("Không đủ vàng!");
            return;
        }
        StartCoroutine(CoBuyItemFromShop(accountId, itemId, token));
    }
    public void SetCurrentShopItem(NpcShopItem shopItem)
    {
        currentShopItem = shopItem;
        if (shopItem == null)
        {
            currentItem = null;
            return;
        }
        BuildShopStatsCacheIfNeeded();
        cachedShopStatsById.TryGetValue(shopItem.itemId, out var stats);
        currentItem = new InventoryItem1
        {
            itemId = shopItem.itemId.ToString(),
            quantity = 1,
            stats = stats
        };
    }
    private void RefreshEquippedSlotUI(string type, string itemId)
    {
        if (CharacterUIManager1.Instance == null || string.IsNullOrEmpty(type) || string.IsNullOrEmpty(itemId))
            return;
        var ui = CharacterUIManager1.Instance;
        switch (type)
        {
            case "Gloves":
                ui.DisplayItem(ui.ArmorSlots[2], itemId, "Gloves");
                CharacterEquipHandler.EquipPartialArmorFromEntry(character, itemId, type);
                break;
            case "Belt":
                ui.DisplayItem(ui.ArmorSlots[5], itemId, "Belt");
                CharacterEquipHandler.EquipPartialArmorFromEntry(character, itemId, type);
                break;
            case "Boots":
                ui.DisplayItem(ui.ArmorSlots[1], itemId, "Boots");
                CharacterEquipHandler.EquipPartialArmorFromEntry(character, itemId, type);
                break;
            case "Vest":
                ui.DisplayItem1(ui.ArmorSlots[4], itemId, "Vest");
                CharacterEquipHandler.EquipPartialArmorFromEntry(character, itemId, type);
                break;
            case "Armor":
                ui.DisplayItem(ui.ArmorSlots[0], itemId, "Armor");
                CharacterEquipHandler.TestEquipArmor(character, itemId);
                break;
            case "Helmet":
                ui.DisplayItem1(ui.Helmetslot, itemId, "Helmet");
                break;
            case "MeleeWeapon1H":
                ui.DisplayItem1(ui.MeleeWeapon1Hslot, itemId, "MeleeWeapon1H");
                break;
            case "MeleeWeapon2H":
                ui.DisplayItem1(ui.MeleeWeapon2Hslot, itemId, "MeleeWeapon2H");
                break;
            case "Cape":
                ui.DisplayItem1(ui.Capeslot, itemId, "Cape");
                break;
            case "Shield":
                ui.DisplayItem1(ui.Shieldslot, itemId, "Shield");
                break;
            case "Pauldrons":
                ui.DisplayItem1(ui.ArmorSlots[3], itemId, "Pauldrons");
                CharacterEquipHandler.EquipPartialArmorFromEntry(character, itemId, type);
                break;
            case "Glasses":
                ui.DisplayItem1(ui.Glassesslot, itemId, "Glasses");
                break;
            case "Hair":
                ui.DisplayItem1(ui.Hairslot, itemId, "Hair");
                break;
            case "Back":
                ui.DisplayItem1(ui.Backslot, itemId, "Back");
                break;
            case "Mask":
                ui.DisplayItem1(ui.Maskslot, itemId, "Mask");
                break;
            case "Bow":
                ui.DisplayItem1(ui.Bowslot, itemId, "Bow");
                CharacterEquipHandler.TestEquipBow(character, itemId);
                break;
        }
    }
    private void HandleInventorySwap(string type, string newItemId, Dictionary<string, string> dict)
    {
        if (type == "Bow" || type.Contains("Weapon"))
        {
            string[] weaponKeys = { "PrimaryMeleeWeapon", "SecondaryMeleeWeapon", "Bow" };
            foreach (string key in weaponKeys)
            {
                if (dict.TryGetValue(key, out string oldWeaponId) &&
                    !string.IsNullOrEmpty(oldWeaponId) &&
                    oldWeaponId != newItemId)
                {
                    InventoryManager.Instance.AddItem(oldWeaponId, 1);
                    dict[key] = "";
                }
            }
            InventoryManager.Instance.RemoveItem(newItemId, 1);
            return;
        }
        string equippedItemId = CharacterUIManager1.Instance.GetItemIdFromJson(PlayerDataHolder1.CharacterJson, type);
        if (!string.IsNullOrEmpty(equippedItemId))
        {
            if (equippedItemId == newItemId)
            {
                int idx = InventoryManager.Instance.playerInventory.FindIndex(i => i == currentItem);
                if (idx >= 0)
                {
                    InventoryManager.Instance.playerInventory.RemoveAt(idx);
                    InventoryManager.Instance.AddItem(equippedItemId, 1);
                }
            }
            else
            {
                InventoryManager.Instance.AddItem(equippedItemId, 1);
                InventoryManager.Instance.RemoveItem(newItemId, 1);
            }
        }
        else
        {
            InventoryManager.Instance.RemoveItem(newItemId, 1);
        }
    }
    private void UpdateCharacterDictAfterEquip(Dictionary<string, string> dict, string type, string itemId)
    {
        if (type == "Bow")
        {
            dict.Remove("PrimaryMeleeWeapon");
            dict.Remove("SecondaryMeleeWeapon");
            dict.Remove("MeleeWeapon1H");
            dict.Remove("MeleeWeapon2H");
        }
        if (type == "PrimaryMeleeWeapon" || type == "MeleeWeapon1H")
        {
            dict.Remove("Bow");
            dict.Remove("SecondaryMeleeWeapon");
            dict.Remove("MeleeWeapon2H");
        }
        if (type == "MeleeWeapon2H")
        {
            dict.Remove("Bow");
            dict.Remove("SecondaryMeleeWeapon");
            dict.Remove("MeleeWeapon1H");
        }
        switch (type)
        {
            case "Helmet":
            case "Armor":
            case "Boots":
            case "Gloves":
            case "Pauldrons":
            case "Vest":
            case "Belt":
            case "Shield":
            case "Cape":
            case "Back":
            case "Glasses":
            case "Hair":
            case "Mask":
                dict[type] = itemId;
                break;
            case "Bow":
                dict["Bow"] = itemId;
                dict["WeaponType"] = "Bow";
                break;
            case "MeleeWeapon1H":
                dict["PrimaryMeleeWeapon"] = itemId;
                dict["MeleeWeapon1H"] = itemId;
                dict.Remove("MeleeWeapon2H");
                dict["WeaponType"] = "Melee1H";
                break;
            case "MeleeWeapon2H":
                dict["PrimaryMeleeWeapon"] = itemId;
                dict["MeleeWeapon2H"] = itemId;
                dict.Remove("MeleeWeapon1H");
                dict.Remove("SecondaryMeleeWeapon");
                dict["WeaponType"] = "Melee2H";
                break;
            default:
                Debug.LogWarning($"[ItemDetailsUI] Loai chua ho tro: {type}");
                break;
        }
    }
    private IEnumerator CoBuyItemFromShop(int accountId, int itemId, string token)
    {
        var buyData = new
        {
            AccountId = accountId,
            ItemId = itemId
        };
        yield return ApiClientBase.Instance.Post<ShopBuyResponse>(
            "account/shop/buy",
            buyData,
            resp =>
            {
                if (resp != null)
                {
                    PlayerDataHolder1.CurrentPlayerState.gold = resp.newGold;
                    if (CharacterUIManager1.Instance != null && CharacterUIManager1.Instance.gold != null)
                        CharacterUIManager1.Instance.gold.text = resp.newGold.ToString();
                }
                ShowEquipMessage("Mua thành công!");
                if (ShopItemDetailPanel.Instance != null)
                    ShopItemDetailPanel.Instance.Hide();
                InventoryManager.Instance.LoadInventory(null);
            },
            error => ShowEquipMessage("Lỗi khi mua: " + error)
        );
    }
    public class ShopBuyResponse
    {
        public string message { get; set; }
        public int newGold { get; set; }
    }
    public void Itemdaily() //chung
    {
        if (PanelDaily != null && PanelDaily.activeSelf)
        {
            InventoryManager.Instance.AddItem(currentItemId, 1);
            Debug.Log(" PanelDaily đang bật → Đã thêm item vào inventory.");
        }
    }
    //hiệu ứng text
    public void ShowEquipMessage(string msg, float duration = 2.5f)
    {
        if (equipMessageCoroutine != null) StopCoroutine(equipMessageCoroutine);
        equipMessageCoroutine = StartCoroutine(FlyUpEquipMessage(msg, duration));
    }
    IEnumerator FlyUpEquipMessage(string msg, float duration)
    {
        // Set lại vị trí, scale, alpha ban đầu mỗi lần gọi
        equipMessageText.text = msg;
        var rect = equipMessageText.rectTransform;
        rect.anchoredPosition = equipMsgOriginPos;
        equipMessageText.color = new Color(1, 1, 1, 0);
        equipMessageText.transform.localScale = Vector3.one * 1.15f;
        // Fade in + scale in (0.15s)
        float t = 0f;
        while (t < 0.15f)
        {
            equipMessageText.color = new Color(1, 1, 1, Mathf.Lerp(0, 1, t / 0.15f));
            equipMessageText.transform.localScale = Vector3.Lerp(Vector3.one * 1.15f, Vector3.one, t / 0.15f);
            t += Time.deltaTime;
            yield return null;
        }
        equipMessageText.color = new Color(1, 1, 1, 1);
        equipMessageText.transform.localScale = Vector3.one;
        // Bay lên (move y lên), giữ trong duration-0.3s
        float moveTime = duration - 0.3f;
        float yStart = equipMsgOriginPos.y;
        float yEnd = yStart + 60f; // Bay lên 60 đơn vị pixel, tuỳ UI chỉnh lại số này
        t = 0f;
        while (t < moveTime)
        {
            float percent = t / moveTime;
            float y = Mathf.Lerp(yStart, yEnd, percent);
            rect.anchoredPosition = new Vector2(rect.anchoredPosition.x, y);
            t += Time.deltaTime;
            yield return null;
        }
        rect.anchoredPosition = new Vector2(rect.anchoredPosition.x, yEnd);
        // Fade out + scale out (0.15s)
        t = 0f;
        while (t < 0.15f)
        {
            equipMessageText.color = new Color(1, 1, 1, Mathf.Lerp(1, 0, t / 0.15f));
            equipMessageText.transform.localScale = Vector3.Lerp(Vector3.one, Vector3.one * 0.95f, t / 0.15f);
            t += Time.deltaTime;
            yield return null;
        }
        equipMessageText.text = "";
        equipMessageText.color = new Color(1, 1, 1, 0);
        rect.anchoredPosition = equipMsgOriginPos; // Reset lại vị trí cho lần sau
    }
    //code ký gửi
    // Class gửi dữ liệu
    public class MarketItemSendDto
    {
        public int SellerAccountId { get; set; }
        public int ItemId { get; set; }
        public int Quantity { get; set; }
        public int Price { get; set; }
    }
    // Trong ItemDetailsUI.cs hoặc class chứa hàm:
    public void OnClickDeposit()
    {
        int quantity = int.Parse(inputQuantity.text);
        int price = int.Parse(inputPrice.text);
        if (currentItem == null || currentItem.stats == null)
        {
            ShowEquipMessage("Không có item để ký gửi");
            return;
        }
        int itemIdInt = currentItem.stats.Item_ID;
        int accountId = SessionManager.AccountId;
        string token = SessionManager.Token;
        MarketItemSendDto dto = new MarketItemSendDto
        {
            SellerAccountId = accountId,
            ItemId = itemIdInt,
            Quantity = quantity,
            Price = price
        };
        StartCoroutine(CoDepositToMarket(dto, token));
    }
    IEnumerator CoDepositToMarket(MarketItemSendDto dto, string token)
    {
        yield return ApiClientBase.Instance.Post<object>(
            "Account/market/deposit",
            dto,
            _ =>
            {
                ShowEquipMessage("Đã ký gửi thành công!");
                InventoryManager.Instance.LoadInventory(null);
                if (MarketShopUI.Instance != null)
                    MarketShopUI.Instance.LoadMarketItems();
                panel.SetActive(false);
            },
            error => ShowEquipMessage("Lỗi ký gửi: " + error)
        );
    }
    private void BuildShopStatsCacheIfNeeded()
    {
        if (cachedShopStatsById != null)
            return;
        cachedShopStatsById = new Dictionary<int, ItemStats>();
        var statsList = Resources.LoadAll<ItemStats>("ItemStats");
        foreach (var stats in statsList)
        {
            if (stats == null) continue;
            if (!cachedShopStatsById.ContainsKey(stats.Item_ID))
                cachedShopStatsById.Add(stats.Item_ID, stats);
        }
        Debug.Log($"[ItemDetailsUI] Cached ItemStats: {cachedShopStatsById.Count}");
    }
    private void RefreshPlayerCache()
    {
        if (PlayerSpawner.LocalPlayerObject == null)
        {
            cachedPlayerObject = null;
            cachedEquipStatManager = null;
            return;
        }
        GameObject playerObj = PlayerSpawner.LocalPlayerObject.gameObject;
        if (cachedPlayerObject != playerObj)
        {
            cachedPlayerObject = playerObj;
            cachedEquipStatManager = playerObj.GetComponent<EquipmentStatManager>();
        }
        if (character == null && CharacterUIManager1.Instance != null)
        {
            character = CharacterUIManager1.Instance.character;
        }
        if (playerClone == null && PlayerCloneController.Instante != null)
        {
            playerClone = PlayerCloneController.Instante.gameObject;
        }
    }
    private Dictionary<string, string> GetCharacterJsonDict()
    {
        if (string.IsNullOrEmpty(PlayerDataHolder1.CharacterJson))
            return new Dictionary<string, string>();
        return JsonConvert.DeserializeObject<Dictionary<string, string>>(PlayerDataHolder1.CharacterJson)
               ?? new Dictionary<string, string>();
    }
    private bool TryValidateCurrentItem(out string message)
    {
        message = "";
        if (currentItem == null)
        {
            message = "Không có item để dùng.";
            return false;
        }
        if (currentItem.stats == null)
        {
            message = "Item bị thiếu dữ liệu stats.";
            return false;
        }
        if (PlayerDataHolder1.CurrentPlayerState == null)
        {
            message = "Chưa có dữ liệu nhân vật.";
            return false;
        }
        if (InventoryManager.Instance == null)
        {
            message = "InventoryManager chưa sẵn sàng.";
            return false;
        }
        int playerLevel = PlayerDataHolder1.CurrentPlayerState.level;
        int requiredLevel = currentItem.stats.LevelRequired;
        if (playerLevel < requiredLevel)
        {
            message = $"Cần cấp {requiredLevel} mới mặc được!";
            return false;
        }
        return true;
    }
    private bool TryHandleInventorySwapBeforeEquip(
        Dictionary<string, string> dict,
        string type,
        string newItemId,
        out string message)
    {
        message = "";
        if (dict == null)
        {
            message = "Dữ liệu nhân vật bị lỗi.";
            return false;
        }
        bool isWeapon =
            type == EquipKeys.Bow ||
            type == EquipKeys.MeleeWeapon1H ||
            type == EquipKeys.MeleeWeapon2H ||
            type.Contains("Weapon");
        if (isWeapon)
            return TryHandleWeaponSwap(dict, newItemId, out message);
        return TryHandleNormalEquipSwap(dict, type, newItemId, out message);
    }
    private bool TryHandleWeaponSwap(
    Dictionary<string, string> dict,
    string newItemId,
    out string message)
    {
        message = "";
        if (InventoryManager.Instance == null)
        {
            message = "Inventory chưa sẵn sàng.";
            return false;
        }
        HashSet<string> equippedWeaponIds = new HashSet<string>();
        AddIfValid(equippedWeaponIds, CharacterJsonService.GetValue(dict, EquipKeys.PrimaryMeleeWeapon));
        AddIfValid(equippedWeaponIds, CharacterJsonService.GetValue(dict, EquipKeys.SecondaryMeleeWeapon));
        AddIfValid(equippedWeaponIds, CharacterJsonService.GetValue(dict, EquipKeys.MeleeWeapon1H));
        AddIfValid(equippedWeaponIds, CharacterJsonService.GetValue(dict, EquipKeys.MeleeWeapon2H));
        AddIfValid(equippedWeaponIds, CharacterJsonService.GetValue(dict, EquipKeys.Bow));
        if (equippedWeaponIds.Contains(newItemId))
        {
            message = "Vũ khí này đã đang được trang bị.";
            return false;
        }
        foreach (var oldWeaponId in equippedWeaponIds)
        {
            if (!string.IsNullOrEmpty(oldWeaponId))
                InventoryManager.Instance.AddItem(oldWeaponId, 1);
        }
        InventoryManager.Instance.RemoveItem(newItemId, 1);
        return true;
    }
    private bool TryHandleNormalEquipSwap(
    Dictionary<string, string> dict,
    string type,
    string newItemId,
    out string message)
    {
        message = "";
        if (InventoryManager.Instance == null)
        {
            message = "Inventory chưa sẵn sàng.";
            return false;
        }
        string equippedItemId = CharacterJsonService.GetValue(dict, type);
        if (!string.IsNullOrEmpty(equippedItemId) && equippedItemId == newItemId)
        {
            message = "Item này đã đang được trang bị.";
            return false;
        }
        if (!string.IsNullOrEmpty(equippedItemId))
        {
            InventoryManager.Instance.AddItem(equippedItemId, 1);
        }
        InventoryManager.Instance.RemoveItem(newItemId, 1);
        return true;
    }
    private void AddIfValid(HashSet<string> set, string itemId)
    {
        if (!string.IsNullOrWhiteSpace(itemId))
            set.Add(itemId);
    }
}