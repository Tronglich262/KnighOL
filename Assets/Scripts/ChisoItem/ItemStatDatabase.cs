using UnityEngine;
using System.Collections.Generic;

// Quản lý lookup chỉ số item cho CharacterUI, ưu tiên theo Item_ID (int)
public class ItemStatDatabase : MonoBehaviour
{
    public static ItemStatDatabase Instance;

    private Dictionary<int, ItemStats> dictById;
    private Dictionary<string, ItemStats> dict;

    void Awake()
    {
        Instance = this;
        dictById = new Dictionary<int, ItemStats>();
        dict = new Dictionary<string, ItemStats>();
        var allStats = Resources.LoadAll<ItemStats>("ItemStats");
        foreach (var stat in allStats)
        {
            dictById[stat.Item_ID] = stat;
            if (!string.IsNullOrEmpty(stat.itemId))
                dict[stat.itemId] = stat;
        }
    }

    // Lookup CHUẨN cho mọi xử lý inventory, backend, UI (nên dùng)
    public ItemStats GetStatsdtb(int itemId)
    {
        dictById.TryGetValue(itemId, out var stats);
        return stats;
    }

    // Lookup theo mã string cũ, để kiểm tra đặc biệt
    public ItemStats GetStats(string id)
    {
        if (string.IsNullOrEmpty(id))
        {
            Debug.LogError("[ItemStatDatabase] GetStats bị truyền NULL hoặc EMPTY id!");
            return null;
        }
        dict.TryGetValue(id, out var stats);
        if (stats == null)
        {
            Debug.LogWarning($"[ItemStatDatabase] Không tìm thấy ItemStats với id: {id}");
        }
        return stats;
    }

    //code cua tuấn anh
    public List<ItemStats> GetAll()
    {
        return new List<ItemStats>(dictById.Values);
    }
    public string GetStringIdFromInt(int itemId)
    {
        if (dictById.TryGetValue(itemId, out var stats))
        {
            return stats.itemId; // trả về chuỗi chuẩn để dùng lookup
        }
        return null;
    }
}
