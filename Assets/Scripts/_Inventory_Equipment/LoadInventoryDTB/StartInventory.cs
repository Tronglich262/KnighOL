using System.Collections;
using UnityEngine;

public class StartInventory : MonoBehaviour
{
    public static StartInventory Instance;

    private void Awake()
    {
        Instance = this;
    }

    private IEnumerator Start()
    {
        // Chờ InventoryManager và SessionManager sẵn sàng
        yield return new WaitUntil(() => InventoryManager.Instance != null);
        yield return new WaitUntil(() => SessionManager.HasValidSession());

        Debug.Log($"[StartInventory] Đang load inventory cho accountId: {SessionManager.AccountId}");

        // Load PlayerState trước
        yield return StartCoroutine(AuthManager.Instance.GetPlayerState((state) =>
        {
            if (state != null)
            {
                PlayerDataHolder1.CurrentPlayerState = state;
                Debug.Log($"[StartInventory] PlayerState: Level={state.level}, Exp={state.exp}, Gold={state.gold}, Diamond={state.diamond}");
            }
            else
            {
                Debug.LogError("[StartInventory] Không load được PlayerState!");
            }
        }));

        // Chờ local player spawn
        yield return new WaitUntil(() => PlayerSpawner.LocalPlayerObject != null);

        // Load PlayerStats rồi gán vào local player
        yield return StartCoroutine(AuthManager.Instance.GetPlayerStats(stats =>
        {
            if (stats != null)
            {
                GameObject player = PlayerSpawner.LocalPlayerObject != null
                    ? PlayerSpawner.LocalPlayerObject.gameObject
                    : null;

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

        // Load inventory sau khi state/stats đã sẵn sàng
        InventoryManager.Instance.LoadInventory(null);
        Debug.Log($"[StartInventory] Load inventory xong cho accountId: {SessionManager.AccountId}");
    }
}