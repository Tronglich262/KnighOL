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
        var itemId = item.itemId;

        var dict = JsonConvert.DeserializeObject<Dictionary<string, string>>(PlayerDataHolder1.CharacterJson);

        switch (type)
        {
            case "Bow":
                dict["Bow"] = itemId;
                dict["WeaponType"] = "Bow";
                break;
            case "MeleeWeapon1H":
                dict["PrimaryMeleeWeapon"] = itemId;
                dict["WeaponType"] = "Melee1H";
                break;
            case "MeleeWeapon2H":
                dict["SecondaryMeleeWeapon"] = itemId;
                dict["WeaponType"] = "Melee2H";
                break;
            default:
                dict[type] = itemId;
                break;
        }

        // Armor mix từng phần
        if (ArmorTypeToIndexes.ContainsKey(type))
        {
            EquipPartialArmorFromEntry(character, itemId, type);
        }
        // Bow mix từng phần
        else if (BowTypeToIndexes.ContainsKey(type))
        {
            EquipPartialBowFromEntry(character, itemId, type);
        }
        else
        {
            // Các loại khác dùng FromJson (nếu có)
            string updatedJson = JsonConvert.SerializeObject(dict, Formatting.None);
            PlayerDataHolder1.CharacterJson = updatedJson;
            character.FromJson(updatedJson);
        }

        //  Sau khi gán Armor, ta đồng bộ lại JSON (rất quan trọng!)
        dict["Armor"] = SaveArmorState(character.Armor);
        string finalJson = JsonConvert.SerializeObject(dict, Formatting.None);
        PlayerDataHolder1.CharacterJson = finalJson;

        // Gửi server nếu có
        if (AuthManager.Instance != null)
            AuthManager.Instance.StartCoroutine(AuthManager.Instance.SaveCharacterToServer(finalJson));

        //  Nếu là clone → gửi JSON về player thật
        if (ItemDetailsUI.Instance != null && ItemDetailsUI.Instance.playerClone != null)
        {
            var cloneController = ItemDetailsUI.Instance.playerClone.GetComponent<PlayerCloneController>();
            if (cloneController != null)
            {
                cloneController.SendCharacterJsonToTarget(finalJson);
            }
        }
        else
        {
            //  Nếu đang là player thật → cập nhật trực tiếp
            PlayerAvatar.Instance?.UpdateCharacterJson(finalJson);
        }
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
        Debug.Log($"✅ TestEquipBow: Gán Bow '{bowId}' thành công.");
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
}
