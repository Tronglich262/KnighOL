using Assets.HeroEditor.Common.CharacterScripts;
using Assets.HeroEditor.Common.CommonScripts;
using HeroEditor.Common.Enums;
using Newtonsoft.Json;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class CharacterUIManager1 : MonoBehaviour
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

    public Character character;
    public Text gold;
    public Text diamond;

    public static CharacterUIManager1 Instance;

    private readonly List<ItemStats> equippedItems = new List<ItemStats>(16);
    private CharacterData _cachedData;
    private Dictionary<string, string> _cachedDict;

    private void Awake()
    {
        Instance = this;
    }

    private void Start()
    {
        if (!string.IsNullOrEmpty(PlayerDataHolder1.CharacterJson) && character != null)
        {
            LoadCharacterToUI();
        }
    }

    private void Update()
    {
        var state = PlayerDataHolder1.CurrentPlayerState;
        if (state == null) return;

        if (gold != null) gold.text = state.gold.ToString();
        if (diamond != null) diamond.text = state.diamond.ToString();
    }

    public void LoadCharacterToUI()
    {
        string json = PlayerDataHolder1.CharacterJson;
        if (string.IsNullOrEmpty(json))
        {
            Debug.LogWarning("CharacterJson rỗng.");
            return;
        }

        LoadCharacterFromJson(json);
    }

    public void LoadCharacterFromJson(string json)
    {
        if (string.IsNullOrEmpty(json) || character == null)
            return;

        _cachedData = JsonUtility.FromJson<CharacterData>(json);
        _cachedDict = JsonConvert.DeserializeObject<Dictionary<string, string>>(json) ?? new Dictionary<string, string>();

        equippedItems.Clear();
        ClearAllSlots();
        ResetLocalCharacterVisual();

        character.FromJson(json);

        for (int i = 0; i < CharacterEquipmentHelper.PartialArmorTypes.Length; i++)
        {
            string type = CharacterEquipmentHelper.PartialArmorTypes[i];
            string id = CharacterEquipmentHelper.GetValue(_cachedDict, type);
            if (!string.IsNullOrEmpty(id))
            {
                CharacterEquipHandler.EquipPartialArmorFromEntry(character, id, type);
            }
        }

        ApplyWeaponFromData();
        character.Initialize();

        DisplayArmorSlots();
        DisplayEquipmentSlots();
        RecalculateStats();
    }

    private void ApplyWeaponFromData()
    {
        if (_cachedData == null || string.IsNullOrEmpty(_cachedData.WeaponType))
            return;

        switch (_cachedData.WeaponType)
        {
            case "Melee1H":
                {
                    string weaponId = !string.IsNullOrEmpty(_cachedData.MeleeWeapon1H)
                        ? _cachedData.MeleeWeapon1H
                        : _cachedData.PrimaryMeleeWeapon;

                    if (string.IsNullOrEmpty(weaponId)) return;

                    var entry = character.SpriteCollection.MeleeWeapon1H.FirstOrDefault(e => e.Id == weaponId);
                    if (entry != null)
                    {
                        character.WeaponType = WeaponType.Melee1H;
                        character.Equip(entry, EquipmentPart.MeleeWeapon1H);
                    }
                    break;
                }

            case "Melee2H":
                {
                    string weaponId = ResolveMelee2H();
                    if (string.IsNullOrEmpty(weaponId)) return;

                    var entry = character.SpriteCollection.MeleeWeapon2H.FirstOrDefault(e => e.Id == weaponId);
                    if (entry != null)
                    {
                        character.WeaponType = WeaponType.Melee2H;
                        character.Equip(entry, EquipmentPart.MeleeWeapon2H);
                    }
                    break;
                }

            case "Bow":
                {
                    if (string.IsNullOrEmpty(_cachedData.Bow)) return;

                    var entry = character.SpriteCollection.Bow.FirstOrDefault(e => e.Id == _cachedData.Bow);
                    if (entry != null)
                    {
                        character.WeaponType = WeaponType.Bow;
                        character.Equip(entry, EquipmentPart.Bow);
                    }
                    break;
                }
        }
    }

    private void DisplayArmorSlots()
    {
        for (int i = 0; i < ArmorSlots.Length && i < CharacterEquipmentHelper.ArmorTypes.Length; i++)
        {
            string type = CharacterEquipmentHelper.ArmorTypes[i];
            string value = GetArmorDisplayValue(type);
            DisplaySlot(ArmorSlots[i], value, type, false, true);
        }

        string fullArmor = _cachedData.Armor != null && _cachedData.Armor.Length > 0
            ? _cachedData.Armor[0]
            : CharacterEquipmentHelper.GetValue(_cachedDict, "Armor");

        if (!string.IsNullOrEmpty(fullArmor))
            DisplaySlot(ArmorGeneralSlot, fullArmor, EquipKeys.Armor, true, true);
    }

    private void DisplayEquipmentSlots()
    {
        DisplaySlot(Helmetslot, _cachedData.Helmet, EquipKeys.Helmet, true, true);
        DisplaySlot(MeleeWeapon1Hslot, ResolveMelee1H(), EquipKeys.MeleeWeapon1H, false, true);
        DisplaySlot(MeleeWeapon2Hslot, ResolveMelee2H(), EquipKeys.MeleeWeapon2H, false, true);
        DisplaySlot(Firearms1Hslot, _cachedData.Firearms1H, EquipKeys.Firearms1H, false, true);
        DisplaySlot(Firearms2Hslot, _cachedData.Firearms2H, EquipKeys.Firearms2H, false, true);
        DisplaySlot(Bowslot, _cachedData.Bow, EquipKeys.Bow, false, true);
        DisplaySlot(Hairslot, _cachedData.Hair, EquipKeys.Hair, true, true);
        DisplaySlot(Pauldronsslot, _cachedData.Pauldrons, EquipKeys.Pauldrons, true, true);
        DisplaySlot(Bootslot, _cachedData.Boots, EquipKeys.Boots, true, true);
        DisplaySlot(Beltslot, _cachedData.Belt, EquipKeys.Belt, true, true);
        DisplaySlot(Glovesslot, _cachedData.Gloves, EquipKeys.Gloves, true, true);
        DisplaySlot(Vestslot, _cachedData.Vest, EquipKeys.Vest, true, true);
        DisplaySlot(Capeslot, _cachedData.Cape, EquipKeys.Cape, true, true);
        DisplaySlot(Backslot, _cachedData.Back, EquipKeys.Back, true, true);
        DisplaySlot(Maskslot, _cachedData.Mask, EquipKeys.Mask, true, true);
        DisplaySlot(Glassesslot, _cachedData.Glasses, EquipKeys.Glasses, true, true);
        DisplaySlot(Shieldslot, _cachedData.Shield, EquipKeys.Shield, true, true);
    }

    private string GetArmorDisplayValue(string type)
    {
        switch (type)
        {
            case "Armor":
                return _cachedData.Armor != null && _cachedData.Armor.Length > 0 ? _cachedData.Armor[0] : null;
            case "Boots":
                return _cachedData.Boots;
            case "Gloves":
                return _cachedData.Gloves;
            case "Pauldrons":
                return _cachedData.Pauldrons;
            case "Vest":
                return _cachedData.Vest;
            case "Belt":
                return _cachedData.Belt;
            default:
                return null;
        }
    }

    private string ResolveMelee1H()
    {
        if (!string.IsNullOrEmpty(_cachedData.MeleeWeapon1H))
            return _cachedData.MeleeWeapon1H;

        if (_cachedData.WeaponType == "Melee1H")
            return _cachedData.PrimaryMeleeWeapon;

        return null;
    }

    private string ResolveMelee2H()
    {
        if (!string.IsNullOrEmpty(_cachedData.MeleeWeapon2H))
            return _cachedData.MeleeWeapon2H;

        if (_cachedData.WeaponType == "Melee2H" && !string.IsNullOrEmpty(_cachedData.PrimaryMeleeWeapon))
            return _cachedData.PrimaryMeleeWeapon;

        return _cachedData.SecondaryMeleeWeapon;
    }

    private void EquipVisualFromStats(ItemStats stats)
    {
        if (stats == null || stats.Icon == null)
            return;

        switch (stats.Type)
        {
            case "Helmet": character.Helmet = stats.Icon; break;
            case "Glasses": character.Glasses = stats.Icon; break;
            case "Hair": character.Hair = stats.Icon; break;
            case "Back": character.Back = stats.Icon; break;
            case "Cape": character.Cape = stats.Icon; break;
            case "Shield": character.Shield = stats.Icon; break;

            case "Armor": EnsureArmorSize(0); character.Armor[0] = stats.Icon; break;
            case "Boots": EnsureArmorSize(1); character.Armor[1] = stats.Icon; break;
            case "Gloves": EnsureArmorSize(2); character.Armor[2] = stats.Icon; break;
            case "Pauldrons": EnsureArmorSize(3); character.Armor[3] = stats.Icon; break;
            case "Vest": EnsureArmorSize(4); character.Armor[4] = stats.Icon; break;
            case "Belt": EnsureArmorSize(5); character.Armor[5] = stats.Icon; break;
        }
    }

    private void EnsureArmorSize(int index)
    {
        while (character.Armor.Count <= index)
            character.Armor.Add(null);
    }

    private void ResetLocalCharacterVisual()
    {
        character.Armor.Clear();

        character.Helmet = null;
        character.Glasses = null;
        character.Hair = null;
        character.Back = null;
        character.Cape = null;
        character.Shield = null;

        character.PrimaryMeleeWeapon = null;
        character.SecondaryMeleeWeapon = null;
        character.Firearms = null;
        character.Bow = null;
    }

    private void RecalculateStats()
    {
        if (PlayerSpawner.LocalPlayerObject == null)
            return;

        var statComp = PlayerSpawner.LocalPlayerObject.GetComponent<CharacterStats>();
        if (statComp != null)
            statComp.RecalculateStatsFromEquipment(equippedItems);
    }

    public void RefreshFromLatestJson()
    {
        if (string.IsNullOrEmpty(PlayerDataHolder1.CharacterJson))
            return;

        LoadCharacterFromJson(PlayerDataHolder1.CharacterJson);
    }

    public void UpdateCharacterStatsAndUI()
    {
        RecalculateStats();
        StartCoroutine(UpdateStatsUIDelayed());
    }

    private IEnumerator UpdateStatsUIDelayed()
    {
        yield return null;
        ThongTin.instance?.UpdateStatsUI();
    }

    private void ClearAllSlots()
    {
        CharacterEquipmentHelper.ClearSlotUI(Helmetslot);

        for (int i = 0; i < ArmorSlots.Length; i++)
            CharacterEquipmentHelper.ClearSlotUI(ArmorSlots[i]);

        CharacterEquipmentHelper.ClearSlotUI(Vestslot);
        CharacterEquipmentHelper.ClearSlotUI(Pauldronsslot);
        CharacterEquipmentHelper.ClearSlotUI(Glovesslot);
        CharacterEquipmentHelper.ClearSlotUI(Bootslot);
        CharacterEquipmentHelper.ClearSlotUI(Firearms1Hslot);
        CharacterEquipmentHelper.ClearSlotUI(Firearms2Hslot);
        CharacterEquipmentHelper.ClearSlotUI(Bowslot);
        CharacterEquipmentHelper.ClearSlotUI(Hairslot);
        CharacterEquipmentHelper.ClearSlotUI(Beltslot);
        CharacterEquipmentHelper.ClearSlotUI(Capeslot);
        CharacterEquipmentHelper.ClearSlotUI(Backslot);
        CharacterEquipmentHelper.ClearSlotUI(Maskslot);
        CharacterEquipmentHelper.ClearSlotUI(Glassesslot);
        CharacterEquipmentHelper.ClearSlotUI(Shieldslot);
        CharacterEquipmentHelper.ClearSlotUI(ArmorGeneralSlot);
        CharacterEquipmentHelper.ClearSlotUI(MeleeWeapon1Hslot);
        CharacterEquipmentHelper.ClearSlotUI(MeleeWeapon2Hslot);
    }
    // =========================

    public string GetItemIdFromJson(string json, string key)
    {
        if (string.IsNullOrEmpty(json) || string.IsNullOrEmpty(key))
            return null;

        Dictionary<string, string> dict = JsonConvert.DeserializeObject<Dictionary<string, string>>(json);
        if (dict == null)
            return null;

        if (dict.TryGetValue(key, out string value))
            return value;

        return null;
    }

    public void ClearSlot(GameObject slot)
    {
        CharacterEquipmentHelper.ClearSlotUI(slot);
    }

    public List<ItemStats> GetAllEquippedItems(string json)
    {
        var result = new List<ItemStats>();
        if (string.IsNullOrEmpty(json))
            return result;

        Dictionary<string, string> dict = JsonConvert.DeserializeObject<Dictionary<string, string>>(json);
        if (dict == null)
            return result;

        foreach (var kv in dict)
        {
            if (string.IsNullOrEmpty(kv.Value))
                continue;

            string itemId = CharacterEquipmentHelper.GetLastToken(kv.Value);
            string itemType = kv.Key;

            var stats = ItemDatabase.Instance.GetItemStatsById(itemId, itemType);
            if (stats != null)
                result.Add(stats);
        }

        return result;
    }
    private void DisplaySlot(
    GameObject slot,
    string itemPath,
    string expectedType,
    bool applyVisual,
    bool addStats)
    {
        if (slot == null)
            return;

        if (string.IsNullOrEmpty(itemPath))
        {
            CharacterEquipmentHelper.ClearSlotUI(slot);
            return;
        }

        string cleanId = CharacterEquipmentHelper.GetCleanId(itemPath);
        string itemName = CharacterEquipmentHelper.GetLastToken(cleanId);

        var icon = IconCollection.Active.FindIconItem(cleanId, expectedType);
        if (icon == null)
            icon = CharacterEquipmentHelper.FindIcon(itemName, expectedType);

        if (icon == null)
        {
            CharacterEquipmentHelper.ClearSlotUI(slot);
            return;
        }

        CharacterEquipmentHelper.SetSlotUI(slot, itemName, icon.Sprite, icon.Id, icon.Type);

        if (!applyVisual && !addStats)
            return;

        ItemStats stats = ItemDatabase.Instance.GetItemStatsById(
            CharacterEquipmentHelper.GetLastToken(icon.Id),
            icon.Type
        );

        if (stats == null)
            return;

        if (addStats)
            equippedItems.Add(stats);

        if (applyVisual)
            EquipVisualFromStats(stats);
    }

    public void DisplayItem(GameObject slot, string itemPath, string expectedType = null)
    {
        DisplaySlot(slot, itemPath, expectedType, false, true);
    }

    public void DisplayItem1(GameObject slot, string itemPath, string expectedType = null)
    {
        DisplaySlot(slot, itemPath, expectedType, true, true);
    }
}