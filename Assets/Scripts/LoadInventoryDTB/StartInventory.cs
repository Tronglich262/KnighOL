using System.Collections;
using UnityEngine;

public class StartInventory : MonoBehaviour
{  
    public StartInventory Instance;
    public void Awake()
    {
            Instance = this;
    }
    IEnumerator Start()
    {
        yield return new WaitUntil(() => InventoryManager.Instance != null);

        // ĐÚNG: Lấy accountId/token từ một instance, không phải class
        InventoryManager.Instance.session.AccountId = AuthManager.Instance.UserSession.AccountId;
        InventoryManager.Instance.session.Token = AuthManager.Instance.UserSession.Token;

        InventoryManager.Instance.LoadInventory(null);
        Debug.Log($"[StartInventory] Đang load inventory cho accountId: {InventoryManager.Instance.session.AccountId}, token: {InventoryManager.Instance.session.Token}");



        // --- Load PlayerState trước khi load inventory ---
        yield return StartCoroutine(AuthManager.Instance.GetPlayerState((state) =>
        {
            if (state != null)
            {
                // Lưu lại hoặc truyền sang các hệ thống khác nếu muốn
                PlayerDataHolder1.CurrentPlayerState = state;
                Debug.Log($"[StartInventory] PlayerState: Level={state.level}, Exp={state.exp}, Gold={state.gold}, Diamond={state.diamond}");
            }
            else
            {
                Debug.LogError("[StartInventory] Không load được PlayerState!");
            }
        }));

        // Sau khi đã có PlayerState, load inventory như bình thường
        InventoryManager.Instance.LoadInventory(null);
        Debug.Log($"[StartInventory] Đang load inventory cho accountId: {InventoryManager.Instance.session.AccountId}, token: {InventoryManager.Instance.session.Token}");
    }
}

