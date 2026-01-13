using System.Collections.Generic;
using UnityEngine;
using Newtonsoft.Json;

public class EquipmentStatManager : MonoBehaviour
{
    public List<ItemStats> equippedItems = new();
    private CharacterStats stats;

    void Awake()
    {
        stats = GetComponent<CharacterStats>();
    }

    // 🔥 GỌI KHI LOGIN
    public void LoadFromCharacterJson(string json)
    {
        equippedItems.Clear();
        if (string.IsNullOrEmpty(json)) return;

        var dict = JsonConvert.DeserializeObject<Dictionary<string, string>>(json);

        foreach (var pair in dict)
        {
            if (string.IsNullOrEmpty(pair.Value)) continue;

            ItemStats item = FindItemStats(pair.Value);
            if (item != null)
                equippedItems.Add(item);
        }

        Recalculate();
    }

    // 🔥 GỌI KHI MẶC / THÁO ĐỒ
    public void Equip(ItemStats item)
    {
        equippedItems.RemoveAll(i => i.Type == item.Type);
        equippedItems.Add(item);
        Recalculate();
    }

    public void Unequip(string type)
    {
        equippedItems.RemoveAll(i => i.Type == type);
        Recalculate();
    }

    void Recalculate()
    {
        stats.RecalculateStatsFromEquipment(equippedItems);
        ThongTin.instance?.UpdateStatsUI();
    }

    ItemStats FindItemStats(string itemId)
    {
        var all = Resources.LoadAll<ItemStats>("ItemStats");
        foreach (var i in all)
            if (i.itemId == itemId || i.Item_ID.ToString() == itemId)
                return i;
        return null;
    }
}
