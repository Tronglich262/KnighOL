using System.Collections.Generic;
using Newtonsoft.Json;

public static class CharacterJsonService
{
    private static readonly string[] RequiredKeys =
 {
    "Head", "Body", "Ears", "Hair", "Beard", "Helmet",
    "Glasses", "Mask", "Cape", "Back", "Shield",
    "PrimaryMeleeWeapon", "SecondaryMeleeWeapon",
    "MeleeWeapon1H", "MeleeWeapon2H",
    "Bow", "WeaponType",
    "Armor", "Boots", "Gloves", "Pauldrons", "Vest", "Belt",
    "Expression", "HideEars", "FullHair", "BodyScaleX", "BodyScaleY"
};

    public static Dictionary<string, string> LoadDict(string json = null)
    {
        json ??= PlayerDataHolder1.CharacterJson;

        if (string.IsNullOrEmpty(json))
            return CreateEmptyDict();

        try
        {
            var dict = JsonConvert.DeserializeObject<Dictionary<string, string>>(json)
                       ?? CreateEmptyDict();

            // project không dùng firearm
            dict.Remove("Firearms");
            dict.Remove("FirearmParams");

            EnsureRequiredKeys(dict);
            return dict;
        }
        catch
        {
            return CreateEmptyDict();
        }
    }

    public static string SaveDict(Dictionary<string, string> dict)
    {
        EnsureRequiredKeys(dict);
        return JsonConvert.SerializeObject(dict, Formatting.None);
    }

    public static void EnsureRequiredKeys(Dictionary<string, string> dict)
    {
        foreach (var key in RequiredKeys)
        {
            if (!dict.ContainsKey(key))
                dict[key] = "";
        }
    }

    public static Dictionary<string, string> CreateEmptyDict()
    {
        var dict = new Dictionary<string, string>();
        EnsureRequiredKeys(dict);
        return dict;
    }

    public static void SetValue(Dictionary<string, string> dict, string key, string value)
    {
        EnsureRequiredKeys(dict);
        dict[key] = string.IsNullOrEmpty(value) ? "" : value;
    }

    public static string GetValue(Dictionary<string, string> dict, string key)
    {
        EnsureRequiredKeys(dict);
        return dict.TryGetValue(key, out var value) ? value : "";
    }
}