using System.Collections;
using UnityEngine;

public class StartInventory : MonoBehaviour
{  
    IEnumerator Start()
    {
        yield return new WaitUntil(() => InventoryManager.Instance != null);

        // ĐÚNG: Lấy accountId/token từ một instance, không phải class
        InventoryManager.Instance.session.AccountId = AuthManager.Instance.UserSession.AccountId;
        InventoryManager.Instance.session.Token = AuthManager.Instance.UserSession.Token;

        InventoryManager.Instance.LoadInventory(null);
        Debug.Log($"[StartInventory] Đang load inventory cho accountId: {InventoryManager.Instance.session.AccountId}, token: {InventoryManager.Instance.session.Token}");

    }
}
