using UnityEngine;
using Fusion;

public class ItemPickup : MonoBehaviour
{
    public string itemId; // ID trùng với ItemStats
    public int quantity = 1;

    private static int localItemHCCount = 0; // Sử dụng static để giữ đếm xuyên suốt game session (trong client)
    private static bool missionCompleted = false;

    private static UpdateMission _mission;

    private void Awake()
    {
        // Tìm và gán UpdateMission nếu chưa có
        if (_mission == null)
        {
            _mission = FindFirstObjectByType<UpdateMission>();
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player")) return;

        // Kiểm tra HasInputAuthority để đảm bảo chỉ player local nhặt
        var netObj = other.GetComponent<NetworkObject>();
        if (netObj == null || !netObj.HasInputAuthority) return;

        // Thêm item vào túi
        InventoryManager.Instance.AddItem(itemId, quantity);

        // Nếu là item HC, đếm và xử lý
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

                    _mission?.slotItemHc();
                }
            }
        }
        
        Destroy(gameObject);
    }
}