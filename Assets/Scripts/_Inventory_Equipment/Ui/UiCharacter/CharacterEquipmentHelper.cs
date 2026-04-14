using Assets.HeroEditor.Common.CommonScripts;
using Newtonsoft.Json;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public static class CharacterEquipmentHelper
{
    public static readonly string[] ArmorTypes =
    {
        EquipKeys.Armor,
        EquipKeys.Boots,
        EquipKeys.Gloves,
        EquipKeys.Pauldrons,
        EquipKeys.Vest,
        EquipKeys.Belt
    };

    public static readonly string[] PartialArmorTypes =
    {
        EquipKeys.Boots,
        EquipKeys.Gloves,
        EquipKeys.Pauldrons,
        EquipKeys.Vest,
        EquipKeys.Belt
    };

    private static readonly string[] IconCollectionNames =
    {
        "Extensions.Legendary",
        "FantasyHeroes.Basic",
        "Extensions.Epic",
        "FantasyHeroes.Samurai",
        "Extensions.AbandonedWorkshop",
        "UndeadHeroes.Undead",
        "Extensions.MoonStyle [NoPaint]"
    };

    private static readonly Dictionary<string, ItemIcon> IconCache = new();
    private static IconCollection _cachedCollection;

    public static void RebuildIconCacheIfNeeded()
    {
        if (IconCollection.Active == null) return;
        if (_cachedCollection == IconCollection.Active && IconCache.Count > 0) return;

        _cachedCollection = IconCollection.Active;
        IconCache.Clear();

        var icons = IconCollection.Active.Icons;
        if (icons == null) return;

        foreach (var icon in icons)
        {
            if (icon == null || string.IsNullOrEmpty(icon.Id) || string.IsNullOrEmpty(icon.Type))
                continue;

            string key1 = $"{icon.Type}|{icon.Id}";
            if (!IconCache.ContainsKey(key1))
                IconCache[key1] = icon;

            string shortName = GetLastToken(icon.Id);
            string key2 = $"{icon.Type}|{shortName}";
            if (!IconCache.ContainsKey(key2))
                IconCache[key2] = icon;
        }
    }

    public static string GetCleanId(string value)
    {
        if (string.IsNullOrEmpty(value)) return null;

        int hashIndex = value.IndexOf('#');
        return hashIndex >= 0 ? value[..hashIndex].Trim() : value.Trim();
    }

    public static string GetLastToken(string value, char separator = '.')
    {
        if (string.IsNullOrEmpty(value)) return value;

        int index = value.LastIndexOf(separator);
        return index >= 0 ? value[(index + 1)..] : value;
    }

    public static string GetValue(Dictionary<string, string> dict, string key)
    {
        if (dict == null || string.IsNullOrEmpty(key)) return null;
        return dict.TryGetValue(key, out var value) ? value : null;
    }

    public static ItemIcon FindIcon(string itemPathOrName, string expectedType)
    {
        if (string.IsNullOrEmpty(itemPathOrName) || string.IsNullOrEmpty(expectedType))
            return null;

        RebuildIconCacheIfNeeded();

        string cleanId = GetCleanId(itemPathOrName);

        if (IconCache.TryGetValue($"{expectedType}|{cleanId}", out var fullMatch))
            return fullMatch;

        string shortName = GetLastToken(cleanId);
        if (IconCache.TryGetValue($"{expectedType}|{shortName}", out var shortMatch))
            return shortMatch;

        foreach (var prefix in IconCollectionNames)
        {
            string fullId = $"{prefix}.{expectedType}.{shortName}";
            if (IconCache.TryGetValue($"{expectedType}|{fullId}", out var prefixedMatch))
                return prefixedMatch;
        }

        return null;
    }

    public static EquipmentSlotCache GetSlotCache(GameObject slot)
        => slot != null ? slot.GetComponent<EquipmentSlotCache>() : null;

    public static Image GetSlotImage(GameObject slot)
    {
        var cache = GetSlotCache(slot);
        return cache != null && cache.iconImage != null
            ? cache.iconImage
            : slot != null ? slot.GetComponentInChildren<Image>(true) : null;
    }

    public static TextMeshProUGUI GetSlotLabel(GameObject slot)
    {
        var cache = GetSlotCache(slot);
        return cache != null && cache.label != null
            ? cache.label
            : slot != null ? slot.GetComponentInChildren<TextMeshProUGUI>(true) : null;
    }

    public static EquipmentSlotUI GetEquipmentSlotUI(GameObject slot)
    {
        var cache = GetSlotCache(slot);
        return cache != null && cache.equipmentSlotUI != null
            ? cache.equipmentSlotUI
            : slot != null ? slot.GetComponent<EquipmentSlotUI>() : null;
    }

    public static void ClearSlotUI(GameObject slot)
    {
        if (slot == null) return;

        Sprite defaultIcon = IconCollection.Active != null
            ? IconCollection.Active.DefaultItemIcon
            : null;

        var img = GetSlotImage(slot);
        if (img != null)
        {
            img.sprite = defaultIcon;
            img.color = Color.gray;
        }

        var tmp = GetSlotLabel(slot);
        if (tmp != null) tmp.text = string.Empty;

        var eqSlot = GetEquipmentSlotUI(slot);
        if (eqSlot != null)
            eqSlot.SetItem(string.Empty, defaultIcon, string.Empty);
    }

    public static void SetSlotUI(GameObject slot, string labelText, Sprite sprite, string itemId, string itemType)
    {
        if (slot == null) return;

        Sprite finalSprite = sprite != null ? sprite : IconCollection.Active?.DefaultItemIcon;

        var img = GetSlotImage(slot);
        if (img != null)
        {
            img.sprite = finalSprite;
            img.color = sprite != null ? Color.white : Color.gray;
        }

        var tmp = GetSlotLabel(slot);
        if (tmp != null) tmp.text = labelText ?? string.Empty;

        var eqSlot = GetEquipmentSlotUI(slot);
        if (eqSlot != null)
            eqSlot.SetItem(itemId ?? string.Empty, finalSprite, itemType ?? string.Empty);
    }

}