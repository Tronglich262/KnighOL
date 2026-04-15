using Assets.HeroEditor.Common.CharacterScripts;
using HeroEditor.Common.Enums;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public static class CharacterEquipHandler
{
    public static readonly Dictionary<string, List<int>> ArmorTypeToIndexes = new()
    {
        { EquipKeys.Pauldrons, new List<int> { 0, 1 } },
        { EquipKeys.Boots,     new List<int> { 9, 7 } },
        { EquipKeys.Vest,      new List<int> { 11 } },
        { EquipKeys.Belt,      new List<int> { 8 } },
        { EquipKeys.Gloves,    new List<int> { 3, 4, 2, 5, 6, 10 } }
    };

    public static readonly Dictionary<string, List<int>> BowTypeToIndexes = new()
    {
        { "Arrow", new List<int> { 0 } },
        { "Limb",  new List<int> { 1 } },
        { "Riser", new List<int> { 2 } },
    };

    public static void EquipItemToCharacter(InventoryItem1 item)
    {
        EquipmentCoordinator.Equip(item, out _);
    }

    public static void UnequipItem(string type)
    {
        EquipmentCoordinator.Unequip(type, out _);
    }

    public static void TestEquipArmor(Character character, string armorId)
    {
        EquipFullArmor(character, armorId);
    }

    public static void TestEquipBow(Character character, string bowId)
    {
        EquipFullBow(character, bowId);
    }

    public static void EquipFullArmor(Character character, string armorId)
    {
        if (character == null || character.SpriteCollection == null) return;

        var entry = character.SpriteCollection.Armor.Find(e => e.Id == armorId);
        if (entry == null || entry.Sprites == null || entry.Sprites.Count != 12) return;

        while (character.Armor.Count < 12)
            character.Armor.Add(null);

        for (int i = 0; i < 12; i++)
            character.Armor[i] = entry.Sprites[i];

        character.EquipArmor(character.Armor);
        character.Initialize();
    }

    public static void EquipPartialArmorFromEntry(Character character, string itemId, string type)
    {
        if (character == null || character.SpriteCollection == null) return;
        if (!ArmorTypeToIndexes.TryGetValue(type, out var indexes)) return;

        string[] parts = itemId.Split('.');
        if (parts.Length < 4) return;

        string armorName = parts[3];
        string baseArmorId = $"{parts[0]}.{parts[1]}.Armor.{armorName}";

        var entry = character.SpriteCollection.Armor.Find(e => e.Id == baseArmorId);
        if (entry == null) return;

        while (character.Armor.Count < 12)
            character.Armor.Add(null);

        foreach (var idx in indexes)
        {
            if (idx >= 0 && idx < entry.Sprites.Count)
                character.Armor[idx] = entry.Sprites[idx];
        }

        character.EquipArmor(character.Armor);
        character.Initialize();
    }

    public static void EquipFullBow(Character character, string bowId)
    {
        if (character == null || character.SpriteCollection == null) return;

        var entry = character.SpriteCollection.Bow.Find(e => e.Id == bowId);
        if (entry == null || entry.Sprites == null || entry.Sprites.Count != 3) return;

        while (character.Bow.Count < 3)
            character.Bow.Add(null);

        for (int i = 0; i < 3; i++)
            character.Bow[i] = entry.Sprites[i];

        character.EquipBow(character.Bow);
        character.Initialize();
    }

    public static void EquipPartialBowFromEntry(Character character, string itemId, string type)
    {
        if (character == null || character.SpriteCollection == null) return;
        if (!BowTypeToIndexes.TryGetValue(type, out var indexes)) return;

        string[] parts = itemId.Split('.');
        if (parts.Length < 4) return;

        string bowName = parts[3];
        string baseBowId = $"{parts[0]}.{parts[1]}.Bow.{bowName}";

        var entry = character.SpriteCollection.Bow.Find(e => e.Id == baseBowId);
        if (entry == null) return;

        while (character.Bow.Count < 3)
            character.Bow.Add(null);

        foreach (var idx in indexes)
        {
            if (idx >= 0 && idx < entry.Sprites.Count)
                character.Bow[idx] = entry.Sprites[idx];
        }

        character.EquipBow(character.Bow);
        character.Initialize();
    }

    public static void EquipPartialArmor(Character character, string type, Sprite sprite)
    {
        if (!ArmorTypeToIndexes.TryGetValue(type, out var indexes)) return;

        while (character.Armor.Count < 12)
            character.Armor.Add(null);

        foreach (var i in indexes)
            character.Armor[i] = sprite;

        character.EquipArmor(character.Armor);
        character.Initialize();
    }

    public static void EquipPartialBow(Character character, string type, Sprite sprite)
    {
        if (!BowTypeToIndexes.TryGetValue(type, out var indexes)) return;

        while (character.Bow.Count < 3)
            character.Bow.Add(null);

        foreach (var i in indexes)
            character.Bow[i] = sprite;

        character.EquipBow(character.Bow);
        character.Initialize();
    }
}