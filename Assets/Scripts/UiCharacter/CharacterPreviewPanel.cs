using Assets.HeroEditor.Common.CharacterScripts;
using Assets.HeroEditor.Common.CommonScripts;
using HeroEditor.Common.Enums;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using HeroEditor.Common;

public class CharacterPreviewPanel : MonoBehaviour
{
    public GameObject Helmetslot;
    public GameObject[] ArmorSlots;
    public GameObject Vestslot;
    public GameObject Pauldronsslot;
    public GameObject Glovesslot;
    public GameObject Firearms1Hslot;
    public GameObject Firearms2Hslot;
    public GameObject Bowslot;
    public GameObject Hairslot;
    public GameObject Beltslot;
    public GameObject Capeslot;
    public GameObject Backslot;
    public GameObject Maskslot;
    public GameObject Glassesslot;
    public GameObject Shieldslot;
    public GameObject Bootslot;
    public GameObject ArmorGeneralSlot;
    public GameObject MeleeWeapon1Hslot;
    public GameObject MeleeWeapon2Hslot;

    private List<ItemStats> equippedItems = new List<ItemStats>();

    public static CharacterPreviewPanel Instance;
    public Character characterPreview; // Nhân vật trên panel preview
    private string _currentPreviewJson = null;

    private void Awake()
    {
        Instance = this;

        // LUÔN tìm GameObject tên "ClonePreview" ở trong scene (dù panel là prefab)
        GameObject clonePreviewObj = GameObject.Find("ClonePreview");
        if (clonePreviewObj != null)
        {
            characterPreview = clonePreviewObj.GetComponent<Character>();
            Debug.Log("[PREVIEW] Đã gán ClonePreview từ scene: " + characterPreview.gameObject.name);
        }
        else
        {
            Debug.LogError("[PREVIEW] Không tìm thấy ClonePreview trong scene!");
        }
    }


    void Start()
    {
        gameObject.SetActive(false);
    }

    /// <summary>
    /// Hàm này được gọi khi bạn click vào một player bất kỳ để show preview!
    /// </summary>
    public void LoadCharacterFromJson(string json)
    {
        Debug.Log($"[PREVIEW] LoadCharacterFromJson: {json?.Substring(0, Mathf.Min(50, json.Length))}");
        if (string.IsNullOrEmpty(json)) return;
        _currentPreviewJson = json;

        equippedItems.Clear();
        ClearAllSlots();

        if (characterPreview != null)
        {
            characterPreview.Armor.Clear();
            characterPreview.Helmet = null;
            characterPreview.Glasses = null;
            characterPreview.Hair = null;
            characterPreview.Back = null;
            characterPreview.Cape = null;
            characterPreview.Shield = null;
            characterPreview.PrimaryMeleeWeapon = null;
            characterPreview.SecondaryMeleeWeapon = null;
            characterPreview.Firearms = null;
            characterPreview.Bow = null;

            characterPreview.FromJson(json);
            characterPreview.Initialize();

            // ---- BỔ SUNG ĐÚNG CHUẨN PHẦN NÀY ----
            string[] mixTypes = { "Boots", "Gloves", "Belt", "Pauldrons", "Vest" };
            var dict = JsonConvert.DeserializeObject<Dictionary<string, string>>(json);

            foreach (var type in mixTypes)
            {
                if (dict.TryGetValue(type, out string partId) && !string.IsNullOrEmpty(partId))
                {
                    CharacterEquipHandler.EquipPartialArmorFromEntry(characterPreview, partId, type);
                }
            }
            if (dict.TryGetValue("WeaponType", out string weaponType))
            {
                if (weaponType == "Melee2H" && dict.TryGetValue("SecondaryMeleeWeapon", out string weaponId) && !string.IsNullOrEmpty(weaponId))
                {
                    // Load MeleeWeapon2H đúng chuẩn
                    var entry = characterPreview.SpriteCollection.MeleeWeapon2H.FirstOrDefault(e => e.Id == weaponId);
                    if (entry != null)
                    {
                        characterPreview.WeaponType = WeaponType.Melee2H;
                        characterPreview.Equip(entry, EquipmentPart.MeleeWeapon2H);
                    }
                }
            }
                    characterPreview.Initialize(); // Gọi lại để cập nhật hoàn chỉnh visual
                                           // ---- HẾT BỔ SUNG ----
        }

        else
        {
            Debug.LogError("[PREVIEW] characterPreview bị NULL!");
            return;
        }



        LoadCharacterToUI(); // Update UI luôn!
    }

    /// <summary>
    /// Hàm này tự động dùng _currentPreviewJson, KHÔNG BAO GIỜ DÙNG PlayerDataHolder1!
    /// </summary>
    public void LoadCharacterToUI()
    {
        string json = _currentPreviewJson;
        Debug.Log("[PREVIEW] LoadCharacterToUI: " + (json == null ? "NULL" : json.Substring(0, Mathf.Min(50, json.Length))));
        if (string.IsNullOrEmpty(json)) return;
        CharacterData characterData = JsonUtility.FromJson<CharacterData>(json);
        string[] armorTypes = { "Armor", "Boots", "Gloves", "Pauldrons", "Vest", "Belt" };
        for (int i = 0; i < ArmorSlots.Length && i < armorTypes.Length; i++)
        {
            string armorValue = GetArmorValue(json, i);
            string expectedType = armorTypes[i];
            DisplayItem(ArmorSlots[i], armorValue, expectedType);
        }
        string fullArmorId = GetItemIdFromJson(json, "Armor");
        if (!string.IsNullOrEmpty(fullArmorId))
        {
            DisplayItem1(ArmorGeneralSlot, fullArmorId, "Armor");
        }
        DisplayItem1(Helmetslot, characterData.Helmet, "Helmet");
        DisplayItem1(MeleeWeapon1Hslot, characterData.PrimaryMeleeWeapon, "PrimaryMeleeWeapon");
        DisplayItem1(MeleeWeapon2Hslot, characterData.SecondaryMeleeWeapon, "SecondaryMeleeWeapon");
        DisplayItem1(Firearms1Hslot, characterData.Firearms1H, "Firearms1H");
        DisplayItem1(Firearms2Hslot, characterData.Firearms2H, "Firearms2H");
        if (!string.IsNullOrEmpty(characterData.Bow)) DisplayItem1(Bowslot, characterData.Bow, "Bow");
        DisplayItem1(Hairslot, characterData.Hair, "Hair");
        DisplayItem1(Pauldronsslot, characterData.Pauldrons, "Pauldrons");
        DisplayItem1(Bootslot, characterData.Boots, "Boots");
        DisplayItem1(Beltslot, characterData.Belt, "Belt");
        DisplayItem1(Glovesslot, characterData.Gloves, "Gloves");
        DisplayItem1(Vestslot, characterData.Vest, "Vest");
        DisplayItem1(Capeslot, characterData.Cape, "Cape");
        DisplayItem1(Backslot, characterData.Back, "Back");
        DisplayItem1(Maskslot, characterData.Mask, "Mask");
        DisplayItem1(Glassesslot, characterData.Glasses, "Glasses");
        DisplayItem1(Shieldslot, characterData.Shield, "Shield");
    }

    public void ClearAllSlots()
    {
        ClearSlot(Helmetslot);
        foreach (var s in ArmorSlots) ClearSlot(s);
        ClearSlot(Vestslot); ClearSlot(Pauldronsslot); ClearSlot(Glovesslot); ClearSlot(Firearms1Hslot);
        ClearSlot(Firearms2Hslot); ClearSlot(Bowslot); ClearSlot(Hairslot); ClearSlot(Beltslot);
        ClearSlot(Capeslot); ClearSlot(Backslot); ClearSlot(Maskslot); ClearSlot(Glassesslot);
        ClearSlot(Shieldslot); ClearSlot(Bootslot); ClearSlot(ArmorGeneralSlot);
        ClearSlot(MeleeWeapon1Hslot); ClearSlot(MeleeWeapon2Hslot);
    }

    public void DisplayItem1(GameObject slot, string itemPath, string expectedType = null)
    {
        if (slot == null || string.IsNullOrEmpty(itemPath)) return;

        string id = itemPath.Split('#')[0].Trim();
        TextMeshProUGUI tmpText = slot.GetComponentInChildren<TextMeshProUGUI>();
        if (tmpText != null) tmpText.text = id;

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

                // Cộng chỉ số
                string itemId = icon.Id.Split('.').Last();
                string itemType = icon.Type;
                var stats = ItemDatabase.Instance.GetItemStatsById(itemId, itemType);
                if (stats != null)
                {
                    equippedItems.Add(stats);
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

    public void DisplayItem(GameObject slot, string itemPath, string expectedType = null)
    {
        if (slot == null) return;

        Image img = slot.GetComponentInChildren<Image>();
        if (img == null) return;

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
            if (tmpText != null) tmpText.text = "";
            return;
        }

        string raw = itemPath.Split('#')[0].Trim();
        string name = raw.Split('.').Last();

        string[] collections = {
            "Extensions.Legendary",
            "FantasyHeroes.Basic",
            "Extensions.Epic",
            "FantasyHeroes.Samurai",
            "Extensions.AbandonedWorkshop",
            "UndeadHeroes.Undead",
            "Extensions.MoonStyle [NoPaint]"
        };

        TextMeshProUGUI tmpText2 = slot.GetComponentInChildren<TextMeshProUGUI>();
        if (tmpText2 != null) tmpText2.text = name;

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
            if (icon != null) return icon;
        }
        return null;
    }

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
        if (string.IsNullOrEmpty(spriteName) || sprite == null) return;

        switch (stats.Type)
        {
            case "Helmet": characterPreview.Helmet = sprite; break;
            case "Glasses": characterPreview.Glasses = sprite; break;
            case "Hair": characterPreview.Hair = sprite; break;
            case "Back": characterPreview.Back = sprite; break;
            case "Cape": characterPreview.Cape = sprite; break;
            case "Shield": characterPreview.Shield = sprite; break;
            case "Armor": EnsureArmorListSize(0); characterPreview.Armor[0] = sprite; break;
            case "Boots": EnsureArmorListSize(1); characterPreview.Armor[1] = sprite; break;
            case "Gloves": EnsureArmorListSize(2); characterPreview.Armor[2] = sprite; break;
            case "Pauldrons": EnsureArmorListSize(3); characterPreview.Armor[3] = sprite; break;
            case "Vest": EnsureArmorListSize(4); characterPreview.Armor[4] = sprite; break;
            case "Belt": EnsureArmorListSize(5); characterPreview.Armor[5] = sprite; break;
            case "MeleeWeapon1H":
            case "PrimaryMeleeWeapon": EquipWeapon(sprite, WeaponType.Melee1H); break;
            case "MeleeWeapon2H":
            case "SecondaryMeleeWeapon": EquipWeapon(sprite, WeaponType.Melee2H); break;
            case "Bow": EquipWeapon(sprite, WeaponType.Bow); break;
            case "Firearms1H": EquipWeapon(sprite, WeaponType.Firearms1H); break;
            case "Firearms2H": EquipWeapon(sprite, WeaponType.Firearms2H); break;
            default: break;
        }
        characterPreview.Initialize();
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

        characterPreview.WeaponType = type;
        characterPreview.Equip(entry, EquipmentPart.MeleeWeapon1H); // <--- CHỈ LUÔN LUÔN LÀ MeleeWeapon1H
    }


    private void EnsureArmorListSize(int index)
    {
        while (characterPreview.Armor.Count <= index)
        {
            characterPreview.Armor.Add(null);
        }
    }

    public string GetItemIdFromJson(string json, string key)
    {
        if (string.IsNullOrEmpty(json) || string.IsNullOrEmpty(key)) return null;
        var dict = JsonConvert.DeserializeObject<Dictionary<string, string>>(json);
        if (dict.ContainsKey(key)) return dict[key];
        return null;
    }

    public void ClearPreviewData()
    {
        ClearAllSlots();
        equippedItems.Clear();
        _currentPreviewJson = null;
    }
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

}
