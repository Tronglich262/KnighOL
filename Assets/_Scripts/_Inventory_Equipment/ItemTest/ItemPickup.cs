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
        // Chá»‰ cho phÃ©p player nháº·t
        if (!other.CompareTag("Player")) return;

        var netObj = other.GetComponent<NetworkObject>();
        if (netObj == null || !netObj.HasInputAuthority) return; 

        // 1. ThÃªm item vÃ o inventory (tá»± lÆ°u lÃªn server náº¿u InventoryManager Ä‘Ã£ setup Ä‘Ãºng)
        InventoryManager.Instance.AddItem(itemId, quantity);

        // 2. Láº¥y itemId dáº¡ng int Ä‘á»ƒ bÃ¡o nhiá»‡m vá»¥
        int itemIdInt = 0;
        if (info != null && info.Itemid > 0)
        {
            itemIdInt = info.Itemid;
        }
        else if (!int.TryParse(itemId, out itemIdInt) && !string.IsNullOrEmpty(itemId))
        {
            var stat = ItemStatDatabase.GetOrCreate().GetStats(itemId);
            if (stat != null) itemIdInt = stat.Item_ID;
            else
            {
                Debug.LogWarning($"Khong convert duoc itemId '{itemId}' sang int.");
            }
        }

        // 3. BÃ¡o nhiá»‡m vá»¥ "CollectItem" vá»›i itemId thá»±c táº¿
        if (itemIdInt > 0)
        {
            AuthManager.GetOrCreate()?.UpdateQuestProgress("CollectItem", itemIdInt, quantity);
        }

        // 4. Logic nhiá»‡m vá»¥ cÅ© (item Ä‘áº·c biá»‡t, vÃ­ dá»¥ nhiá»‡m vá»¥ test nháº·t 5 HC)
        if (CompareTag("ItemHC"))
        {
            if (!missionCompleted)
            {
                localItemHCCount++;
                Debug.Log($"[Client] Da nhat {localItemHCCount}/5 item HC");
                if (localItemHCCount >= 5)
                {
                    missionCompleted = true;
                }
            }
        }

        Destroy(gameObject);
    }
}
