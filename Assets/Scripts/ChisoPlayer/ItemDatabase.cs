using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using System;

public class ItemDatabase : MonoBehaviour
{
    public static ItemDatabase Instance;
    public List<ItemStats> items = new List<ItemStats>();

    private Dictionary<string, List<ItemStats>> itemDict;

    private void Awake()
    {
        Instance = this;
        LoadAllItemStats(); // <--- thêm dòng này

        itemDict = new Dictionary<string, List<ItemStats>>();

        foreach (var item in items)
        {
            string fullId = item.itemId.Trim();
            string shortId = fullId.Split('.').Last();

            // Thêm fullId
            if (!itemDict.ContainsKey(fullId))
            {
                itemDict[fullId] = new List<ItemStats>();
            }
            itemDict[fullId].Add(item);

            // Thêm shortId (phần tên item)
            if (!itemDict.ContainsKey(shortId))
            {
                itemDict[shortId] = new List<ItemStats>();
            }
            itemDict[shortId].Add(item);
        }
    }

    // Hàm này sẽ tự động load tất cả ItemStats ScriptableObject từ Resources/ItemStats
    private void LoadAllItemStats()
    {
        items = Resources.LoadAll<ItemStats>("ItemStats").ToList();
        Debug.Log($"[ItemDatabase] Đã load {items.Count} item từ Resources/ItemStats");
    }

    public ItemStats GetItemStatsById(string id, string expectedType = null)
    {
        if (itemDict.TryGetValue(id, out var statsList))
        {
            if (!string.IsNullOrEmpty(expectedType))
            {
                return statsList.FirstOrDefault(s => s.Type == expectedType)
                       ?? statsList.FirstOrDefault();
            }
            return statsList.FirstOrDefault();
        }

        // Debug.LogWarning($"[ItemDatabase] Không tìm thấy itemId: {id} Type:  {expectedType}");
        return null;
    }
}
