using Assets.HeroEditor.Common.CharacterScripts;
using HeroEditor.Common;
using HeroEditor.Common.Enums;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public static class CharacterEquipHandler
{
    // ===== MAPPING =====
    public static readonly Dictionary<string, List<int>> ArmorTypeToIndexes = new()
    {
        { "Pauldrons", new List<int> { 0, 1 } },
        { "Boots",     new List<int> { 9, 7 } },
        { "Vest",      new List<int> { 11 } },
        { "Belt",      new List<int> { 8 } },
        { "Gloves",    new List<int> { 3, 4, 2, 5, 6, 10 } }
    };

    public static readonly Dictionary<string, List<int>> BowTypeToIndexes = new()
    {
        { "Arrow", new List<int> { 0 } },
        { "Limb",  new List<int> { 1 } },
        { "Riser", new List<int> { 2 } },
    };

    // ====== TRANG BỊ ITEM CHÍNH (auto detect) ======
    public static void EquipItemToCharacter(InventoryItem1 item)
    {
        if (item == null || item.stats == null) return;

        var character = CharacterUIManager1.Instance.character;
        var type = item.stats.Type;
        var itemId = ItemIdUtility.Normalize(item.itemId);

        var dict = JsonConvert.DeserializeObject<Dictionary<string, string>>(PlayerDataHolder1.CharacterJson)
                   ?? new Dictionary<string, string>();

        bool isWeapon =
            type == EquipKeys.MeleeWeapon1H ||
            type == EquipKeys.MeleeWeapon2H ||
            type == EquipKeys.Bow;

        if (isWeapon)
        {
            ClearWeaponFields(dict);
        }

        switch (type)
        {
            case EquipKeys.Bow:
                dict[EquipKeys.Bow] = itemId;
                dict["WeaponType"] = EquipKeys.Weapon_Bow;
                break;

            case EquipKeys.MeleeWeapon1H:
                dict[EquipKeys.MeleeWeapon1H] = itemId;
                dict[EquipKeys.PrimaryMeleeWeapon] = itemId;
                dict["WeaponType"] = EquipKeys.Weapon_Melee1H;
                break;

            case EquipKeys.MeleeWeapon2H:
                dict[EquipKeys.MeleeWeapon2H] = itemId;
                dict[EquipKeys.PrimaryMeleeWeapon] = itemId;
                dict["WeaponType"] = EquipKeys.Weapon_Melee2H;
                break;
            default:
                dict[type] = itemId;
                break;
        }

        if (ArmorTypeToIndexes.ContainsKey(type))
        {
            EquipPartialArmorFromEntry(character, itemId, type);
        }
        else if (BowTypeToIndexes.ContainsKey(type))
        {
            EquipPartialBowFromEntry(character, itemId, type);
        }
        else
        {
            string updatedJson = JsonConvert.SerializeObject(dict, Formatting.None);
            PlayerDataHolder1.CharacterJson = updatedJson;
            character.FromJson(updatedJson);
        }

        dict["Armor"] = SaveArmorState(character.Armor);
        string finalJson = JsonConvert.SerializeObject(dict, Formatting.None);
        PlayerDataHolder1.CharacterJson = finalJson;

        if (AuthManager.Instance != null)
            AuthManager.Instance.StartCoroutine(AuthManager.Instance.SaveCharacterToServer(finalJson));

        if (ItemDetailsUI.Instance != null && ItemDetailsUI.Instance.playerClone != null)
        {
            var cloneController = ItemDetailsUI.Instance.playerClone.GetComponent<PlayerCloneController>();
            if (cloneController != null)
                cloneController.SendCharacterJsonToTarget(finalJson);
        }
        else
        {
            PlayerAvatar.Instance?.UpdateCharacterJson(finalJson);
        }

        CharacterUIManager1.Instance.RefreshFromLatestJson();
        CharacterUIManager1.Instance.UpdateCharacterStatsAndUI();
    }

    // ====== HÀM MẶC TOÀN BỘ GIÁP ======
    public static void EquipFullArmor(Character character, string armorId)
    {
        if (character == null || character.SpriteCollection == null) return;

        var entry = character.SpriteCollection.Armor.Find(e => e.Id == armorId);
        if (entry == null || entry.Sprites == null || entry.Sprites.Count != 12) return;

        while (character.Armor.Count < 12) character.Armor.Add(null);

        for (int i = 0; i < 12; i++)
        {
            character.Armor[i] = entry.Sprites[i];
        }

        character.EquipArmor(character.Armor);
        character.Initialize();
    }

    // ====== HÀM MẶC TỪNG PHẦN ARMOR ======
    public static void EquipPartialArmorFromEntry(Character character, string itemId, string type)
    {
        if (character == null || character.SpriteCollection == null) return;
        if (!ArmorTypeToIndexes.ContainsKey(type)) return;

        string[] parts = itemId.Split('.');
        if (parts.Length < 4) return;
        string armorName = parts[3];
        string baseArmorId = $"{parts[0]}.{parts[1]}.Armor.{armorName}";

        var entry = character.SpriteCollection.Armor.Find(e => e.Id == baseArmorId);
        if (entry == null) return;

        var indexes = ArmorTypeToIndexes[type];
        while (character.Armor.Count < 12) character.Armor.Add(null);

        foreach (var idx in indexes)    
        {
            if (idx < entry.Sprites.Count)
                character.Armor[idx] = entry.Sprites[idx];
        }

        character.EquipArmor(character.Armor);
        character.Initialize();
    }

    // ====== HÀM MẶC TOÀN BỘ BOW ======
    public static void EquipFullBow(Character character, string bowId)
    {
        if (character == null || character.SpriteCollection == null) return;

        var entry = character.SpriteCollection.Bow.Find(e => e.Id == bowId);
        if (entry == null || entry.Sprites == null || entry.Sprites.Count != 3) return;

        while (character.Bow.Count < 3) character.Bow.Add(null);

        for (int i = 0; i < 3; i++)
        {
            character.Bow[i] = entry.Sprites[i];
        }

        character.EquipBow(character.Bow);
        character.Initialize();
    }

    // ====== HÀM MẶC TỪNG PHẦN BOW ======
    public static void EquipPartialBowFromEntry(Character character, string itemId, string type)
    {
        if (character == null || character.SpriteCollection == null) return;
        if (!BowTypeToIndexes.ContainsKey(type)) return;

        string[] parts = itemId.Split('.');
        if (parts.Length < 4) return;
        string bowName = parts[3];
        string baseBowId = $"{parts[0]}.{parts[1]}.Bow.{bowName}";

        var entry = character.SpriteCollection.Bow.Find(e => e.Id == baseBowId);
        if (entry == null) return;

        var indexes = BowTypeToIndexes[type];
        while (character.Bow.Count < 3) character.Bow.Add(null);

        foreach (var idx in indexes)
        {
            if (idx < entry.Sprites.Count)
                character.Bow[idx] = entry.Sprites[idx];
        }

        character.EquipBow(character.Bow);
        character.Initialize();
    }

    // ====== HÀM LƯU TRẠNG THÁI ARMOR VÀO JSON ======
    private static string SaveArmorState(List<Sprite> armor)
    {
        if (armor == null || armor.All(s => s == null)) return "";
        var first = armor.FirstOrDefault(s => s != null);
        if (first == null) return "";
        var name = first.name;
        var id = name.Split('_')[0];
        return id;
    }

    // ====== HÀM TEST (giữ nguyên) ======
    public static void TestEquipArmor(Character character, string armorId)
    {
        EquipFullArmor(character, armorId);
    }
    public static void TestEquipBow(Character character, string bowId)
    {
        EquipFullBow(character, bowId);
    }

    // ====== HÀM CŨ CHỈ ĐỂ DEMO ICON, ĐỪNG DÙNG CHO GAMEPLAY ======
    public static void EquipPartialArmor(Character character, string type, Sprite sprite)
    {
        if (!ArmorTypeToIndexes.TryGetValue(type, out var indexes)) return;
        while (character.Armor.Count < 12) character.Armor.Add(null);
        foreach (var i in indexes)
        {
            character.Armor[i] = sprite;
        }
        character.EquipArmor(character.Armor);
        character.Initialize();
    }
    public static void EquipPartialBow(Character character, string type, Sprite sprite)
    {
        if (!BowTypeToIndexes.TryGetValue(type, out var indexes)) return;
        while (character.Bow.Count < 3) character.Bow.Add(null);
        foreach (var i in indexes)
        {
            character.Bow[i] = sprite;
        }
        character.EquipBow(character.Bow);
        character.Initialize();
    }
    public static void UnequipItem(string type)
    {
        var character = CharacterUIManager1.Instance.character;
        if (character == null) return;

        bool isWeapon = type == EquipKeys.MeleeWeapon1H ||
                        type == EquipKeys.MeleeWeapon2H ||
                        type == EquipKeys.Bow ||
                        type == EquipKeys.PrimaryMeleeWeapon;

        if (isWeapon)
        {
            ItemDetailsPanel.Instance?.ShowEquipMessage("Không thể tháo vũ khí, hãy chọn vũ khí khác để thay thế!");
            return;
        }

        var dict = JsonConvert.DeserializeObject<Dictionary<string, string>>(PlayerDataHolder1.CharacterJson)
                   ?? new Dictionary<string, string>();

        dict[type] = "";

        if (type == EquipKeys.Armor)
            dict["Armor"] = "";

        if (ArmorTypeToIndexes.TryGetValue(type, out var indexes))
        {
            while (character.Armor.Count < 12) character.Armor.Add(null);

            foreach (var idx in indexes)
            {
                if (idx >= 0 && idx < character.Armor.Count)
                    character.Armor[idx] = null;
            }

            character.EquipArmor(character.Armor);
        }

        PreserveWeaponInfo(dict);
        RestoreWeaponVisual(character, dict);

        string json = JsonConvert.SerializeObject(dict, Formatting.None);
        PlayerDataHolder1.CharacterJson = json;

        if (AuthManager.Instance != null)
            AuthManager.Instance.StartCoroutine(AuthManager.Instance.SaveCharacterToServer(json));

        PlayerAvatar.Instance?.UpdateCharacterJson(json);

        character.Initialize();

        // CHỈ clear đúng slot vừa tháo, KHÔNG refresh toàn bộ UI nữa
        ClearUnequippedSlotOnly(type);

        CharacterUIManager1.Instance.UpdateCharacterStatsAndUI();
    }
    private static void ClearUnequippedSlotOnly(string type)
    {
        var ui = CharacterUIManager1.Instance;
        if (ui == null) return;

        switch (type)
        {
            case EquipKeys.Helmet:
                CharacterEquipmentHelper.ClearSlotUI(ui.Helmetslot);
                break;

            case EquipKeys.Armor:
                CharacterEquipmentHelper.ClearSlotUI(ui.ArmorGeneralSlot);
                if (ui.ArmorSlots != null && ui.ArmorSlots.Length > 0)
                    CharacterEquipmentHelper.ClearSlotUI(ui.ArmorSlots[0]);
                break;

            case EquipKeys.Boots:
                if (ui.ArmorSlots != null && ui.ArmorSlots.Length > 1)
                    CharacterEquipmentHelper.ClearSlotUI(ui.ArmorSlots[1]);
                break;

            case EquipKeys.Gloves:
                if (ui.ArmorSlots != null && ui.ArmorSlots.Length > 2)
                    CharacterEquipmentHelper.ClearSlotUI(ui.ArmorSlots[2]);
                break;

            case EquipKeys.Pauldrons:
                if (ui.ArmorSlots != null && ui.ArmorSlots.Length > 3)
                    CharacterEquipmentHelper.ClearSlotUI(ui.ArmorSlots[3]);
                break;

            case EquipKeys.Vest:
                if (ui.ArmorSlots != null && ui.ArmorSlots.Length > 4)
                    CharacterEquipmentHelper.ClearSlotUI(ui.ArmorSlots[4]);
                break;

            case EquipKeys.Belt:
                if (ui.ArmorSlots != null && ui.ArmorSlots.Length > 5)
                    CharacterEquipmentHelper.ClearSlotUI(ui.ArmorSlots[5]);
                break;

            case EquipKeys.Cape:
                CharacterEquipmentHelper.ClearSlotUI(ui.Capeslot);
                break;

            case EquipKeys.Back:
                CharacterEquipmentHelper.ClearSlotUI(ui.Backslot);
                break;

            case EquipKeys.Mask:
                CharacterEquipmentHelper.ClearSlotUI(ui.Maskslot);
                break;

            case EquipKeys.Glasses:
                CharacterEquipmentHelper.ClearSlotUI(ui.Glassesslot);
                break;

            case EquipKeys.Shield:
                CharacterEquipmentHelper.ClearSlotUI(ui.Shieldslot);
                break;

            case EquipKeys.Hair:
                CharacterEquipmentHelper.ClearSlotUI(ui.Hairslot);
                break;
        }
    }
    // ===== SỬA LỖI ĐỒNG BỘ: DÙNG CHUẨN EQUIPKEYS =====
    private static void PreserveWeaponInfo(Dictionary<string, string> dict)
    {
        // 1. Ưu tiên kiểm tra Bow
        if (dict.TryGetValue(EquipKeys.Bow, out var bow) && !string.IsNullOrEmpty(bow))
        {
            dict[EquipKeys.Bow] = bow;
            dict["WeaponType"] = EquipKeys.Weapon_Bow;

            // Dọn dẹp rác melee nếu có
            dict.Remove(EquipKeys.MeleeWeapon1H);
            dict.Remove(EquipKeys.MeleeWeapon2H);
            dict.Remove(EquipKeys.PrimaryMeleeWeapon);
            return;
        }

        // 2. Kiểm tra Melee 1H
        if (dict.TryGetValue(EquipKeys.MeleeWeapon1H, out var melee1H) && !string.IsNullOrEmpty(melee1H))
        {
            dict[EquipKeys.MeleeWeapon1H] = melee1H;
            dict[EquipKeys.PrimaryMeleeWeapon] = melee1H; // Đồng bộ sang Primary như khi Equip mới
            dict["WeaponType"] = EquipKeys.Weapon_Melee1H;

            dict.Remove(EquipKeys.MeleeWeapon2H);
            dict.Remove(EquipKeys.Bow);
            return;
        }

        // 3. Kiểm tra Melee 2H
        if (dict.TryGetValue(EquipKeys.MeleeWeapon2H, out var melee2H) && !string.IsNullOrEmpty(melee2H))
        {
            dict[EquipKeys.MeleeWeapon2H] = melee2H;
            dict[EquipKeys.PrimaryMeleeWeapon] = melee2H; // Đồng bộ sang Primary
            dict["WeaponType"] = EquipKeys.Weapon_Melee2H;

            dict.Remove(EquipKeys.MeleeWeapon1H);
            dict.Remove(EquipKeys.Bow);
            return;
        }

        // 4. Fallback (Dự phòng nếu JSON chỉ còn PrimaryMeleeWeapon)
        if (dict.TryGetValue(EquipKeys.PrimaryMeleeWeapon, out var primary) && !string.IsNullOrEmpty(primary))
        {
            string wt = dict.TryGetValue("WeaponType", out var val) ? val : "";

            if (wt == EquipKeys.Weapon_Melee1H || wt == "0" || wt.Contains("1H") || wt == "Melee1H")
            {
                dict[EquipKeys.MeleeWeapon1H] = primary;
                dict["WeaponType"] = EquipKeys.Weapon_Melee1H;
            }
            else if (wt == EquipKeys.Weapon_Melee2H || wt == "1" || wt.Contains("2H") || wt == "Melee2H")
            {
                dict[EquipKeys.MeleeWeapon2H] = primary;
                dict["WeaponType"] = EquipKeys.Weapon_Melee2H;
            }
        }
    }

    private static void SyncWeaponConsistency(Dictionary<string, string> dict)
    {
        // Đã được gộp toàn bộ logic an toàn và xử lý triệt để bên trong PreserveWeaponInfo.
        // Giữ hàm này trống để không phá vỡ cấu trúc gọi hàm hiện tại trong UnequipItem.
    }

    private static void RestoreWeaponVisual(Character character, Dictionary<string, string> dict)
    {
        if (character == null || dict == null) return;

        string weaponType = dict.TryGetValue("WeaponType", out var wt) ? wt : "";

        // So sánh bao quát cả giá trị hằng số và các giá trị ép kiểu thủ công
        if (weaponType == EquipKeys.Weapon_Melee1H || weaponType == "Melee1H" || weaponType == "0")
        {
            string id = dict.TryGetValue(EquipKeys.MeleeWeapon1H, out var m1) && !string.IsNullOrEmpty(m1) ? m1 :
                        dict.TryGetValue(EquipKeys.PrimaryMeleeWeapon, out var p1) ? p1 : null;

            if (!string.IsNullOrEmpty(id))
            {
                var entry = character.SpriteCollection.MeleeWeapon1H.FirstOrDefault(e => e.Id == id);
                if (entry != null)
                {
                    character.WeaponType = WeaponType.Melee1H;
                    character.Equip(entry, EquipmentPart.MeleeWeapon1H);
                }
            }
        }
        else if (weaponType == EquipKeys.Weapon_Melee2H || weaponType == "Melee2H" || weaponType == "1")
        {
            string id = dict.TryGetValue(EquipKeys.MeleeWeapon2H, out var m2) && !string.IsNullOrEmpty(m2) ? m2 :
                        dict.TryGetValue(EquipKeys.PrimaryMeleeWeapon, out var p2) ? p2 : null;

            if (!string.IsNullOrEmpty(id))
            {
                var entry = character.SpriteCollection.MeleeWeapon2H.FirstOrDefault(e => e.Id == id);
                if (entry != null)
                {
                    character.WeaponType = WeaponType.Melee2H;
                    character.Equip(entry, EquipmentPart.MeleeWeapon2H);
                }
            }
        }
        else if (weaponType == EquipKeys.Weapon_Bow || weaponType == "Bow" || weaponType == "2")
        {
            if (dict.TryGetValue(EquipKeys.Bow, out var bowId) && !string.IsNullOrEmpty(bowId))
            {
                var entry = character.SpriteCollection.Bow.FirstOrDefault(e => e.Id == bowId);
                if (entry != null)
                {
                    character.WeaponType = WeaponType.Bow;
                    character.Equip(entry, EquipmentPart.Bow);
                }
            }
        }
    }

    // Giữ nguyên hàm ClearWeaponFields
    public static void ClearWeaponFields(Dictionary<string, string> dict)
    {
        if (dict == null) return;
        dict.Remove(EquipKeys.MeleeWeapon1H);
        dict.Remove(EquipKeys.MeleeWeapon2H);
        dict.Remove(EquipKeys.PrimaryMeleeWeapon);
        dict.Remove(EquipKeys.SecondaryMeleeWeapon);
        dict.Remove(EquipKeys.Bow);
        dict.Remove("Firearms");
        dict.Remove("FirearmParams");
        dict.Remove("WeaponType");
    }
    
}
