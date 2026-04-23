using UnityEngine;
using Fusion;

public class ItemPickup : MonoBehaviour
{
    public string itemId; 
    public int quantity = 1;

    private static int localItemHCCount = 0;
    private static bool missionCompleted = false;
    private iteminfo info;  

    private void Awake()
    {
        info = GetComponent<iteminfo>();
    }

    private void OnTriggerEnter(Collider other)
    {
        // Chỉ cho phép player nhặt
        if (!other.CompareTag("Player")) return;

        var netObj = other.GetComponent<NetworkObject>();
        if (netObj == null || !netObj.HasInputAuthority) return; 

        // 1. Thêm item vào inventory (tự lưu lên server nếu InventoryManager đã setup đúng)
        InventoryManager.Instance.AddItem(itemId, quantity);

        // 2. Lấy itemId dạng int để báo nhiệm vụ
        int itemIdInt = 0;
        if (info != null && info.Itemid > 0)
        {
            itemIdInt = info.Itemid;
        }
        else if (!int.TryParse(itemId, out itemIdInt) && !string.IsNullOrEmpty(itemId))
        {
            var stat = ItemStatDatabase.Instance.GetStats(itemId);
            if (stat != null) itemIdInt = stat.Item_ID;
            else
            {
                Debug.LogWarning($"Không convert được itemId '{itemId}' sang int.");
            }
        }

        // 3. Báo nhiệm vụ "CollectItem" với itemId thực tế
        if (itemIdInt > 0)
        {
            AuthManager.Instance?.UpdateQuestProgress("CollectItem", itemIdInt, quantity);
        }

        // 4. Logic nhiệm vụ cũ (item đặc biệt, ví dụ nhiệm vụ test nhặt 5 HC)
        if (CompareTag("ItemHC"))
        {
            if (!missionCompleted)
            {
                localItemHCCount++;
                Debug.Log($"[Client] Đã nhặt {localItemHCCount}/5 item HC");
                if (localItemHCCount >= 5)
                {
                    missionCompleted = true;
                }
            }
        }

        Destroy(gameObject);
    }
}
