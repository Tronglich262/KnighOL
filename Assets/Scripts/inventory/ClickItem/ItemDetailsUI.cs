
//sử dụng hero phải khai báo
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
using UnityEngine.Networking;
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
    public DailyCheckInSimple dailyCheckInSimple;

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

    private void Start()
    {

        StartCoroutine(EquipArmorFromSavedJson());
        if (character == null && CharacterUIManager1.Instance != null)
        {
            character = CharacterUIManager1.Instance.character;
            Debug.Log(" character được gán từ CharacterUIManager1.");
        }
    }
    void Awake()
    {
        Debug.Log("Da chay awake ItemDetailsUI");
        Instance = this;
        panel.SetActive(false);
        equipMsgOriginPos = equipMessageText.rectTransform.anchoredPosition;

    }
    public void Show(InventoryItem1 item)
    {
        currentItem = item;
        Debug.Log($"🟢 Show panel: {item.itemId} / {item.quantity}");

        icon.sprite = item.stats?.Icon;
        nameText.text = item.stats?.Name ?? "Không rõ";
        descText.text = $"ID: {item.itemId}\nSố lượng: {item.quantity}";

        if (item.stats != null)
        {
            descText.text = $"<b>{item.stats.Description}</b>\n" +
                            //$"<b>Số lượng:</b> {item.quantity}\n\n" +
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
        string type = currentItem.stats.Type;
        string newWeaponId = currentItem.itemId;
        var dict = JsonConvert.DeserializeObject<Dictionary<string, string>>(PlayerDataHolder1.CharacterJson);

        // Nếu là vũ khí
        if (type == "Bow" || type.Contains("Weapon"))
        {
            string[] weaponKeys = { "PrimaryMeleeWeapon", "SecondaryMeleeWeapon", "Bow" };
            foreach (string key in weaponKeys)
            {
                if (dict.TryGetValue(key, out string oldWeaponId) && !string.IsNullOrEmpty(oldWeaponId) && oldWeaponId != newWeaponId)
                {
                    InventoryManager.Instance.AddItem(oldWeaponId, 1);
                    dict[key] = "";
                }
            }
            InventoryManager.Instance.RemoveItem(newWeaponId, 1);
        }
        else
        {
            // Các item thường swap như cũ
            string equippedItemId = CharacterUIManager1.Instance.GetItemIdFromJson(PlayerDataHolder1.CharacterJson, type);
            if (!string.IsNullOrEmpty(equippedItemId))
            {
                if (equippedItemId == currentItem.itemId)
                {
                    var idx = InventoryManager.Instance.playerInventory.FindIndex(i => i == currentItem);
                    if (idx >= 0)
                    {
                        InventoryManager.Instance.playerInventory.RemoveAt(idx);
                        InventoryManager.Instance.AddItem(equippedItemId, 1);
                    }
                }
                else
                {
                    InventoryManager.Instance.AddItem(equippedItemId, 1);
                    InventoryManager.Instance.RemoveItem(currentItem.itemId, 1);
                }
            }
            else
            {
                InventoryManager.Instance.RemoveItem(currentItem.itemId, 1);
            }
        }

        EquipToCharacter(currentItem.stats);


        if (CharacterUIManager1.Instance != null && currentItem != null)
        {
            // string type = currentItem.stats.Type;
            string itemId = currentItem.itemId;

            // Nếu là Gloves thì hiển thị lại đúng slot từ ArmorSlots
            if (type == "Gloves")
            {
                CharacterUIManager1.Instance.DisplayItem(
                    CharacterUIManager1.Instance.ArmorSlots[2], // Index 2 là Gloves
                    itemId,
                    "Gloves"
                );
                CharacterEquipHandler.EquipPartialArmorFromEntry(character, currentItem.itemId, type);
                Debug.Log("Bao tay " + currentItem.itemId);
            }
            if (type == "Belt")
            {
                CharacterUIManager1.Instance.DisplayItem(
                    CharacterUIManager1.Instance.ArmorSlots[5], // Index 2 là Gloves
                    itemId,
                    "Belt"

                );
                CharacterEquipHandler.EquipPartialArmorFromEntry(character, currentItem.itemId, type);
            }
            if (type == "Boots")
            {
                CharacterUIManager1.Instance.DisplayItem(
                    CharacterUIManager1.Instance.ArmorSlots[1], // Index 2 là Gloves
                    itemId,
                    "Boots"
                );
                CharacterEquipHandler.EquipPartialArmorFromEntry(character, currentItem.itemId, type);
            }
            if (type == "Vest")
            {
                CharacterUIManager1.Instance.DisplayItem1(
                    CharacterUIManager1.Instance.ArmorSlots[4], // Index 2 là Gloves
                    itemId,
                    "Vest"
                );
                CharacterEquipHandler.EquipPartialArmorFromEntry(character, currentItem.itemId, type);
            }
            if (type == "Armor")
            {
                CharacterUIManager1.Instance.DisplayItem(
                    CharacterUIManager1.Instance.ArmorSlots[0], // Index 2 là Gloves
                    itemId,
                    "Armor"
                );
                CharacterEquipHandler.TestEquipArmor(character, currentItem.itemId);

            }
            if (type == "Helmet")
            {
                CharacterUIManager1.Instance.DisplayItem1(
                    CharacterUIManager1.Instance.Helmetslot, // Index 2 là Gloves
                    itemId,
                    "Helmet"
                );

            }

            if (type == "MeleeWeapon1H")
            {
                CharacterUIManager1.Instance.DisplayItem1(
                    CharacterUIManager1.Instance.MeleeWeapon1Hslot, // Index 2 là Gloves
                    itemId,
                    "MeleeWeapon1H"
                );
            }
            if (type == "MeleeWeapon2H")
            {
                CharacterUIManager1.Instance.DisplayItem1(
                    CharacterUIManager1.Instance.MeleeWeapon2Hslot, // Index 2 là Gloves
                    itemId,
                    "MeleeWeapon2H"
                );
            }
            if (type == "Cape")
            {
                CharacterUIManager1.Instance.DisplayItem1(
                    CharacterUIManager1.Instance.Capeslot, // Index 2 là Gloves
                    itemId,
                    "Cape"
                );
            }
            if (type == "Shield")
            {
                CharacterUIManager1.Instance.DisplayItem1(
                    CharacterUIManager1.Instance.Shieldslot, // Index 2 là Gloves
                    itemId,
                    "Shield"
                );
            }
            if (type == "Pauldrons")
            {
                CharacterUIManager1.Instance.DisplayItem1(
                    CharacterUIManager1.Instance.ArmorSlots[3], // Index 2 là Gloves
                    itemId,
                    "Pauldrons"
                );
            }
            if (type == "Glasses")
            {
                CharacterUIManager1.Instance.DisplayItem1(
                    CharacterUIManager1.Instance.Glassesslot, // Index 2 là Gloves
                    itemId,
                    "Glasses"
                );
            }

            if (type == "Hair")
            {
                CharacterUIManager1.Instance.DisplayItem1(
                    CharacterUIManager1.Instance.Hairslot, // Index 2 là Gloves
                    itemId,
                    "Hair"
                );
            }
            if (type == "Back")
            {
                CharacterUIManager1.Instance.DisplayItem1(
                    CharacterUIManager1.Instance.Backslot, // Index 2 là Gloves
                    itemId,
                    "Back"
                );
            }
            if (type == "Bow")
            {
                CharacterUIManager1.Instance.DisplayItem1(
                    CharacterUIManager1.Instance.Bowslot, // Index 2 là Gloves
                    itemId,
                    "Bow"
                );
            }
            CharacterEquipHandler.TestEquipBow(character, currentItem.itemId);



            // Có thể làm tương tự với Boots, Vest, Belt, Armor, Pauldrons...
        }

        if (currentItem == null || currentItem.stats == null)
        {
            Debug.LogError(" currentItem null hoặc thiếu stats.");
            return;
        }

        if (string.IsNullOrEmpty(PlayerDataHolder1.CharacterJson))
        {
            Debug.LogError(" Chưa có CharacterJson.");
            return;
        }

        // Parse JSON hiện tại từ nhân vật
        dict = JsonConvert.DeserializeObject<Dictionary<string, string>>(PlayerDataHolder1.CharacterJson);

        //  Xoá vũ khí cũ nếu đang chuyển đổi loại
        if (currentItem.stats.Type == "Bow")
        {
            dict.Remove("PrimaryMeleeWeapon");
            dict.Remove("SecondaryMeleeWeapon");
        }
        if (currentItem.stats.Type == "PrimaryMeleeWeapon" || currentItem.stats.Type == "MeleeWeapon1H")
        {
            dict.Remove("Bow");
            dict.Remove("SecondaryMeleeWeapon");
        }
        if (currentItem.stats.Type == "SecondaryMeleeWeapon" || currentItem.stats.Type == "MeleeWeapon2H")
        {
            dict.Remove("PrimaryMeleeWeapon");
            dict.Remove("Bow");
        }


        // Ghi đè item vào đúng slot
        switch (currentItem.stats.Type)
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

                //case "Bow":
                dict[currentItem.stats.Type] = currentItem.itemId;
                break;
            case "Bow":
                dict["Bow"] = currentItem.itemId;
                dict["WeaponType"] = "Bow";

                break;
            case "MeleeWeapon1H":
                dict["PrimaryMeleeWeapon"] = currentItem.itemId;
                dict["WeaponType"] = "Melee1H";

                break;
            case "MeleeWeapon2H":
                dict["SecondaryMeleeWeapon"] = currentItem.itemId;
                dict["WeaponType"] = "Melee2H";

                break;
            default:
                Debug.LogWarning($"❌ Loại chưa hỗ trợ: {currentItem.stats.Type}");
                return;
        }
        // Serialize lại JSON
        // Serialize lại JSON
        string updatedJson = JsonConvert.SerializeObject(dict, Formatting.None);
        PlayerDataHolder1.CharacterJson = updatedJson;

        if (PlayerAvatar.Instance != null && PlayerAvatar.Instance.HasStateAuthority)
        {
            PlayerAvatar.Instance.UpdateCharacterJson(updatedJson);
            PlayerAvatar.Instance.SendCharacterJsonToAllClients(); //  thêm dòng này!
            Debug.Log("Đã gửi JSON mới cho tất cả client.");
        }

        //}
        // 🟡 Nếu đang test trên clone, gửi JSON về player thật thông qua controller
        if (playerClone != null)
        {
            var cloneCtrl = playerClone.GetComponent<PlayerCloneController>();
            if (cloneCtrl != null)
            {
                cloneCtrl.SendCharacterJsonToTarget(updatedJson); //  Gửi từ clone → player thật
            }
            else
            {
                Debug.LogError("❌ Không tìm thấy PlayerCloneController.");
            }
        }
        else
        {
            Debug.LogWarning("⚠️ playerClone chưa được gán.");
        }

        // 🟠 Đồng bộ với UI trung tâm (chỉ hiển thị preview)
        if (CharacterUIManager1.Instance != null)
        {
            //  string type = currentItem.stats.Type;
            string itemId = currentItem.itemId;

            if (type == "Armor")
            {
                CleanUnsupportedEntries(dict, character);
                CharacterUIManager1.Instance.character.FromJson(updatedJson);
                StartCoroutine(EquipArmorNextFrame(currentItem.itemId));
            }

            else
            {
                CharacterUIManager1.Instance.character.FromJson(updatedJson);
            }
            string[] mixTypes = new[] { "Boots", "Gloves", "Belt", "Pauldrons", "Vest" };
            foreach (string t in mixTypes)
            {
                if (dict.TryGetValue(t, out string partId) && !string.IsNullOrEmpty(partId))
                {
                    CharacterEquipHandler.EquipPartialArmorFromEntry(character, partId, t);
                }
            }
        }
        if (currentItem == null || currentItem.stats == null)
        {
            ShowEquipMessage(" Trang bị thất bại! Dữ liệu item lỗi");
            return;
        }
        if (string.IsNullOrEmpty(PlayerDataHolder1.CharacterJson))
        {
            ShowEquipMessage(" Trang bị thất bại! Không có dữ liệu nhân vật");
            return;
        }

        ShowEquipMessage(" Trang bị thành công");

        //  Tắt panel
        panel.SetActive(false);
        StartCoroutine(DelayUpdateStatsAndUI());
        // Gửi JSON lên server để lưu theo account của chính client
        // Thay vì check HasStateAuthority của PlayerAvatar.Instance, check theo account hiện tại
        if (AuthManager.Instance != null)
        {
            Debug.Log("🟢 Gửi JSON lên server để lưu theo account của client hiện tại.");
            AuthManager.Instance.StartCoroutine(AuthManager.Instance.SaveCharacterToServer(updatedJson));
        }
        else
        {
            Debug.LogError("❌ Không tìm thấy AuthManager.");
        }



    }
    private IEnumerator DelayUpdateStatsAndUI()
    {
        yield return null;
        CharacterUIManager1.Instance.UpdateCharacterStatsAndUI();
    }

    private string GetEquippedWeaponId(string type)
    {
        var dict = JsonConvert.DeserializeObject<Dictionary<string, string>>(PlayerDataHolder1.CharacterJson);
        switch (type)
        {
            case "Bow":
                return dict.ContainsKey("Bow") ? dict["Bow"] : null;
            case "MeleeWeapon1H":
            case "PrimaryMeleeWeapon":
                return dict.ContainsKey("PrimaryMeleeWeapon") ? dict["PrimaryMeleeWeapon"] : null;
            case "MeleeWeapon2H":
            case "SecondaryMeleeWeapon":
                return dict.ContainsKey("SecondaryMeleeWeapon") ? dict["SecondaryMeleeWeapon"] : null;
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
                        Debug.LogError($"❌ Không tìm thấy MeleeWeapon2H entry: {stats.itemId}");
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
        yield return null; // đợi 1 frame để sprite collection sẵn sàng

        var json = PlayerDataHolder1.CharacterJson;
        if (string.IsNullOrEmpty(json)) yield break;

        var dict = JsonConvert.DeserializeObject<Dictionary<string, string>>(json);
        if (dict.TryGetValue("Armor", out var armorId))
        {
            CharacterEquipHandler.TestEquipArmor(character, armorId);
        }
        if (dict.TryGetValue("WeaponType", out var type) && type == "Melee2H")
        {
            if (dict.TryGetValue("SecondaryMeleeWeapon", out var weaponId))
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
    // Tuấn Anh
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
        Debug.LogError("currentShopItem NULL! Bạn chưa chọn item shop?");
        return;
    }
        int itemId = currentItem.stats.Item_ID;
        int currentGold = PlayerDataHolder1.CurrentPlayerState.gold;
        int accountId = AuthManager.Instance.UserSession.AccountId;
        string token = AuthManager.Instance.UserSession.Token;

        // Bước này chỉ để check nhanh UI, không đảm bảo hoàn toàn (chủ yếu UX).
        // Server sẽ kiểm tra lại!
        int expectedPrice = currentShopItem.price; // ✅
        if (currentGold < expectedPrice)
        {
            ShowEquipMessage("Không đủ vàng!");
            return;
        }

        StartCoroutine(CoBuyItemFromShop(accountId, itemId, token));
    }

    IEnumerator CoBuyItemFromShop(int accountId, int itemId, string token)
    {
        var buyData = new
        {
            AccountId = accountId,
            ItemId = itemId
            // KHÔNG truyền Price!
        };
        string json = Newtonsoft.Json.JsonConvert.SerializeObject(buyData);

        UnityWebRequest req = new UnityWebRequest("https://localhost:7124/api/account/shop/buy", "POST");
        byte[] bodyRaw = System.Text.Encoding.UTF8.GetBytes(json);
        req.uploadHandler = new UploadHandlerRaw(bodyRaw);
        req.downloadHandler = new DownloadHandlerBuffer();
        req.SetRequestHeader("Content-Type", "application/json");
        req.SetRequestHeader("Authorization", "Bearer " + token);

        yield return req.SendWebRequest();

        if (req.result == UnityWebRequest.Result.Success)
        {
            var resp = JsonConvert.DeserializeObject<ShopBuyResponse>(req.downloadHandler.text);
            PlayerDataHolder1.CurrentPlayerState.gold = resp.newGold;
            CharacterUIManager1.Instance.gold.text = resp.newGold.ToString();
            ShowEquipMessage("Mua thành công!");

            InventoryManager.Instance.LoadInventory(null);
        }
        else
        {
            ShowEquipMessage("Lỗi khi mua: " + req.downloadHandler.text);
        }
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
            dailyCheckInSimple.faleCurrentDat();
            dailyCheckInSimple.ClamedDaily();

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
        int accountId = AuthManager.Instance.UserSession.AccountId;
        string token = AuthManager.Instance.UserSession.Token;

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
        string url = "https://localhost:7124/api/Account/market/deposit";
        string json = Newtonsoft.Json.JsonConvert.SerializeObject(dto);

        Debug.Log("Json gửi đi: " + json);

        UnityWebRequest req = new UnityWebRequest(url, "POST");
        byte[] bodyRaw = System.Text.Encoding.UTF8.GetBytes(json);
        req.uploadHandler = new UploadHandlerRaw(bodyRaw);
        req.downloadHandler = new DownloadHandlerBuffer();
        req.SetRequestHeader("Content-Type", "application/json");
        req.SetRequestHeader("Authorization", "Bearer " + token);

        yield return req.SendWebRequest();

        if (req.result == UnityWebRequest.Result.Success)
        {
            ShowEquipMessage("Đã ký gửi thành công!");
            InventoryManager.Instance.LoadInventory(null);
            if (MarketShopUI.Instance != null) MarketShopUI.Instance.LoadMarketItems();
            panel.SetActive(false);
        }
        else
        {
            ShowEquipMessage("Lỗi ký gửi: " + req.downloadHandler.text);
        }
    }


}