using System.Collections.Generic;

public sealed class EquipmentState
{
    private readonly Dictionary<string, string> dict;

    private static readonly string[] PartialArmorTypes =
    {
        EquipKeys.Vest,
        EquipKeys.Pauldrons,
        EquipKeys.Gloves,
        EquipKeys.Boots,
        EquipKeys.Belt
    };

    public EquipmentState(Dictionary<string, string> source)
    {
        dict = source != null
            ? new Dictionary<string, string>(source)
            : CharacterJsonService.CreateEmptyDict();

        Normalize();
    }

    public static EquipmentState FromCurrent()
    {
        return new EquipmentState(CharacterJsonService.LoadDict());
    }

    public static EquipmentState FromJson(string json)
    {
        return new EquipmentState(CharacterJsonService.LoadDict(json));
    }

    public string Get(string key)
    {
        return CharacterJsonService.GetValue(dict, key);
    }

    public void Set(string key, string value)
    {
        CharacterJsonService.SetValue(
            dict,
            key,
            string.IsNullOrWhiteSpace(value) ? "" : ItemIdUtility.Normalize(value)
        );
    }

    public Dictionary<string, string> ToDictionary()
    {
        return new Dictionary<string, string>(dict);
    }

    public string ToJson()
    {
        Normalize();
        return CharacterJsonService.SaveDict(dict);
    }

    public void Equip(string type, string itemId)
    {
        itemId = ItemIdUtility.Normalize(itemId);

        switch (type)
        {
            case EquipKeys.MeleeWeapon1H:
                Set(EquipKeys.MeleeWeapon1H, itemId);
                Set(EquipKeys.MeleeWeapon2H, "");
                Set(EquipKeys.Bow, "");
                Set(EquipKeys.PrimaryMeleeWeapon, itemId);
                Set(EquipKeys.SecondaryMeleeWeapon, "");
                Set("WeaponType", EquipKeys.Weapon_Melee1H);
                break;

            case EquipKeys.MeleeWeapon2H:
                Set(EquipKeys.MeleeWeapon1H, "");
                Set(EquipKeys.MeleeWeapon2H, itemId);
                Set(EquipKeys.Bow, "");
                Set(EquipKeys.PrimaryMeleeWeapon, itemId);
                Set(EquipKeys.SecondaryMeleeWeapon, "");
                Set("WeaponType", EquipKeys.Weapon_Melee2H);
                break;

            case EquipKeys.Bow:
                Set(EquipKeys.MeleeWeapon1H, "");
                Set(EquipKeys.MeleeWeapon2H, "");
                Set(EquipKeys.Bow, itemId);
                Set(EquipKeys.PrimaryMeleeWeapon, "");
                Set(EquipKeys.SecondaryMeleeWeapon, "");
                Set("WeaponType", EquipKeys.Weapon_Bow);
                break;

            default:
                Set(type, itemId);
                break;
        }

        Normalize();
    }

    public bool CanUnequip(string type)
    {
        if (string.IsNullOrEmpty(type))
            return false;

        if (type == EquipKeys.Hair)
            return false;

        if (type == EquipKeys.MeleeWeapon1H ||
            type == EquipKeys.MeleeWeapon2H ||
            type == EquipKeys.Bow ||
            type == EquipKeys.PrimaryMeleeWeapon)
            return false;

        return true;
    }

    public void Unequip(string type)
    {
        if (!CanUnequip(type))
            return;

        Set(type, "");
        Normalize();
    }

    public void Normalize()
    {
        NormalizeWeapons();
        NormalizeArmorLegacyField();
    }

    private void NormalizeWeapons()
    {
        string melee1 = Get(EquipKeys.MeleeWeapon1H);
        string melee2 = Get(EquipKeys.MeleeWeapon2H);
        string bow = Get(EquipKeys.Bow);
        string primary = Get(EquipKeys.PrimaryMeleeWeapon);
        string weaponType = Get("WeaponType");

        if (!string.IsNullOrEmpty(bow))
        {
            Set(EquipKeys.MeleeWeapon1H, "");
            Set(EquipKeys.MeleeWeapon2H, "");
            Set(EquipKeys.PrimaryMeleeWeapon, "");
            Set(EquipKeys.SecondaryMeleeWeapon, "");
            Set("WeaponType", EquipKeys.Weapon_Bow);
            return;
        }

        if (!string.IsNullOrEmpty(melee1))
        {
            Set(EquipKeys.MeleeWeapon2H, "");
            Set(EquipKeys.Bow, "");
            Set(EquipKeys.PrimaryMeleeWeapon, melee1);
            Set(EquipKeys.SecondaryMeleeWeapon, "");
            Set("WeaponType", EquipKeys.Weapon_Melee1H);
            return;
        }

        if (!string.IsNullOrEmpty(melee2))
        {
            Set(EquipKeys.MeleeWeapon1H, "");
            Set(EquipKeys.Bow, "");
            Set(EquipKeys.PrimaryMeleeWeapon, melee2);
            Set(EquipKeys.SecondaryMeleeWeapon, "");
            Set("WeaponType", EquipKeys.Weapon_Melee2H);
            return;
        }

        if (!string.IsNullOrEmpty(primary))
        {
            if (weaponType == EquipKeys.Weapon_Melee1H || weaponType == "Melee1H" || weaponType == "0")
            {
                Set(EquipKeys.MeleeWeapon1H, primary);
                Set(EquipKeys.MeleeWeapon2H, "");
                Set(EquipKeys.Bow, "");
                Set(EquipKeys.SecondaryMeleeWeapon, "");
                Set("WeaponType", EquipKeys.Weapon_Melee1H);
                return;
            }

            if (weaponType == EquipKeys.Weapon_Melee2H || weaponType == "Melee2H" || weaponType == "1")
            {
                Set(EquipKeys.MeleeWeapon2H, primary);
                Set(EquipKeys.MeleeWeapon1H, "");
                Set(EquipKeys.Bow, "");
                Set(EquipKeys.SecondaryMeleeWeapon, "");
                Set("WeaponType", EquipKeys.Weapon_Melee2H);
                return;
            }
        }

        Set(EquipKeys.MeleeWeapon1H, "");
        Set(EquipKeys.MeleeWeapon2H, "");
        Set(EquipKeys.Bow, "");
        Set(EquipKeys.PrimaryMeleeWeapon, "");
        Set(EquipKeys.SecondaryMeleeWeapon, "");
        Set("WeaponType", "");
    }

    private void NormalizeArmorLegacyField()
    {
        bool hasAnyPartial =
            !string.IsNullOrEmpty(Get(EquipKeys.Vest)) ||
            !string.IsNullOrEmpty(Get(EquipKeys.Pauldrons)) ||
            !string.IsNullOrEmpty(Get(EquipKeys.Gloves)) ||
            !string.IsNullOrEmpty(Get(EquipKeys.Boots)) ||
            !string.IsNullOrEmpty(Get(EquipKeys.Belt));

        if (!hasAnyPartial)
        {
            Set(EquipKeys.Armor, "");
        }
    }
}