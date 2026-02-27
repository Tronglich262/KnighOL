using System.Collections;
using UnityEngine;

/// <summary>
/// Gắn lên canvas (active khi vào game). Load chỉ số cho local player ngay khi có LocalPlayerObject,
/// không cần bật panel Thông tin. Chạy trên object luôn active nên đảm bảo load được.
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

        if (AuthManager.Instance == null)
        {
            Debug.LogWarning("[LocalPlayerStatsLoader] AuthManager chưa có.");
            yield break;
        }

        var player = PlayerSpawner.LocalPlayerObject.gameObject;
        var charStats = player.GetComponent<CharacterStats>();
        if (charStats == null)
        {
            Debug.LogWarning("[LocalPlayerStatsLoader] Player không có CharacterStats.");
            yield break;
        }

        StatsLoaded = false;

        yield return StartCoroutine(AuthManager.Instance.GetPlayerStats(result =>
        {
            if (result != null)
                charStats.InitFromPlayerStats(result);
        }));

        var equipMgr = player.GetComponent<EquipmentStatManager>();
        if (equipMgr != null)
            equipMgr.LoadFromCharacterJson(PlayerDataHolder1.CharacterJson);
        else
            Debug.LogWarning("[LocalPlayerStatsLoader] Player thiếu EquipmentStatManager.");

        StatsLoaded = true;

        if (ThongTin.instance != null)
            ThongTin.instance.UpdateStatsUI();
    }
}
