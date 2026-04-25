using System.Collections.Generic;
using UnityEngine;

public class ItemStatDatabase : MonoBehaviour
{
    public static ItemStatDatabase Instance { get; private set; }

    private Dictionary<string, ItemStats> _statsByStringId = new();
    private Dictionary<int, ItemStats> _statsByIntId = new();

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
        InitializeDatabase();
    }

    private void InitializeDatabase()
    {
        ItemStats[] allStats = Resources.LoadAll<ItemStats>("ItemStats");

        foreach (var stat in allStats)
        {
            if (stat == null) continue;

            if (!string.IsNullOrEmpty(stat.itemId))
                _statsByStringId[stat.itemId] = stat;

            _statsByIntId[stat.Item_ID] = stat;
        }

        Debug.Log($"[ItemStatDatabase] Đã load {allStats.Length} item stats thành công.");
    }

    // ==================== METHOD CŨ (GIỮ TƯƠNG THÍCH) ====================
    public ItemStats GetStatsByStringId(string stringId) => _statsByStringId.GetValueOrDefault(stringId);
    public ItemStats GetStatsByIntId(int intId) => _statsByIntId.GetValueOrDefault(intId);

    // ==================== METHOD MỚI (ĐÃ TỐI ƯU) ====================
    public ItemStats GetStats(string stringId) => GetStatsByStringId(stringId);
    public ItemStats GetStatsdtb(int itemId) => GetStatsByIntId(itemId);

    public string GetStringIdFromInt(int intId)
    {
        if (_statsByIntId.TryGetValue(intId, out var stats) && !string.IsNullOrEmpty(stats.itemId))
            return stats.itemId;
        return null;
    }

    public void LogCacheStatus()
    {
        Debug.Log($"[ItemStatDatabase] Cache: {_statsByStringId.Count} string IDs | {_statsByIntId.Count} int IDs");
    }
}