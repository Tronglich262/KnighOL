using Fusion;
using UnityEngine;

public class PlayerNetwork : NetworkBehaviour
{
    [Rpc(RpcSources.All, RpcTargets.All)]
    public void RPC_SendPlayerData(string characterData)
    {
        Debug.Log($"Received player data: {characterData}");
        // Xử lý dữ liệu character ở đây
    }
}
