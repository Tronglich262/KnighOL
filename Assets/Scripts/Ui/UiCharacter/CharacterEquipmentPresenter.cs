using Assets.HeroEditor.Common.CharacterScripts;
using HeroEditor.Common.Enums;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class CharacterEquipmentPresenter
{
    private readonly Character character;
    private readonly List<ItemStats> equippedItems;

    private readonly GameObject helmetSlot;
    private readonly GameObject[] armorSlots;
    private readonly GameObject vestSlot;
    private readonly GameObject pauldronsSlot;
    private readonly GameObject glovesSlot;
    private readonly GameObject bootsSlot;
    private readonly GameObject bowSlot;
    private readonly GameObject hairSlot;
    private readonly GameObject beltSlot;
    private readonly GameObject capeSlot;
    private readonly GameObject backSlot;
    private readonly GameObject maskSlot;
    private readonly GameObject glassesSlot;
    private readonly GameObject shieldSlot;
    private readonly GameObject armorGeneralSlot;
    private readonly GameObject meleeWeapon1HSlot;
    private readonly GameObject meleeWeapon2HSlot;

    private CharacterData cachedData;
    private Dictionary<string, string> cachedDict;

    public CharacterEquipmentPresenter(
        Character character,
        List<ItemStats> equippedItems,
        GameObject helmetSlot,
        GameObject[] armorSlots,
        GameObject vestSlot,
        GameObject pauldronsSlot,
        GameObject glovesSlot,
        GameObject bootsSlot,
        GameObject bowSlot,
        GameObject hairSlot,
        GameObject beltSlot,
        GameObject capeSlot,
        GameObject backSlot,
        GameObject maskSlot,
        GameObject glassesSlot,
        GameObject shieldSlot,
        GameObject armorGeneralSlot,
        GameObject meleeWeapon1HSlot,
        GameObject meleeWeapon2HSlot)
    {
        this.character = character;
        this.equippedItems = equippedItems;
        this.helmetSlot = helmetSlot;
        this.armorSlots = armorSlots;
        this.vestSlot = vestSlot;
        this.pauldronsSlot = pauldronsSlot;
        this.glovesSlot = glovesSlot;
        this.bootsSlot = bootsSlot;
        this.bowSlot = bowSlot;
        this.hairSlot = hairSlot;
        this.beltSlot = beltSlot;
        this.capeSlot = capeSlot;
        this.backSlot = backSlot;
        this.maskSlot = maskSlot;
        this.glassesSlot = glassesSlot;
        this.shieldSlot = shieldSlot;
        this.armorGeneralSlot = armorGeneralSlot;
        this.meleeWeapon1HSlot = meleeWeapon1HSlot;
        this.meleeWeapon2HSlot = meleeWeapon2HSlot;
    }

    public void LoadFromJson(string json, bool applyVisual)
    {
        if (string.IsNullOrEmpty(json) || character == null)
            return;

        cachedData = JsonUtility.FromJson<CharacterData>(json);
        cachedDict = JsonConvert.DeserializeObject<Dictionary<string, string>>(json) ?? new Dictionary<string, string>();

        equippedItems.Clear();
        ClearAllSlots();
        ResetCharacterVisual();

        character.FromJson(json);

        foreach (string type in CharacterEquipmentHelper.PartialArmorTypes)
        {
            if (cachedDict.TryGetValue(type, out string id) && !string.IsNullOrEmpty(id))
            {
                CharacterEquipHandler.EquipPartialArmorFromEntry(character, id, type);
            }
        }

        ApplyWeaponFromData();

        DisplayArmorSlots(applyVisual);
        DisplayEquipmentSlots(applyVisual);

        character.Initialize();
    }

    private void ApplyWeaponFromData()
    {
        string weaponType = cachedDict != null && cachedDict.TryGetValue("WeaponType", out var wt) ? wt : cachedData?.WeaponType;

        if (string.IsNullOrEmpty(weaponType))
            return;

        // Bắt lỗi toàn diện các format có thể xảy ra của enum/string
        bool isMelee1H = weaponType == "Melee1H" || weaponType == "0" || weaponType == EquipKeys.Weapon_Melee1H;
        bool isMelee2H = weaponType == "Melee2H" || weaponType == "1" || weaponType == EquipKeys.Weapon_Melee2H;
        bool isBow = weaponType == "Bow" || weaponType == "2" || weaponType == EquipKeys.Weapon_Bow;

        if (isMelee1H)
        {
            EquipMelee1H(ResolveMelee1H());
        }
        else if (isMelee2H)
        {
            EquipMelee2H(ResolveMelee2H());
        }
        else if (isBow)
        {
            string bowId = cachedData != null && !string.IsNullOrEmpty(cachedData.Bow)
                ? cachedData.Bow
                : CharacterEquipmentHelper.GetValue(cachedDict, "Bow");

            EquipBow(bowId);
        }
    }
    private void EquipMelee1H(string id)
    {
        if (string.IsNullOrEmpty(id)) return;

        var entry = character.SpriteCollection.MeleeWeapon1H.FirstOrDefault(e => e.Id == id);
        if (entry == null) return;

        character.WeaponType = WeaponType.Melee1H;
        character.Equip(entry, EquipmentPart.MeleeWeapon1H);
    }

    private void EquipMelee2H(string id)
    {
        if (string.IsNullOrEmpty(id)) return;

        var entry = character.SpriteCollection.MeleeWeapon2H.FirstOrDefault(e => e.Id == id);
        if (entry == null) return;

        character.WeaponType = WeaponType.Melee2H;
        character.Equip(entry, EquipmentPart.MeleeWeapon2H);
    }

    private void EquipBow(string id)
    {
        if (string.IsNullOrEmpty(id)) return;

        var entry = character.SpriteCollection.Bow.FirstOrDefault(e => e.Id == id);
        if (entry == null) return;

        character.WeaponType = WeaponType.Bow;
        character.Equip(entry, EquipmentPart.Bow);
    }

    private void DisplayArmorSlots(bool applyVisual)
    {
        for (int i = 0; i < armorSlots.Length && i < CharacterEquipmentHelper.ArmorTypes.Length; i++)
        {
            string type = CharacterEquipmentHelper.ArmorTypes[i];
            string value = GetArmorDisplayValue(type);
            DisplaySlot(armorSlots[i], value, type, applyVisual, true);
        }

        string fullArmor = cachedData.Armor != null && cachedData.Armor.Length > 0
            ? cachedData.Armor[0]
            : CharacterEquipmentHelper.GetValue(cachedDict, EquipKeys.Armor);

        if (!string.IsNullOrEmpty(fullArmor))
            DisplaySlot(armorGeneralSlot, fullArmor, EquipKeys.Armor, applyVisual, true);
    }

    private void DisplayEquipmentSlots(bool applyVisual)
    {
        DisplaySlot(helmetSlot, cachedData.Helmet, EquipKeys.Helmet, applyVisual, true);

        string melee1HId = ResolveMelee1H();
        string melee2HId = ResolveMelee2H();
        string bowId = cachedDict != null && cachedDict.TryGetValue(EquipKeys.Bow, out var bow)
            ? bow
            : cachedData?.Bow;

        if (!string.IsNullOrEmpty(melee1HId))
        {
            DisplaySlot(meleeWeapon1HSlot, melee1HId, EquipKeys.MeleeWeapon1H, false, true);
            CharacterEquipmentHelper.ClearSlotUI(meleeWeapon2HSlot);
            CharacterEquipmentHelper.ClearSlotUI(bowSlot);
        }
        else if (!string.IsNullOrEmpty(melee2HId))
        {
            DisplaySlot(meleeWeapon2HSlot, melee2HId, EquipKeys.MeleeWeapon2H, false, true);
            CharacterEquipmentHelper.ClearSlotUI(meleeWeapon1HSlot);
            CharacterEquipmentHelper.ClearSlotUI(bowSlot);
        }
        else if (!string.IsNullOrEmpty(bowId))
        {
            DisplaySlot(bowSlot, bowId, EquipKeys.Bow, false, true);
            CharacterEquipmentHelper.ClearSlotUI(meleeWeapon1HSlot);
            CharacterEquipmentHelper.ClearSlotUI(meleeWeapon2HSlot);
        }
        else
        {
            CharacterEquipmentHelper.ClearSlotUI(meleeWeapon1HSlot);
            CharacterEquipmentHelper.ClearSlotUI(meleeWeapon2HSlot);
            CharacterEquipmentHelper.ClearSlotUI(bowSlot);
        }

        DisplaySlot(hairSlot, cachedData.Hair, EquipKeys.Hair, applyVisual, true);
        DisplaySlot(pauldronsSlot, cachedData.Pauldrons, EquipKeys.Pauldrons, applyVisual, true);
        DisplaySlot(bootsSlot, cachedData.Boots, EquipKeys.Boots, applyVisual, true);
        DisplaySlot(beltSlot, cachedData.Belt, EquipKeys.Belt, applyVisual, true);
        DisplaySlot(glovesSlot, cachedData.Gloves, EquipKeys.Gloves, applyVisual, true);
        DisplaySlot(vestSlot, cachedData.Vest, EquipKeys.Vest, applyVisual, true);
        DisplaySlot(capeSlot, cachedData.Cape, EquipKeys.Cape, applyVisual, true);
        DisplaySlot(backSlot, cachedData.Back, EquipKeys.Back, applyVisual, true);
        DisplaySlot(maskSlot, cachedData.Mask, EquipKeys.Mask, applyVisual, true);
        DisplaySlot(glassesSlot, cachedData.Glasses, EquipKeys.Glasses, applyVisual, true);
        DisplaySlot(shieldSlot, cachedData.Shield, EquipKeys.Shield, applyVisual, true);
    }

    private string GetArmorDisplayValue(string type)
    {
        switch (type)
        {
            case "Armor": return cachedData.Armor != null && cachedData.Armor.Length > 0 ? cachedData.Armor[0] : null;
            case "Boots": return cachedData.Boots;
            case "Gloves": return cachedData.Gloves;
            case "Pauldrons": return cachedData.Pauldrons;
            case "Vest": return cachedData.Vest;
            case "Belt": return cachedData.Belt;
            default: return null;
        }
    }

    private string ResolveMelee1H()
    {
        if (cachedDict != null &&
            cachedDict.TryGetValue(EquipKeys.MeleeWeapon1H, out var id) &&
            !string.IsNullOrEmpty(id))
        {
            return ItemIdUtility.Normalize(id);
        }

        string weaponType = cachedDict != null && cachedDict.TryGetValue("WeaponType", out var wt)
            ? wt
            : cachedData?.WeaponType;

        bool isMelee1H =
            weaponType == EquipKeys.Weapon_Melee1H ||
            weaponType == "0" ||
            weaponType == "Melee1H";

        // fallback từ dict trước
        if (isMelee1H &&
            cachedDict != null &&
            cachedDict.TryGetValue(EquipKeys.PrimaryMeleeWeapon, out var primaryDict) &&
            !string.IsNullOrEmpty(primaryDict))
        {
            return ItemIdUtility.Normalize(primaryDict);
        }

        // fallback từ cachedData sau
        if (isMelee1H && !string.IsNullOrEmpty(cachedData?.PrimaryMeleeWeapon))
        {
            return ItemIdUtility.Normalize(cachedData.PrimaryMeleeWeapon);
        }

        if (cachedData != null && !string.IsNullOrEmpty(cachedData.MeleeWeapon1H))
            return ItemIdUtility.Normalize(cachedData.MeleeWeapon1H);

        return null;
    }

    private string ResolveMelee2H()
    {
        if (cachedDict != null &&
            cachedDict.TryGetValue(EquipKeys.MeleeWeapon2H, out var id) &&
            !string.IsNullOrEmpty(id))
        {
            return ItemIdUtility.Normalize(id);
        }

        string weaponType = cachedDict != null && cachedDict.TryGetValue("WeaponType", out var wt)
            ? wt
            : cachedData?.WeaponType;

        bool isMelee2H =
            weaponType == EquipKeys.Weapon_Melee2H ||
            weaponType == "1" ||
            weaponType == "Melee2H";

        // fallback từ dict trước
        if (isMelee2H &&
            cachedDict != null &&
            cachedDict.TryGetValue(EquipKeys.PrimaryMeleeWeapon, out var primaryDict) &&
            !string.IsNullOrEmpty(primaryDict))
        {
            return ItemIdUtility.Normalize(primaryDict);
        }

        // fallback từ cachedData sau
        if (isMelee2H && !string.IsNullOrEmpty(cachedData?.PrimaryMeleeWeapon))
        {
            return ItemIdUtility.Normalize(cachedData.PrimaryMeleeWeapon);
        }

        if (cachedData != null && !string.IsNullOrEmpty(cachedData.MeleeWeapon2H))
            return ItemIdUtility.Normalize(cachedData.MeleeWeapon2H);

        return null;
    }

    private void DisplaySlot(GameObject slot, string itemPath, string expectedType, bool applyVisual, bool addStats)
    {
        var stats = CharacterSlotRenderer.DisplaySlot(slot, itemPath, expectedType);
        if (stats == null) return;

        if (addStats)
            equippedItems.Add(stats);

        if (applyVisual)
            EquipVisualFromStats(stats);
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

    private void ResetCharacterVisual()
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

        // QUAN TRỌNG: reset luôn render sprite của weapon để tránh giữ state cũ
        if (character.PrimaryMeleeWeaponRenderer != null)
            character.PrimaryMeleeWeaponRenderer.sprite = null;

        if (character.SecondaryMeleeWeaponRenderer != null)
            character.SecondaryMeleeWeaponRenderer.sprite = null;

        if (character.BowRenderers != null)
        {
            foreach (var r in character.BowRenderers)
            {
                if (r != null) r.sprite = null;
            }
        }
    }
    public void ClearAllSlots()
    {
        CharacterEquipmentHelper.ClearSlotUI(helmetSlot);

        foreach (var slot in armorSlots)
            CharacterEquipmentHelper.ClearSlotUI(slot);

        CharacterEquipmentHelper.ClearSlotUI(vestSlot);
        CharacterEquipmentHelper.ClearSlotUI(pauldronsSlot);
        CharacterEquipmentHelper.ClearSlotUI(glovesSlot);
        CharacterEquipmentHelper.ClearSlotUI(bootsSlot);
        CharacterEquipmentHelper.ClearSlotUI(bowSlot);
        CharacterEquipmentHelper.ClearSlotUI(hairSlot);
        CharacterEquipmentHelper.ClearSlotUI(beltSlot);
        CharacterEquipmentHelper.ClearSlotUI(capeSlot);
        CharacterEquipmentHelper.ClearSlotUI(backSlot);
        CharacterEquipmentHelper.ClearSlotUI(maskSlot);
        CharacterEquipmentHelper.ClearSlotUI(glassesSlot);
        CharacterEquipmentHelper.ClearSlotUI(shieldSlot);
        CharacterEquipmentHelper.ClearSlotUI(armorGeneralSlot);
        CharacterEquipmentHelper.ClearSlotUI(meleeWeapon1HSlot);
        CharacterEquipmentHelper.ClearSlotUI(meleeWeapon2HSlot);
    }
   
}