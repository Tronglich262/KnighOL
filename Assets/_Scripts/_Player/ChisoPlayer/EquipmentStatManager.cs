using System.Collections.Generic;
using UnityEngine;
using Newtonsoft.Json;

public class EquipmentStatManager : MonoBehaviour
{
    public List<ItemStats> equippedItems = new();

    private CharacterStats stats;

    private static readonly string[] StatSlots =
    {
        EquipKeys.Helmet,
        EquipKeys.Armor,
        EquipKeys.Vest,
        EquipKeys.Pauldrons,
        EquipKeys.Gloves,
        EquipKeys.Boots,
        EquipKeys.Shield,
        EquipKeys.Cape,
        EquipKeys.Mask,
        EquipKeys.Glasses,
        EquipKeys.Belt,
        EquipKeys.Back,
        EquipKeys.Hair,
        EquipKeys.Bow,
        EquipKeys.MeleeWeapon1H,
        EquipKeys.MeleeWeapon2H
    };

    private void Awake()
    {
        stats = GetComponent<CharacterStats>();
    }

    public void LoadFromCharacterJson(string json)
    {
        equippedItems.Clear();

        if (string.IsNullOrEmpty(json))
        {
            Recalculate();
            return;
        }

        var dict = JsonConvert.DeserializeObject<Dictionary<string, string>>(json);
        if (dict == null)
        {
            Recalculate();
            return;
        }

        foreach (var slot in StatSlots)
        {
            if (!dict.TryGetValue(slot, out string itemId))
                continue;

            if (string.IsNullOrWhiteSpace(itemId))
                continue;

            ItemStats item = FindItemStats(itemId);
            if (item != null)
                equippedItems.Add(item);
        }

        Recalculate();
    }

    public void Equip(ItemStats item)
    {
        if (item == null || string.IsNullOrEmpty(item.Type))
            return;

        equippedItems.RemoveAll(i => i != null && i.Type == item.Type);
        equippedItems.Add(item);
        Recalculate();
    }

    public void Unequip(string type)
    {
        if (string.IsNullOrEmpty(type))
            return;

        equippedItems.RemoveAll(i => i != null && i.Type == type);
        Recalculate();
    }

    private void Recalculate()
    {
        if (stats != null)
            stats.RecalculateStatsFromEquipment(equippedItems);

        ThongTin.instance?.UpdateStatsUI();
    }

    private ItemStats FindItemStats(string itemId)
    {
        if (ItemStatDatabase.Instance == null || string.IsNullOrWhiteSpace(itemId))
            return null;

        if (int.TryParse(itemId, out int intId))
            return ItemStatDatabase.Instance.GetStatsByIntId(intId);

        return ItemStatDatabase.Instance.GetStatsByStringId(itemId);
    }
}