using Assets.HeroEditor.Common.CharacterScripts;
using Assets.HeroEditor.Common.CommonScripts;
using HeroEditor.Common.Enums;
using Newtonsoft.Json;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class CharacterUIManager1 : MonoBehaviour
{
    public GameObject Helmetslot;  //mu
    public GameObject[] ArmorSlots;

    public GameObject Vestslot;// ao trong
    public GameObject Pauldronsslot;//quan
    public GameObject Glovesslot;//bao tay
    public GameObject Firearms1Hslot;
    public GameObject Firearms2Hslot;

    // public GameObject Firearms1Hslot; // Súng
    // public GameObject Firearms2Hslot; // Súng to*/
    public GameObject Bowslot; // Cung
    public GameObject Hairslot;//toc
    public GameObject Beltslot;//nhan
    public GameObject Capeslot;//ao choang
    public GameObject Backslot; // Bao cung
    public GameObject Maskslot;//bit mat
    public GameObject Glassesslot; //kinh
    public GameObject Shieldslot;//khien
    public GameObject Bootslot;
    //
    public GameObject ArmorGeneralSlot;
    public GameObject MeleeWeapon1Hslot;
    public GameObject MeleeWeapon2Hslot; // Nếu có dùng vũ khí 2 tay
    //load chỉ số
    private List<ItemStats> equippedItems = new List<ItemStats>();
    public static CharacterUIManager1 Instance;
    public Character character; // ← nhân vật trong UI

    public TextMeshProUGUI gold;
    public TextMeshProUGUI diamond;
    private void Awake()
    {
        Instance = this;
    }

    void Start()
    {
        if (PlayerDataHolder1.CharacterJson != null && character != null)
        {
            character.FromJson(PlayerDataHolder1.CharacterJson);
            Debug.Log(" Character UI đã load lại từ JSON khi vào lại game.");
        }
        LoadCharacterToUI();
    }
    private void Update()
    {
        var state = PlayerDataHolder1.CurrentPlayerState;
        if (state == null) return;
        gold.text = $"{state.gold}";
        diamond.text = $"{state.diamond}";


    }

    public  void LoadCharacterToUI()
    {
        string json = PlayerDataHolder1.CharacterJson;
        Debug.Log("CharacterJson: " + json);

        if (string.IsNullOrEmpty(json))
        {
            Debug.LogWarning("CharacterJson rỗng.");
            return;
        }
        CharacterData characterData = JsonUtility.FromJson<CharacterData>(json);
        string[] armorTypes = { "Armor", "Boots", "Gloves", "Pauldrons", "Vest", "Belt" };

        for (int i = 0; i < ArmorSlots.Length && i < armorTypes.Length; i++)
        {
            string armorValue = GetArmorValue(json, i);
            string expectedType = armorTypes[i];

            DisplayItem(ArmorSlots[i], armorValue, expectedType);

            
        }
        string fullArmorId = GetItemIdFromJson(PlayerDataHolder1.CharacterJson, "Armor");
        if (!string.IsNullOrEmpty(fullArmorId))
        {
            DisplayItem1(ArmorGeneralSlot, fullArmorId, "Armor");
        }

        DisplayItem1(Helmetslot, characterData.Helmet);
        DisplayItem1(MeleeWeapon1Hslot, characterData.PrimaryMeleeWeapon);
        DisplayItem1(MeleeWeapon2Hslot, characterData.SecondaryMeleeWeapon);
        if (characterData.WeaponType == "Firearms1H" && !string.IsNullOrEmpty(characterData.Firearms))
        {
            DisplayItem1(Firearms1Hslot, characterData.Firearms);
        }
        if (!string.IsNullOrEmpty(characterData.Bow))
        {
            DisplayItem1(Bowslot, characterData.Bow);
        }
        DisplayItem1(Hairslot, characterData.Hair);
        DisplayItem1(Pauldronsslot, characterData.Pauldrons);
        DisplayItem1(Bootslot, characterData.Boots);
        DisplayItem1(Beltslot, characterData.Belt);
        DisplayItem1(Glovesslot, characterData.Gloves);
        DisplayItem1(Vestslot, characterData.Vest);
        DisplayItem1(Capeslot, characterData.Cape);
        DisplayItem1(Backslot, characterData.Back);
        DisplayItem1(Maskslot, characterData.Mask);
        DisplayItem1(Glassesslot, characterData.Glasses);
        DisplayItem1(Shieldslot, characterData.Shield);
        GameObject player = GameObject.FindWithTag("Player");
        if (player != null)
        {
            var statComp = player.GetComponent<CharacterStats>();
            if (statComp != null)
            {
                statComp.RecalculateStatsFromEquipment(equippedItems);
            }
        }
    }
    public void LoadCharacterFromJson(string json)
    {
        if (string.IsNullOrEmpty(json)) return;

        var characterData = JsonConvert.DeserializeObject<CharacterData>(json);
        if (characterData == null) return;

        equippedItems.Clear(); // clear stat cũ

        //  Quan trọng: cập nhật Armor[]
        string[] armorTypes = { "Armor", "Boots", "Gloves", "Pauldrons", "Vest", "Belt" };
        for (int i = 0; i < ArmorSlots.Length && i < armorTypes.Length; i++)
        {
            string armorValue = GetArmorValue(json, i);
            string expectedType = armorTypes[i];
            DisplayItem(ArmorSlots[i], armorValue, expectedType);
            Debug.Log($"thu tu duyệt+ {ArmorSlots[i]}, {armorValue}, {expectedType}");
        }

        // Các slot còn lại
        DisplayItem1(Helmetslot, characterData.Helmet, "Helmet");
        DisplayItem1(Glovesslot, characterData.Gloves, "Gloves");
        DisplayItem1(MeleeWeapon1Hslot, characterData.PrimaryMeleeWeapon, "PrimaryMeleeWeapon");
        DisplayItem1(MeleeWeapon2Hslot, characterData.SecondaryMeleeWeapon, "SecondaryMeleeWeapon");
        DisplayItem1(Firearms1Hslot, characterData.Firearms1H, "Firearms1H");
        DisplayItem1(Firearms2Hslot, characterData.Firearms2H, "Firearms2H");
        if (!string.IsNullOrEmpty(characterData.Bow))
        {
            DisplayItem1(Bowslot, characterData.Bow, "Bow");
        }
        DisplayItem1(Hairslot, characterData.Hair, "Hair");
        DisplayItem1(Pauldronsslot, characterData.Pauldrons, "Pauldrons");
        DisplayItem1(Bootslot, characterData.Boots, "Boots");
        DisplayItem1(Beltslot, characterData.Belt, "Belt");
        DisplayItem1(Vestslot, characterData.Vest, "Vest");
        DisplayItem1(Capeslot, characterData.Cape, "Cape");
        DisplayItem1(Backslot, characterData.Back, "Back");
        DisplayItem1(Maskslot, characterData.Mask, "Mask");
        DisplayItem1(Glassesslot, characterData.Glasses, "Glasses");
        DisplayItem1(Shieldslot, characterData.Shield, "Shield");

        // Cập nhật chỉ số
        var player = GameObject.FindWithTag("Player");
        if (player != null)
        {
            var statComp = player.GetComponent<CharacterStats>();
            if (statComp != null)
            {
                statComp.RecalculateStatsFromEquipment(equippedItems);
            }
        }
    }





    public void DisplayItem1(GameObject slot, string itemPath, string expectedType = null)
    {
        if (slot == null || string.IsNullOrEmpty(itemPath)) return;

        string id = itemPath.Split('#')[0].Trim();

        TextMeshProUGUI tmpText = slot.GetComponentInChildren<TextMeshProUGUI>();
        if (tmpText != null)
            tmpText.text = id;

        Image img = slot.GetComponentInChildren<Image>();
        if (img != null)
        {
            var icon = IconCollection.Active.FindIconItem(id, expectedType);

            if (icon != null)
            {
                img.sprite = icon.Sprite;
                img.color = Color.white;

                var eqSlot = slot.GetComponent<EquipmentSlotUI>();
                if (eqSlot != null)
                {
                    eqSlot.SetItem(id, icon.Sprite, icon.Type);
                }

                // THÊM DÒNG NÀY để cộng stats:
                string itemId = icon.Id.Split('.').Last(); // hoặc giữ nguyên icon.Id nếu bạn dùng full ID
                string itemType = icon.Type;
                var stats = ItemDatabase.Instance.GetItemStatsById(itemId, itemType);
                if (stats != null)
                {
                    equippedItems.Add(stats);
                    Debug.Log($"[STAT ADDED] {itemId} ({itemType}) ➜ STR: {stats.Strength}, DEF: {stats.Defense}, AGI: {stats.Agility}, VIT: {stats.Vitality}");

                }
                if (character != null && stats != null)
                {
                    EquipToCharacterFromStats(stats);
                }


            }
            else
            {
                img.sprite = IconCollection.Active.DefaultItemIcon;
                img.color = Color.gray;
            }
        }
    }
    //hàm gỡ icon mặc định
    public void ClearSlot(GameObject slot)
    {
        if (slot == null) return;

        Image img = slot.GetComponentInChildren<Image>();
        if (img != null)
        {
            img.sprite = IconCollection.Active.DefaultItemIcon;
            img.color = Color.gray;
        }

        TextMeshProUGUI tmpText = slot.GetComponentInChildren<TextMeshProUGUI>();
        if (tmpText != null)
        {
            tmpText.text = "";
        }

        var eqSlot = slot.GetComponent<EquipmentSlotUI>();
        if (eqSlot != null)
        {
            eqSlot.SetItem("", IconCollection.Active.DefaultItemIcon, "");
        }
    }
    //gỡ






    public void DisplayItem(GameObject slot, string itemPath, string expectedType = null)
    {
        if (slot == null) return;

        Image img = slot.GetComponentInChildren<Image>();
        if (img == null) return;

        // Nếu không có item, gán icon mặc định
        if (string.IsNullOrEmpty(itemPath))
        {
            img.sprite = IconCollection.Active.DefaultItemIcon;
            img.color = Color.gray;
            var eqSlot = slot.GetComponent<EquipmentSlotUI>();
            if (eqSlot != null)
            {
                eqSlot.SetItem("", IconCollection.Active.DefaultItemIcon, "");
            }
            TextMeshProUGUI tmpText = slot.GetComponentInChildren<TextMeshProUGUI>();
            if (tmpText != null)
                tmpText.text = "";
            return;
        }

        string raw = itemPath.Split('#')[0].Trim();  // Loại bỏ màu
        string name = raw.Split('.').Last();         // Lấy tên item

        string[] collections = {
        "Extensions.Legendary",
        "FantasyHeroes.Basic",
        "Extensions.Epic",
        "Extensions.Epic",
        "FantasyHeroes.Samurai",
        "Extensions.AbandonedWorkshop",
        "Extensions.Legendary",
        "UndeadHeroes.Undead",
        "Extensions.Epic",
        "Extensions.MoonStyle [NoPaint]",

    };

        TextMeshProUGUI tmpText2 = slot.GetComponentInChildren<TextMeshProUGUI>();
        if (tmpText2 != null)
            tmpText2.text = name;

        var icon = FindIconWithFallback(collections, expectedType, name);

        if (icon != null)
        {
            img.sprite = icon.Sprite;
            img.color = Color.white;

            var eqSlot = slot.GetComponent<EquipmentSlotUI>();
            if (eqSlot != null)
            {
                eqSlot.SetItem(icon.Id, icon.Sprite, icon.Type);
            }

            string itemId = icon.Id.Split('.').Last();
            string itemType = icon.Type;
            var stats = ItemDatabase.Instance.GetItemStatsById(itemId, itemType);
            if (stats != null)
            {
                equippedItems.Add(stats);
                Debug.Log($"[STAT ADDED] {itemId} ({itemType}) ➜ " +
                 $"STR: {stats.Strength}, DEF: {stats.Defense}, AGI: {stats.Agility}, VIT: {stats.Vitality}");
            }
        }
        else
        {
            img.sprite = IconCollection.Active.DefaultItemIcon;
            img.color = Color.gray;
        }
    }


    ItemIcon FindIconWithFallback(string[] collections, string type, string name)
    {
        foreach (var collection in collections)
        {
            string id = $"{collection}.{type}.{name}";
            var icon = IconCollection.Active.Icons
                .Where(i => i.Type == type)
                .FirstOrDefault(i => i.Id == id);

            if (icon != null)
            {
                // Debug.Log($" Đã tìm thấy icon với ID: {id}");
                return icon;
            }
        }

        //    Debug.LogWarning($" Không tìm thấy icon: {type}.{name} trong bất kỳ bộ nào.");
        return null;
    }


    //hàm đọc dữ liện index của []armor
    string GetArmorValue(string json, int index)
    {
        string key = $"\"Armor[{index}]\":\"";
        int start = json.IndexOf(key);
        if (start == -1) return null;
        start += key.Length;
        int end = json.IndexOf("\"", start);
        if (end == -1) return null;
        return json.Substring(start, end - start);
    }
    private void EquipToCharacterFromStats(ItemStats stats)
    {
        string spriteName = stats.itemId.Split('.').Last();
        Sprite sprite = stats.Icon;

        if (string.IsNullOrEmpty(spriteName) || sprite == null)
        {
            Debug.LogWarning("EquipToCharacter: sprite null hoặc tên trống.");
            return;
        }

        switch (stats.Type)
        {
            case "Helmet":
                character.Helmet = sprite;
                break;
            case "Glasses":
                character.Glasses = sprite;
                break;
            case "Hair":
                character.Hair = sprite;
                break;
            case "Back":
                character.Back = sprite;
                break;
            case "Cape":
                character.Cape = sprite;
                break;
            case "Shield":
                character.Shield = sprite;
                break;
            case "Armor":
                EnsureArmorListSize(0);
                character.Armor[0] = sprite;
                break;
            case "Boots":
                EnsureArmorListSize(1);
                character.Armor[1] = sprite;
                break;
            case "Gloves":
                EnsureArmorListSize(2);
                character.Armor[2] = sprite;
                break;
            case "Pauldrons":
                EnsureArmorListSize(3);
                character.Armor[3] = sprite;
                break;
            case "Vest":
                EnsureArmorListSize(4);
                character.Armor[4] = sprite;
                break;
            case "Belt":
                EnsureArmorListSize(5);
                character.Armor[5] = sprite;
                break;

            case "MeleeWeapon1H":
            case "PrimaryMeleeWeapon":
                EquipWeapon(sprite, WeaponType.Melee1H);
                break;
            case "MeleeWeapon2H":
            case "SecondaryMeleeWeapon":
                EquipWeapon(sprite, WeaponType.Melee2H);
                break;

            case "Bow":
                EquipWeapon(sprite, WeaponType.Bow);
                break;

            case "Firearms1H":
                EquipWeapon(sprite, WeaponType.Firearms1H);
                break;

            case "Firearms2H":
                EquipWeapon(sprite, WeaponType.Firearms2H);
                break;

            default:
                Debug.LogWarning($"Chưa hỗ trợ trang bị: {stats.Type}");
                break;
        }

        character.Initialize();
    }
    private void EquipWeapon(Sprite sprite, WeaponType type)
    {
        if (sprite == null) return;

        var entry = new HeroEditor.Common.SpriteGroupEntry(
            edition: "Custom",
            collection: "Default",
            type: type.ToString(),
            name: sprite.name,
            path: "",
            sprite: sprite,
            sprites: new List<Sprite> { sprite }
        );

        character.WeaponType = type;
        character.Equip(entry, EquipmentPart.MeleeWeapon1H);
    }
    private void EnsureArmorListSize(int index)
    {
        while (character.Armor.Count <= index)
        {
            character.Armor.Add(null);
        }
    }
    public void RefreshFromLatestJson()
    {
        if (string.IsNullOrEmpty(PlayerDataHolder1.CharacterJson)) return;

        character.FromJson(PlayerDataHolder1.CharacterJson);

        // Ép lại các phần giáp
        StartCoroutine(EquipAllArmorAfterJson());

        //  NEW: Ép lại vũ khí Melee2H nếu đang dùng
        string melee2HId = GetItemIdFromJson(PlayerDataHolder1.CharacterJson, "SecondaryMeleeWeapon");
        if (!string.IsNullOrEmpty(melee2HId))
        {
            var stats = ItemDatabase.Instance.GetItemStatsById(melee2HId.Split('.').Last(), "MeleeWeapon2H");
            if (stats != null)
            {
                var entry = character.SpriteCollection.MeleeWeapon2H.FirstOrDefault(e => e.Id == stats.itemId);
                if (entry != null)
                {
                    character.WeaponType = WeaponType.Melee2H;
                    character.Equip(entry, EquipmentPart.MeleeWeapon2H);
                    Debug.Log(" Đã ép lại MeleeWeapon2H sau khi load JSON.");
                }
                else
                {
                    Debug.LogWarning($"⚠ Không tìm thấy entry MeleeWeapon2H với ID: {stats.itemId}");
                }
            }
        }

        LoadCharacterToUI(); // Cập nhật UI
    }

    public IEnumerator EquipAllArmorAfterJson()
    {
        yield return null; // Đợi 1 frame để SpriteCollection sẵn sàng

        string[] armorTypes = { "Armor", "Boots", "Gloves", "Pauldrons", "Vest", "Belt" };

        foreach (var type in armorTypes)
        {
            string id = GetItemIdFromJson(PlayerDataHolder1.CharacterJson, type);
            if (!string.IsNullOrEmpty(id))
            {
                var stats = ItemDatabase.Instance.GetItemStatsById(id.Split('.').Last(), type);
                if (stats != null && stats.Icon != null)
                {
                    CharacterEquipHandler.EquipPartialArmor(character, type, stats.Icon);
                }
            }
        }

        Debug.Log(" Đã ép lại toàn bộ Armor sau khi load JSON.");
    }
    public string GetItemIdFromJson(string json, string key)
    {
        if (string.IsNullOrEmpty(json) || string.IsNullOrEmpty(key)) return null;
        var dict = JsonConvert.DeserializeObject<Dictionary<string, string>>(json);
        if (dict.ContainsKey(key)) return dict[key];
        return null;
    }
    //lấy dữ liệu chỉ số đô
    public void UpdateCharacterStatsAndUI()
    {
        var player = GameObject.FindWithTag("Player");
        if (player != null)
        {
            var statComp = player.GetComponent<CharacterStats>();
            if (statComp != null)
            {
                var equipped = GetAllEquippedItems(PlayerDataHolder1.CharacterJson);
                statComp.RecalculateStatsFromEquipment(equipped);
            }
        }
        StartCoroutine(UpdateStatsUIDelayed());
    }

    private IEnumerator UpdateStatsUIDelayed()
    {
        yield return null;
        ThongTin.instance?.UpdateStatsUI();
    }

    public List<ItemStats> GetAllEquippedItems(string json)
    {
        var result = new List<ItemStats>();
        if (string.IsNullOrEmpty(json)) return result;

        var dict = Newtonsoft.Json.JsonConvert.DeserializeObject<Dictionary<string, string>>(json);
        foreach (var kv in dict)
        {
            if (string.IsNullOrEmpty(kv.Value)) continue;
            string itemId = kv.Value.Split('.').Last();
            string itemType = kv.Key;

            // Nếu bạn có mapping type thì thay đổi cho phù hợp (ví dụ "PrimaryMeleeWeapon" => "MeleeWeapon1H")
            var stats = ItemDatabase.Instance.GetItemStatsById(itemId, itemType);
            if (stats != null) result.Add(stats);
        }
        return result;
    }


}
