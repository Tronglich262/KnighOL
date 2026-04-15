using System.Collections.Generic;
using System.Linq;
using Assets.HeroEditor.Common.CharacterScripts;
using HeroEditor.Common.Enums;
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
        if (character == null || dict == null || character.SpriteCollection == null)
            return;

        ResetCharacterVisual(character);

        ApplySimpleParts(character, dict);
        ApplyArmor(character, dict);
        ApplyBow(character, dict);
        ApplyWeapons(character, dict);

        character.Initialize();
    }

    private static void ResetCharacterVisual(Character character)
    {
        character.Helmet = null;
        character.Glasses = null;
        character.Mask = null;
        character.Cape = null;
        character.Back = null;
        character.Shield = null;
        character.Hair = null;

        while (character.Armor.Count < 12)
            character.Armor.Add(null);

        for (int i = 0; i < character.Armor.Count; i++)
            character.Armor[i] = null;

        while (character.Bow.Count < 3)
            character.Bow.Add(null);

        for (int i = 0; i < character.Bow.Count; i++)
            character.Bow[i] = null;
    }

    private static void ApplySimpleParts(Character character, Dictionary<string, string> dict)
    {
        ApplySingleSpritePart(character, dict, EquipKeys.Helmet, EquipmentPart.Helmet);
        ApplySingleSpritePart(character, dict, EquipKeys.Glasses, EquipmentPart.Glasses);
        ApplySingleSpritePart(character, dict, EquipKeys.Mask, EquipmentPart.Mask);
        ApplySingleSpritePart(character, dict, EquipKeys.Cape, EquipmentPart.Cape);
        ApplySingleSpritePart(character, dict, EquipKeys.Back, EquipmentPart.Back);
        ApplySingleSpritePart(character, dict, EquipKeys.Shield, EquipmentPart.Shield);
      //  ApplySingleSpritePart(character, dict, EquipKeys.Hair, EquipmentPart.Hair);
    }

    private static void ApplySingleSpritePart(Character character, Dictionary<string, string> dict, string key, EquipmentPart part)
    {
        if (!dict.TryGetValue(key, out string id) || string.IsNullOrWhiteSpace(id))
            return;

        var sprite = FindSpriteForPart(character, part, id);
        if (sprite == null)
            return;

        switch (part)
        {
            case EquipmentPart.Helmet: character.Helmet = sprite; break;
            case EquipmentPart.Glasses: character.Glasses = sprite; break;
            case EquipmentPart.Mask: character.Mask = sprite; break;
            case EquipmentPart.Cape: character.Cape = sprite; break;
            case EquipmentPart.Back: character.Back = sprite; break;
            case EquipmentPart.Shield: character.Shield = sprite; break;
            //case EquipmentPart.Hair: character.Hair = sprite; break;
        }
    }

    public static void ApplyArmor(Character character, Dictionary<string, string> dict)
    {
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

            var entry = character.SpriteCollection.Armor.FirstOrDefault(e => e.Id == baseArmorId);
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

            var entry = character.SpriteCollection.Bow.FirstOrDefault(e => e.Id == baseBowId);
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

    public static void ApplyWeapons(Character character, Dictionary<string, string> dict)
    {
        if (character == null || character.SpriteCollection == null)
            return;

        var kind = WeaponStateResolver.ResolveKind(dict);
        string weaponId = WeaponStateResolver.ResolveItemId(dict);

        if (string.IsNullOrWhiteSpace(weaponId))
            return;

        switch (kind)
        {
            case WeaponStateResolver.WeaponKind.Melee1H:
                {
                    var entry = character.SpriteCollection.MeleeWeapon1H.Find(e => e.Id == weaponId);
                    if (entry != null)
                    {
                        character.WeaponType = HeroEditor.Common.Enums.WeaponType.Melee1H;
                        character.Equip(entry, HeroEditor.Common.Enums.EquipmentPart.MeleeWeapon1H);
                    }
                    break;
                }

            case WeaponStateResolver.WeaponKind.Melee2H:
                {
                    var entry = character.SpriteCollection.MeleeWeapon2H.Find(e => e.Id == weaponId);
                    if (entry != null)
                    {
                        character.WeaponType = HeroEditor.Common.Enums.WeaponType.Melee2H;
                        character.Equip(entry, HeroEditor.Common.Enums.EquipmentPart.MeleeWeapon2H);
                    }
                    break;
                }

            case WeaponStateResolver.WeaponKind.Bow:
                {
                    var entry = character.SpriteCollection.Bow.Find(e => e.Id == weaponId);
                    if (entry != null)
                    {
                        character.WeaponType = HeroEditor.Common.Enums.WeaponType.Bow;
                        character.Equip(entry, HeroEditor.Common.Enums.EquipmentPart.Bow);
                    }
                    break;
                }
        }
    }

    private static string Get(Dictionary<string, string> dict, string key)
    {
        return dict != null && dict.TryGetValue(key, out var value) ? value : null;
    }

    private static Sprite FindSpriteForPart(Character character, EquipmentPart part, string id)
    {
        switch (part)
        {
            case EquipmentPart.Helmet:
                return character.SpriteCollection.Helmet.FirstOrDefault(e => e.Id == id)?.Sprite;
            case EquipmentPart.Glasses:
                return character.SpriteCollection.Glasses.FirstOrDefault(e => e.Id == id)?.Sprite;
            case EquipmentPart.Mask:
                return character.SpriteCollection.Mask.FirstOrDefault(e => e.Id == id)?.Sprite;
            case EquipmentPart.Cape:
                return character.SpriteCollection.Cape.FirstOrDefault(e => e.Id == id)?.Sprite;
            case EquipmentPart.Back:
                return character.SpriteCollection.Back.FirstOrDefault(e => e.Id == id)?.Sprite;
            case EquipmentPart.Shield:
                return character.SpriteCollection.Shield.FirstOrDefault(e => e.Id == id)?.Sprite;
           // case EquipmentPart.Hair:
          //      return character.SpriteCollection.Hair.FirstOrDefault(e => e.Id == id)?.Sprite;
            default:
                return null;
        }
    }
}