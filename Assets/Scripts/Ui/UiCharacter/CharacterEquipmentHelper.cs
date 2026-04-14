using Assets.HeroEditor.Common.CommonScripts;
using System.Collections.Generic;
using System.Linq;
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

    public static string GetCleanId(string value)
    {
        if (string.IsNullOrEmpty(value)) return null;

        int hashIndex = value.IndexOf('#');
        return hashIndex >= 0 ? value.Substring(0, hashIndex).Trim() : value.Trim();
    }

    public static string GetLastToken(string value, char separator = '.')
    {
        if (string.IsNullOrEmpty(value)) return value;

        int index = value.LastIndexOf(separator);
        return index >= 0 ? value.Substring(index + 1) : value;
    }

    public static string GetValue(Dictionary<string, string> dict, string key)
    {
        if (dict == null || string.IsNullOrEmpty(key))
            return null;

        return dict.TryGetValue(key, out string value) ? value : null;
    }

    public static ItemIcon FindIcon(string itemName, string expectedType)
    {
        if (string.IsNullOrEmpty(itemName) || string.IsNullOrEmpty(expectedType))
            return null;

        var icons = IconCollection.Active.Icons;

        for (int i = 0; i < IconCollectionNames.Length; i++)
        {
            string fullId = $"{IconCollectionNames[i]}.{expectedType}.{itemName}";
            var icon = icons.FirstOrDefault(x => x.Type == expectedType && x.Id == fullId);
            if (icon != null)
                return icon;
        }

        return null;
    }

    public static EquipmentSlotCache GetSlotCache(GameObject slot)
    {
        return slot != null ? slot.GetComponent<EquipmentSlotCache>() : null;
    }

    public static Image GetSlotImage(GameObject slot)
    {
        var cache = GetSlotCache(slot);
        if (cache != null && cache.iconImage != null)
            return cache.iconImage;

        return slot != null ? slot.GetComponentInChildren<Image>(true) : null;
    }

    public static TextMeshProUGUI GetSlotLabel(GameObject slot)
    {
        var cache = GetSlotCache(slot);
        if (cache != null && cache.label != null)
            return cache.label;

        return slot != null ? slot.GetComponentInChildren<TextMeshProUGUI>(true) : null;
    }

    public static EquipmentSlotUI GetEquipmentSlotUI(GameObject slot)
    {
        var cache = GetSlotCache(slot);
        if (cache != null && cache.equipmentSlotUI != null)
            return cache.equipmentSlotUI;

        return slot != null ? slot.GetComponent<EquipmentSlotUI>() : null;
    }

    public static void ClearSlotUI(GameObject slot)
    {
        if (slot == null) return;

        var img = GetSlotImage(slot);
        if (img != null)
        {
            img.sprite = IconCollection.Active.DefaultItemIcon;
            img.color = Color.gray;
        }

        var tmp = GetSlotLabel(slot);
        if (tmp != null)
            tmp.text = "";

        var eqSlot = GetEquipmentSlotUI(slot);
        if (eqSlot != null)
            eqSlot.SetItem("", IconCollection.Active.DefaultItemIcon, "");
    }

    public static void SetSlotUI(GameObject slot, string labelText, Sprite sprite, string itemId, string itemType)
    {
        if (slot == null) return;

        var img = GetSlotImage(slot);
        if (img != null)
        {
            img.sprite = sprite != null ? sprite : IconCollection.Active.DefaultItemIcon;
            img.color = sprite != null ? Color.white : Color.gray;
        }

        var tmp = GetSlotLabel(slot);
        if (tmp != null)
            tmp.text = labelText ?? "";

        var eqSlot = GetEquipmentSlotUI(slot);
        if (eqSlot != null)
        {
            eqSlot.SetItem(
                itemId ?? "",
                sprite != null ? sprite : IconCollection.Active.DefaultItemIcon,
                itemType ?? ""
            );
        }
    }
}