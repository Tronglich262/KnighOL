using UnityEngine;
using Fusion;

public class ItemPickup : MonoBehaviour
{
    public string itemId; // ID dạng string, ví dụ "Sword01" hoặc "123"
    public int quantity = 1;

    private static int localItemHCCount = 0;
    private static bool missionCompleted = false;
    private iteminfo info;  // Component chứa dữ liệu item nếu có

    private static UpdateMission _mission;

    private void Awake()
    {
        info = GetComponent<iteminfo>();

        if (_mission == null)
        {
            _mission = FindFirstObjectByType<UpdateMission>();
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        // Chỉ cho phép player nhặt
        if (!other.CompareTag("Player")) return;

        var netObj = other.GetComponent<NetworkObject>();
        if (netObj == null || !netObj.HasInputAuthority) return; // Chỉ local player nhặt

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
            // Nếu itemId là string, tra sang int trong database
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
                    Debug.Log("🎉 [Client] Hoàn thành nhiệm vụ nhặt 5 item HC");
                    _mission?.slotItemHc(); // Gọi hàm hiện popup/hoàn thành nhiệm vụ
                }
            }
        }

        Destroy(gameObject); // Xoá item trên map sau khi nhặt
    }
}
