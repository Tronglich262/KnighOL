using System.Collections.Generic;
using UnityEngine;

public class ItemStatDatabase : MonoBehaviour
{
    public static ItemStatDatabase Instance { get; private set; }

    private readonly Dictionary<int, ItemStats> dictById = new();
    private readonly Dictionary<string, ItemStats> dictByString = new();

    [SerializeField] private string resourcesPath = "ItemStats";

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        LoadDatabase();
    }

    private void LoadDatabase()
    {
        dictById.Clear();
        dictByString.Clear();

        ItemStats[] allStats = Resources.LoadAll<ItemStats>(resourcesPath);
        foreach (var stat in allStats)
        {
            if (stat == null) continue;

            dictById[stat.Item_ID] = stat;

            if (!string.IsNullOrWhiteSpace(stat.itemId))
                dictByString[stat.itemId] = stat;
        }

        Debug.Log($"[ItemStatDatabase] Loaded {dictById.Count} items from Resources/{resourcesPath}");
    }

    public ItemStats GetStatsByIntId(int itemId)
    {
        dictById.TryGetValue(itemId, out var stats);
        return stats;
    }

    public ItemStats GetStatsByStringId(string itemId)
    {
        if (string.IsNullOrWhiteSpace(itemId)) return null;
        dictByString.TryGetValue(itemId, out var stats);
        return stats;
    }
    // ====== BACKWARD COMPATIBILITY (GIỮ CODE CŨ KHÔNG BỊ LỖI) ======

    public List<ItemStats> GetAll()
    {
        return new List<ItemStats>(dictById.Values);
    }

    public ItemStats GetStats(string id)
    {
        return GetStatsByStringId(id);
    }

    public ItemStats GetStatsdtb(int itemId)
    {
        return GetStatsByIntId(itemId);
    }

    public string GetStringIdFromInt(int itemId)
    {
        if (dictById.TryGetValue(itemId, out var stats))
            return stats.itemId;

        return null;
    }
}