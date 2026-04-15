using System.Collections.Generic;
using Assets.HeroEditor.Common.CharacterScripts;
using UnityEngine;

public static class CharacterVisualCompositeBuilder
{
    private static readonly Dictionary<string, List<int>> ArmorTypeToIndexes = new()
    {
        { EquipKeys.Pauldrons, new List<int> { 0, 1 } },
        { EquipKeys.Boots,     new List<int> { 9, 7 } },
        { EquipKeys.Vest,      new List<int> { 11 } },
        { EquipKeys.Belt,      new List<int> { 8 } },
        { EquipKeys.Gloves,    new List<int> { 3, 4, 2, 5, 6, 10 } }
    };

    private static readonly Dictionary<string, List<int>> BowTypeToIndexes = new()
    {
        { "Arrow", new List<int> { 0 } },
        { "Limb",  new List<int> { 1 } },
        { "Riser", new List<int> { 2 } },
    };

    public static void ApplyAll(Character character, Dictionary<string, string> dict)
    {
        if (character == null || dict == null)
            return;

        ApplyArmor(character, dict);
        ApplyBow(character, dict);
        ApplyWeaponsAndOtherParts(character, dict);

        character.Initialize();
    }

    public static void ApplyArmor(Character character, Dictionary<string, string> dict)
    {
        if (character == null || character.SpriteCollection == null)
            return;

        while (character.Armor.Count < 12)
            character.Armor.Add(null);

        for (int i = 0; i < character.Armor.Count; i++)
            character.Armor[i] = null;

        foreach (var kv in ArmorTypeToIndexes)
        {
            string type = kv.Key;
            if (!dict.TryGetValue(type, out string itemId) || string.IsNullOrWhiteSpace(itemId))
                continue;

            string[] parts = itemId.Split('.');
            if (parts.Length < 4)
                continue;

            string armorName = parts[3];
            string baseArmorId = $"{parts[0]}.{parts[1]}.Armor.{armorName}";

            var entry = character.SpriteCollection.Armor.Find(e => e.Id == baseArmorId);
            if (entry == null || entry.Sprites == null)
                continue;

            foreach (int index in kv.Value)
            {
                if (index >= 0 && index < entry.Sprites.Count && index < character.Armor.Count)
                    character.Armor[index] = entry.Sprites[index];
            }
        }

        character.EquipArmor(character.Armor);
    }

    public static void ApplyBow(Character character, Dictionary<string, string> dict)
    {
        if (character == null || character.SpriteCollection == null)
            return;

        while (character.Bow.Count < 3)
            character.Bow.Add(null);

        for (int i = 0; i < character.Bow.Count; i++)
            character.Bow[i] = null;

        foreach (var kv in BowTypeToIndexes)
        {
            string type = kv.Key;
            if (!dict.TryGetValue(type, out string itemId) || string.IsNullOrWhiteSpace(itemId))
                continue;

            string[] parts = itemId.Split('.');
            if (parts.Length < 4)
                continue;

            string bowName = parts[3];
            string baseBowId = $"{parts[0]}.{parts[1]}.Bow.{bowName}";

            var entry = character.SpriteCollection.Bow.Find(e => e.Id == baseBowId);
            if (entry == null || entry.Sprites == null)
                continue;

            foreach (int index in kv.Value)
            {
                if (index >= 0 && index < entry.Sprites.Count && index < character.Bow.Count)
                    character.Bow[index] = entry.Sprites[index];
            }
        }

        character.EquipBow(character.Bow);
    }

    public static void ApplyWeaponsAndOtherParts(Character character, Dictionary<string, string> dict)
    {
        string json = CharacterJsonService.SaveDict(dict);
        character.FromJson(json);
    }
}