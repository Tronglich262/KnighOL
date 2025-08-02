using Assets.HeroEditor.Common.CharacterScripts;
using Fusion;
using Fusion.Sockets;
using System;
using System.Collections.Generic;
using UnityEngine;

public class PlayerSpawner : SimulationBehaviour, INetworkRunnerCallbacks
{
    public NetworkObject playerPrefab;
    public GameObject characterCanvasPrefab;
    public static NetworkObject LocalPlayerObject;

    public void OnPlayerJoined(NetworkRunner runner, PlayerRef player)
    {
        var sync = FindFirstObjectByType<NicknameSyncManager>();
        if (sync != null) sync.OnPlayerJoined(runner, player);

        if (player == runner.LocalPlayer)
        {
            // UI
            var canvas = Instantiate(characterCanvasPrefab);
            canvas.SetActive(true);
            InventoryManager.Instance.uiManager = canvas.GetComponentInChildren<InventoryUIManager>();

            // Spawn
            Vector3 spawnPosition = new Vector3(0, -7.02f, 0);
            Quaternion spawnRotation = Quaternion.identity;

            NetworkObject obj = runner.Spawn(playerPrefab, spawnPosition, spawnRotation, player);
            LocalPlayerObject = obj;


            // Clone handling
            var clone = GameObject.Find("CloneUI");
            if (clone != null)
            {
                var cloneCtrl = clone.GetComponent<PlayerCloneController>();
                cloneCtrl?.SetTarget(obj);
                ItemDetailsUI.Instance.playerClone = clone;
                ItemDetailsUI.Instance.character = clone.GetComponent<Character>();
                CharacterUIManager1.Instance.character = clone.GetComponent<Character>();

                string json = PlayerDataHolder1.CharacterJson;
                clone.GetComponent<Character>().FromJson(json);
                clone.GetComponent<PlayerCloneController>().LoadJson(json);
            }
            else Debug.LogWarning("Không tìm thấy PlayerClone trong scene.");

            var avatar = obj.GetComponent<PlayerAvatar>();
            if (avatar != null)
            {
                Debug.Log("UpdateCharacterJson ban đầu");
                avatar.UpdateCharacterJson(PlayerDataHolder1.CharacterJson);
                avatar.RPC_SetDisplayName(PlayerDataHolder1.PlayerName); //  GỌI TỪ SERVER

            }

            string nickname = PlayerDataHolder1.PlayerName;

            var nameTag = obj.GetComponentInChildren<NameTagManager>();
            if (nameTag != null && obj.HasInputAuthority)
            {
                nameTag.RPC_SetNickname(nickname);
            }

            string token = PlayerDataHolder1.Token;
            if (OnlineAccountManager.Instance.OnlineTokens.TryGetValue(token, out PlayerRef oldPlayer))
            {
                if (!oldPlayer.Equals(player) && runner.TryGetPlayerObject(oldPlayer, out NetworkObject oldPlayerObj))
                {
                    oldPlayerObj.GetComponent<PlayerAvatar>()?.RPC_KickToLogin();
                }
            }
            OnlineAccountManager.Instance.OnlineTokens[token] = player;
        }
    }

    public void OnPlayerLeft(NetworkRunner runner, PlayerRef player)
    {
        foreach (var kvp in OnlineAccountManager.Instance.OnlineTokens)
        {
            if (kvp.Value == player)
            {
                OnlineAccountManager.Instance.OnlineTokens.Remove(kvp.Key);
                Debug.Log("Đã xóa token khi client rời game");
                break;
            }
        }
    }

    // Empty implementations
    public void OnInput(NetworkRunner runner, NetworkInput input) { }
    public void OnInputMissing(NetworkRunner runner, PlayerRef player, NetworkInput input) { }
    public void OnShutdown(NetworkRunner runner, ShutdownReason shutdownReason) { }
    public void OnConnectedToServer(NetworkRunner runner) { }
    public void OnConnectFailed(NetworkRunner runner, NetAddress remoteAddress, NetConnectFailedReason reason) { }
    public void OnDisconnectedFromServer(NetworkRunner runner, NetDisconnectReason reason) { }
    public void OnHostMigration(NetworkRunner runner, HostMigrationToken hostMigrationToken) { }
    public void OnObjectEnterAOI(NetworkRunner runner, NetworkObject obj, PlayerRef player) { }
    public void OnObjectExitAOI(NetworkRunner runner, NetworkObject obj, PlayerRef player) { }
    public void OnReliableDataProgress(NetworkRunner runner, PlayerRef player, ReliableKey key, float progress) { }
    public void OnReliableDataReceived(NetworkRunner runner, PlayerRef player, ReliableKey key, ArraySegment<byte> data) { }
    public void OnConnectRequest(NetworkRunner runner, NetworkRunnerCallbackArgs.ConnectRequest request, byte[] token) { }
    public void OnUserSimulationMessage(NetworkRunner runner, SimulationMessagePtr message) { }
    public void OnSessionListUpdated(NetworkRunner runner, List<SessionInfo> sessionList) { }
    public void OnSceneLoadDone(NetworkRunner runner) { }
    public void OnSceneLoadStart(NetworkRunner runner) { }
    public void OnCustomAuthenticationResponse(NetworkRunner runner, Dictionary<string, object> data) { }
}
