using System.Collections.Generic;

public static class WeaponStateResolver
{
    public enum WeaponKind
    {
        None,
        Melee1H,
        Melee2H,
        Bow
    }

    public static WeaponKind ResolveKind(Dictionary<string, string> dict)
    {
        string melee1 = Get(dict, EquipKeys.MeleeWeapon1H);
        string melee2 = Get(dict, EquipKeys.MeleeWeapon2H);
        string bow = Get(dict, EquipKeys.Bow);
        string primary = Get(dict, EquipKeys.PrimaryMeleeWeapon);
        string weaponType = Get(dict, "WeaponType");

        if (!string.IsNullOrWhiteSpace(bow))
            return WeaponKind.Bow;

        if (!string.IsNullOrWhiteSpace(melee1))
            return WeaponKind.Melee1H;

        if (!string.IsNullOrWhiteSpace(melee2))
            return WeaponKind.Melee2H;

        if (!string.IsNullOrWhiteSpace(primary))
        {
            if (weaponType == EquipKeys.Weapon_Melee1H || weaponType == "Melee1H" || weaponType == "0")
                return WeaponKind.Melee1H;

            if (weaponType == EquipKeys.Weapon_Melee2H || weaponType == "Melee2H" || weaponType == "1")
                return WeaponKind.Melee2H;

            if (weaponType == EquipKeys.Weapon_Bow || weaponType == "Bow" || weaponType == "2")
                return WeaponKind.Bow;
        }

        return WeaponKind.None;
    }

    public static string ResolveItemId(Dictionary<string, string> dict)
    {
        var kind = ResolveKind(dict);

        switch (kind)
        {
            case WeaponKind.Melee1H:
                return FirstNotEmpty(
                    Get(dict, EquipKeys.MeleeWeapon1H),
                    Get(dict, EquipKeys.PrimaryMeleeWeapon)
                );

            case WeaponKind.Melee2H:
                return FirstNotEmpty(
                    Get(dict, EquipKeys.MeleeWeapon2H),
                    Get(dict, EquipKeys.PrimaryMeleeWeapon)
                );

            case WeaponKind.Bow:
                return FirstNotEmpty(
                    Get(dict, EquipKeys.Bow),
                    Get(dict, EquipKeys.PrimaryMeleeWeapon)
                );

            default:
                return null;
        }
    }

    private static string Get(Dictionary<string, string> dict, string key)
    {
        return dict != null && dict.TryGetValue(key, out var value) ? value : null;
    }

    private static string FirstNotEmpty(params string[] values)
    {
        if (values == null) return null;

        foreach (var v in values)
        {
            if (!string.IsNullOrWhiteSpace(v))
                return v;
        }

        return null;
    }
}