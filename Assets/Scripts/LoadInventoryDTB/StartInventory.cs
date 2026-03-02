using System.Collections;
using UnityEngine;

public class StartInventory : MonoBehaviour
{
    public static StartInventory Instance;
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

        // Chờ LOCAL player spawn (client join sau phải init đúng object của mình)
        yield return new WaitUntil(() => PlayerSpawner.LocalPlayerObject != null);

        // ----> Load PlayerStats từ server, gán vào LOCAL player
        yield return StartCoroutine(AuthManager.Instance.GetPlayerStats(stats =>
        {
            if (stats != null)
            {
                GameObject player = PlayerSpawner.LocalPlayerObject != null ? PlayerSpawner.LocalPlayerObject.gameObject : null;
                if (player != null)
                {
                    var cs = player.GetComponent<CharacterStats>();
                    if (cs != null)
                    {
                        cs.InitFromPlayerStats(stats);
                        Debug.Log("[StartInventory] Gán PlayerStats vào CharacterStats thành công.");
                    }
                    else
                    {
                        Debug.LogError("[StartInventory] Player không có component CharacterStats!");
                    }
                }
                else
                {
                    Debug.LogError("[StartInventory] LocalPlayerObject chưa có.");
                }
            }
            else
            {
                Debug.LogError("[StartInventory] Không lấy được PlayerStats từ server!");
            }
        }));

        // Sau khi đã có PlayerState, load inventory như bình thường
        InventoryManager.Instance.LoadInventory(null);
        Debug.Log($"[StartInventory] Đang load inventory cho accountId: {InventoryManager.Instance.session.AccountId}, token: {InventoryManager.Instance.session.Token}");

        // Tùy bạn: Gọi update UI sau khi xong mọi thứ
        // ThongTin.instance?.UpdateStatsUI();
    }
}
