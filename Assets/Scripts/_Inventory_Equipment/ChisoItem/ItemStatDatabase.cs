using System.Collections.Generic;
using UnityEngine;

public class ItemStatDatabase : MonoBehaviour
{
    public static ItemStatDatabase Instance { get; private set; }

    // Cache nhanh
    private Dictionary<string, ItemStats> _statsByStringId = new Dictionary<string, ItemStats>();
    private Dictionary<int, ItemStats> _statsByIntId = new Dictionary<int, ItemStats>();

    private bool _isInitialized = false;

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
        if (_isInitialized) return;

        ItemStats[] allStats = Resources.LoadAll<ItemStats>("ItemStats");

        foreach (var stat in allStats)
        {
            if (stat == null) continue;

            if (!string.IsNullOrEmpty(stat.itemId))
                _statsByStringId[stat.itemId] = stat;

            _statsByIntId[stat.Item_ID] = stat;
        }

        _isInitialized = true;
        Debug.Log($"[ItemStatDatabase] Đã load {allStats.Length} item stats vào cache.");
    }

    // ====================== CÁC METHOD CŨ (để tương thích) ======================

    public ItemStats GetStats(string stringId)
    {
        if (string.IsNullOrEmpty(stringId)) return null;
        _statsByStringId.TryGetValue(stringId, out var stats);
        return stats;
    }

    public ItemStats GetStatsdtb(int itemId)
    {
        if (itemId <= 0) return null;
        _statsByIntId.TryGetValue(itemId, out var stats);
        return stats;
    }

    /// <summary>
    /// Method cũ trong InventoryManager.cs
    /// </summary>
    public string GetStringIdFromInt(int intId)
    {
        if (_statsByIntId.TryGetValue(intId, out var stats) && stats != null && !string.IsNullOrEmpty(stats.itemId))
            return stats.itemId;

        return null;
    }

    /// <summary>
    /// Method cũ trong EquipmentStatManager.cs
    /// </summary>
    public ItemStats GetStatsByIntId(int id)
    {
        return GetStatsdtb(id);
    }

    /// <summary>
    /// Method cũ trong EquipmentStatManager.cs
    /// </summary>
    public ItemStats GetStatsByStringId(string id)
    {
        return GetStats(id);
    }

    // ====================== DEBUG ======================
    public void LogCacheStatus()
    {
        Debug.Log($"[ItemStatDatabase] Cache: {_statsByStringId.Count} string IDs | {_statsByIntId.Count} int IDs");
    }
}