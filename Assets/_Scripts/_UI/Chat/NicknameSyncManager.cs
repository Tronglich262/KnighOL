using System.Collections.Generic;
using Fusion;
using UnityEngine;

public class NicknameSyncManager : NetworkBehaviour
{
    public static Dictionary<string, PlayerRef> NameToPlayerRef = new();
    public static Dictionary<PlayerRef, string> PlayerRefToName = new();

    [Rpc(RpcSources.All, RpcTargets.All)]
    public void RPC_AnnounceNickName(string nickname, PlayerRef who, RpcInfo info = default)
    {
        if (!string.IsNullOrEmpty(nickname))
        {
            NameToPlayerRef[nickname] = who;
            PlayerRefToName[who] = nickname;
            Debug.Log($"[NickSync] Đã cập nhật: {nickname} <-> {who}");
        }
    }
    public void OnPlayerJoined(NetworkRunner runner, PlayerRef player)
    {
        RPC_AnnounceNickName(PlayerDataHolder1.PlayerName, runner.LocalPlayer);
        Debug.Log($"[NickSync] Gửi lại nickname: {PlayerDataHolder1.PlayerName} - {runner.LocalPlayer}");
    }
}
