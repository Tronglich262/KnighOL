using Newtonsoft.Json;
using System.Collections.Generic;
using UnityEngine;

public class CharacterEquipmentPresenter
{
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

    private Dictionary<string, string> cachedDict;

    public CharacterEquipmentPresenter(
        Assets.HeroEditor.Common.CharacterScripts.Character character,
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
        if (string.IsNullOrEmpty(json))
            return;

        cachedDict = JsonConvert.DeserializeObject<Dictionary<string, string>>(json) ?? new Dictionary<string, string>();

        equippedItems.Clear();
        ClearAllSlots();

        DisplayArmorSlots();
        DisplayEquipmentSlots();
    }

    public void ClearAllSlots()
    {
        CharacterEquipmentHelper.ClearSlotUI(helmetSlot);
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

        if (armorSlots != null)
        {
            foreach (var slot in armorSlots)
                CharacterEquipmentHelper.ClearSlotUI(slot);
        }
    }

    private void DisplayArmorSlots()
    {
        DisplaySlot(armorGeneralSlot, Get(EquipKeys.Armor), EquipKeys.Armor);

        DisplaySlot(vestSlot, Get(EquipKeys.Vest), EquipKeys.Vest);
        DisplaySlot(pauldronsSlot, Get(EquipKeys.Pauldrons), EquipKeys.Pauldrons);
        DisplaySlot(glovesSlot, Get(EquipKeys.Gloves), EquipKeys.Gloves);
        DisplaySlot(bootsSlot, Get(EquipKeys.Boots), EquipKeys.Boots);
        DisplaySlot(beltSlot, Get(EquipKeys.Belt), EquipKeys.Belt);

        if (armorSlots != null && armorSlots.Length > 0)
        {
            string[] armorTypes =
            {
                EquipKeys.Armor,
                EquipKeys.Boots,
                EquipKeys.Gloves,
                EquipKeys.Pauldrons,
                EquipKeys.Vest,
                EquipKeys.Belt
            };

            for (int i = 0; i < armorSlots.Length && i < armorTypes.Length; i++)
            {
                DisplaySlot(armorSlots[i], Get(armorTypes[i]), armorTypes[i]);
            }
        }
    }

    private void DisplayEquipmentSlots()
    {
        DisplaySlot(helmetSlot, Get(EquipKeys.Helmet), EquipKeys.Helmet);
        DisplaySlot(hairSlot, Get(EquipKeys.Hair), EquipKeys.Hair);
        DisplaySlot(capeSlot, Get(EquipKeys.Cape), EquipKeys.Cape);
        DisplaySlot(backSlot, Get(EquipKeys.Back), EquipKeys.Back);
        DisplaySlot(maskSlot, Get(EquipKeys.Mask), EquipKeys.Mask);
        DisplaySlot(glassesSlot, Get(EquipKeys.Glasses), EquipKeys.Glasses);
        DisplaySlot(shieldSlot, Get(EquipKeys.Shield), EquipKeys.Shield);

        var kind = WeaponStateResolver.ResolveKind(cachedDict);
        string weaponId = WeaponStateResolver.ResolveItemId(cachedDict);

        CharacterEquipmentHelper.ClearSlotUI(meleeWeapon1HSlot);
        CharacterEquipmentHelper.ClearSlotUI(meleeWeapon2HSlot);
        CharacterEquipmentHelper.ClearSlotUI(bowSlot);

        if (string.IsNullOrWhiteSpace(weaponId))
            return;

        switch (kind)
        {
            case WeaponStateResolver.WeaponKind.Melee1H:
                DisplaySlot(meleeWeapon1HSlot, weaponId, EquipKeys.MeleeWeapon1H);
                break;

            case WeaponStateResolver.WeaponKind.Melee2H:
                DisplaySlot(meleeWeapon2HSlot, weaponId, EquipKeys.MeleeWeapon2H);
                break;

            case WeaponStateResolver.WeaponKind.Bow:
                DisplaySlot(bowSlot, weaponId, EquipKeys.Bow);
                break;
        }
    }

    private void DisplaySlot(GameObject slot, string itemId, string expectedType)
    {
        if (slot == null)
            return;

        if (string.IsNullOrWhiteSpace(itemId))
        {
            CharacterEquipmentHelper.ClearSlotUI(slot);
            return;
        }

        var stats = CharacterSlotRenderer.DisplaySlot(slot, itemId, expectedType);
        if (stats != null)
            equippedItems.Add(stats);
    }

    private string Get(string key)
    {
        return cachedDict != null && cachedDict.TryGetValue(key, out var value) ? value : null;
    }
}