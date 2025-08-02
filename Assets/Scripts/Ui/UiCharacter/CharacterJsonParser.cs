/*using System.Collections.Generic;
using UnityEngine;

public static class CharacterJsonParser
{
    public static CharacterEquipmentData ParseCharacterJson(string json)
    {
        var result = new CharacterEquipmentData();
        var dict = MiniJSON.Deserialize(json) as Dictionary<string, object>;

        foreach (var kvp in dict)
        {
            string key = kvp.Key;
            string value = kvp.Value.ToString();

            if (key.StartsWith("Armor["))
            {
                result.Armor.Add(value);
            }
            else if (key == "Expression.Default.Eyebrows") result.Expression_Default_Eyebrows = value;
            else if (key == "Expression.Default.Eyes") result.Expression_Default_Eyes = value;
            else if (key == "Expression.Default.EyesColor") result.Expression_Default_EyesColor = value;
            else if (key == "Expression.Default.Mouth") result.Expression_Default_Mouth = value;
            else
            {
                var field = typeof(CharacterEquipmentData).GetField(key);
                if (field != null)
                    field.SetValue(result, value);
            }
        }

        return result;
    }

}
*/