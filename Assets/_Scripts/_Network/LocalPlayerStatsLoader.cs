using System.Collections;
using UnityEngine;

/// <summary>
/// Gan len canvas active khi vao game. Load chi so cho local player khi co LocalPlayerObject.
/// Khong can bat panel thong tin. Chay tren object luon active de dam bao load duoc.
/// </summary>
public class LocalPlayerStatsLoader : MonoBehaviour
{
    public static bool StatsLoaded { get; private set; }

    void Start()
    {
        StartCoroutine(LoadWhenReady());
    }

    IEnumerator LoadWhenReady()
    {
        while (PlayerSpawner.LocalPlayerObject == null)
            yield return null;

        yield return null;

        if (AuthManager.GetOrCreate() == null)
        {
            Debug.LogWarning("[LocalPlayerStatsLoader] AuthManager chua co.");
            yield break;
        }

        var player = PlayerSpawner.LocalPlayerObject.gameObject;
        var charStats = player.GetComponent<CharacterStats>();
        if (charStats == null)
        {
            Debug.LogWarning("[LocalPlayerStatsLoader] Player khong co CharacterStats.");
            yield break;
        }

        StatsLoaded = false;

        yield return StartCoroutine(AuthManager.GetOrCreate().GetPlayerStats(result =>
        {
            if (result != null)
                charStats.InitFromPlayerStats(result);
        }));

        var equipMgr = player.GetComponent<EquipmentStatManager>();
        if (equipMgr != null)
            equipMgr.LoadFromCharacterJson(PlayerDataHolder1.CharacterJson);
        else
            Debug.LogWarning("[LocalPlayerStatsLoader] Player thiáº¿u EquipmentStatManager.");

        charStats.currentMana = charStats.maxMana;

        StatsLoaded = true;

        if (ThongTin.instance != null)
            ThongTin.instance.UpdateStatsUI();
    }
}
