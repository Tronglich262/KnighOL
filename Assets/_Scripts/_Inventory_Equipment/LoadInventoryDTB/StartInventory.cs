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
        // Chá» InventoryManager vÃ  SessionManager sáºµn sÃ ng
        yield return new WaitUntil(() => InventoryManager.Instance != null);
        yield return new WaitUntil(() => SessionManager.HasValidSession());

        Debug.Log($"[StartInventory] Dang load inventory cho accountId: {SessionManager.AccountId}");

        // Load PlayerState trÆ°á»›c
        yield return StartCoroutine(AuthManager.GetOrCreate().GetPlayerState((state) =>
        {
            if (state != null)
            {
                PlayerDataHolder1.CurrentPlayerState = state;
                Debug.Log($"[StartInventory] PlayerState: Level={state.level}, Exp={state.exp}, Gold={state.gold}, Diamond={state.diamond}");
            }
            else
            {
                Debug.LogError("[StartInventory] Khong load duoc PlayerState!");
            }
        }));

        // Chá» local player spawn
        yield return new WaitUntil(() => PlayerSpawner.LocalPlayerObject != null);

        // Load PlayerStats rá»“i gÃ¡n vÃ o local player
        yield return StartCoroutine(AuthManager.GetOrCreate().GetPlayerStats(stats =>
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
                        Debug.Log("[StartInventory] Gan PlayerStats vao CharacterStats thanh cong.");
                    }
                    else
                    {
                        Debug.LogError("[StartInventory] Player khong co component CharacterStats!");
                    }
                }
                else
                {
                    Debug.LogError("[StartInventory] LocalPlayerObject chua co.");
                }
            }
            else
            {
                Debug.LogError("[StartInventory] Khong lay duoc PlayerStats tu server!");
            }
        }));

        // Load inventory sau khi state/stats Ä‘Ã£ sáºµn sÃ ng
        InventoryManager.Instance.LoadInventory(null);
        Debug.Log($"[StartInventory] Load inventory xong cho accountId: {SessionManager.AccountId}");
    }
}