using UnityEngine;
using UnityEngine.UI;
using TMPro; // ✅ Thêm dòng này nếu bạn dùng TextMeshPro
using System.Collections.Generic;

public class InventoryUIManager : MonoBehaviour
{
    public Transform gridParent;
    public GameObject slotPrefab;
    public List<InventoryItem1> testItems;
    public static InventoryUIManager instance;
    public void Awake()
    {
        instance = this;
 
    }
    void Start()
    {
        DisplayInventory(testItems);
    }
    private ItemStats LoadStatsById(string id)
    {
        var allStats = Resources.LoadAll<ItemStats>("ItemStats");
        return System.Array.Find(allStats, s => s.itemId == id);
    }


    public void DisplayInventory(List<InventoryItem1> items)
    {
        foreach (Transform child in gridParent)
            Destroy(child.gameObject);

        foreach (var item in items)
        {
            // Load stats nếu chưa có
            if (item.stats == null)
            {
                item.stats = LoadStatsById(item.itemId);
                if (item.stats == null)
                {
                    Debug.LogError($"❌ Không tìm thấy ItemStats cho itemId: {item.itemId}");
                    continue;
                }
            }

            GameObject slot = Instantiate(slotPrefab, gridParent);
            InventorySlotUI slotUI = slot.GetComponent<InventorySlotUI>();

            if (slotUI != null)
                slotUI.Init(item);
        }
    }
}
