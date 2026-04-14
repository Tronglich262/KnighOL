using Assets.HeroEditor.Common.CharacterScripts;
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

    private readonly List<ItemStats> equippedItems = new(16);
    private CharacterEquipmentPresenter presenter;

    private int lastGold = int.MinValue;
    private int lastDiamond = int.MinValue;

    private void Awake()
    {
        Instance = this;

        presenter = new CharacterEquipmentPresenter(
            character,
            equippedItems,
            Helmetslot,
            ArmorSlots,
            Vestslot,
            Pauldronsslot,
            Glovesslot,
            Bootslot,
            Bowslot,
            Hairslot,
            Beltslot,
            Capeslot,
            Backslot,
            Maskslot,
            Glassesslot,
            Shieldslot,
            ArmorGeneralSlot,
            MeleeWeapon1Hslot,
            MeleeWeapon2Hslot
        );
    }

    private void Start()
    {
        RefreshFromLatestJson();
        RefreshCurrencyUI(force: true);
    }

    private void LateUpdate()
    {
        RefreshCurrencyUI();
    }

    private void RefreshCurrencyUI(bool force = false)
    {
        var state = PlayerDataHolder1.CurrentPlayerState;
        if (state == null) return;

        if (force || state.gold != lastGold)
        {
            lastGold = state.gold;
            if (gold != null) gold.text = lastGold.ToString();
        }

        if (force || state.diamond != lastDiamond)
        {
            lastDiamond = state.diamond;
            if (diamond != null) diamond.text = lastDiamond.ToString();
        }
    }

    public void RefreshFromLatestJson()
    {
        string json = PlayerDataHolder1.CharacterJson;
        if (string.IsNullOrEmpty(json) || character == null)
            return;

        presenter.LoadFromJson(json, applyVisual: true);
        RecalculateStats();
    }

    private void RecalculateStats()
    {
        if (PlayerSpawner.LocalPlayerObject == null) return;

        var statComp = PlayerSpawner.LocalPlayerObject.GetComponent<CharacterStats>();
        if (statComp != null)
            statComp.RecalculateStatsFromEquipment(equippedItems);
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
    public void LoadCharacterToUI()
    {
        RefreshFromLatestJson();
    }

    public string GetItemIdFromJson(string json, string key)
    {
        if (string.IsNullOrEmpty(json) || string.IsNullOrEmpty(key))
            return null;

        Dictionary<string, string> dict = JsonConvert.DeserializeObject<Dictionary<string, string>>(json);
        if (dict == null)
            return null;

        return dict.TryGetValue(key, out string value) ? value : null;
    }

    public void ClearSlot(GameObject slot)
    {
        CharacterEquipmentHelper.ClearSlotUI(slot);
    }

    public void DisplayItem(GameObject slot, string itemPath, string expectedType = null)
    {
        var stats = CharacterSlotRenderer.DisplaySlot(slot, itemPath, expectedType);
        if (stats == null) return;

        equippedItems.Add(stats);
    }

    public void DisplayItem1(GameObject slot, string itemPath, string expectedType = null)
    {
        var stats = CharacterSlotRenderer.DisplaySlot(slot, itemPath, expectedType);
        if (stats == null) return;

        equippedItems.Add(stats);
        ApplyVisualFromStats(stats);
    }

    private void ApplyVisualFromStats(ItemStats stats)
    {
        if (stats == null || stats.Icon == null || character == null)
            return;

        switch (stats.Type)
        {
            case "Helmet": character.Helmet = stats.Icon; break;
            case "Glasses": character.Glasses = stats.Icon; break;
            case "Hair": character.Hair = stats.Icon; break;
            case "Back": character.Back = stats.Icon; break;
            case "Cape": character.Cape = stats.Icon; break;
            case "Shield": character.Shield = stats.Icon; break;

            case "Armor":
                EnsureArmorSize(0);
                character.Armor[0] = stats.Icon;
                break;

            case "Boots":
                EnsureArmorSize(1);
                character.Armor[1] = stats.Icon;
                break;

            case "Gloves":
                EnsureArmorSize(2);
                character.Armor[2] = stats.Icon;
                break;

            case "Pauldrons":
                EnsureArmorSize(3);
                character.Armor[3] = stats.Icon;
                break;

            case "Vest":
                EnsureArmorSize(4);
                character.Armor[4] = stats.Icon;
                break;

            case "Belt":
                EnsureArmorSize(5);
                character.Armor[5] = stats.Icon;
                break;
            case "MeleeWeapon1H":
                {
                    var entry = character.SpriteCollection.MeleeWeapon1H.FirstOrDefault(e => e.Id == stats.itemId);
                    if (entry != null)
                    {
                        character.WeaponType = WeaponType.Melee1H;
                        character.Equip(entry, EquipmentPart.MeleeWeapon1H);
                    }
                    break;
                }
            case "MeleeWeapon2H":
                {
                    var entry = character.SpriteCollection.MeleeWeapon2H.FirstOrDefault(e => e.Id == stats.itemId);
                    if (entry != null)
                    {
                        character.WeaponType = WeaponType.Melee2H;
                        character.Equip(entry, EquipmentPart.MeleeWeapon2H);
                    }
                    break;
                }
            case "Bow":
                {
                    var entry = character.SpriteCollection.Bow.FirstOrDefault(e => e.Id == stats.itemId);
                    if (entry != null)
                    {
                        character.WeaponType = WeaponType.Bow;
                        character.Equip(entry, EquipmentPart.Bow);
                    }
                    break;
                }
        }
    }

    private void EnsureArmorSize(int index)
    {
        if (character == null) return;

        while (character.Armor.Count <= index)
            character.Armor.Add(null);
    }

}