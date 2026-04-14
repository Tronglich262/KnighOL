using System.Collections.Generic;
using System.Linq;

public static class CharacterJsonSanitizer
{
    public static Dictionary<string, string> NormalizeAll(Dictionary<string, string> dict)
    {
        if (dict == null) return new Dictionary<string, string>();

        var keys = dict.Keys.ToList();
        foreach (var key in keys)
        {
            dict[key] = ItemIdUtility.Normalize(dict[key]);
        }

        return dict;
    }
}