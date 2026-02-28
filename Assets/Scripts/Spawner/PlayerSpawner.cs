using Assets.HeroEditor.Common.CharacterScripts;
using Fusion;
using Fusion.Sockets;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Rendering;

public class PlayerSpawner : SimulationBehaviour, INetworkRunnerCallbacks
{
    public NetworkObject playerPrefab;
    public GameObject characterCanvasPrefab;
    public static NetworkObject LocalPlayerObject;

    // Danh sách vị trí spawn có thể có
    public Transform[] spawnPoints;

    public void OnPlayerJoined(NetworkRunner runner, PlayerRef player)
    {
        // Debug: Log số lượng player đang có
        // Debug.Log($"[PlayerSpawner] Player joined: {player}, Total players: {runner.ActivePlayers.Count()}");

        var sync = FindFirstObjectByType<NicknameSyncManager>();
        if (sync != null) sync.OnPlayerJoined(runner, player);

        if (player == runner.LocalPlayer)
        {
            // UI
            var canvas = Instantiate(characterCanvasPrefab);
            canvas.SetActive(true);
            if (canvas.GetComponent<LocalPlayerStatsLoader>() == null)
                canvas.AddComponent<LocalPlayerStatsLoader>();
            InventoryManager.Instance.uiManager = canvas.GetComponentInChildren<InventoryUIManager>();

            // Tính vị trí spawn dựa trên số thứ tự player
            int playerIndex = runner.ActivePlayers.Count() - 1;
            Vector3 spawnPosition;
            Quaternion spawnRotation = Quaternion.identity;

            // Nếu có spawnPoints array, sử dụng chúng
            if (spawnPoints != null && spawnPoints.Length > 0)
            {
                // Lấy vị trí spawn theo index (loop nếu vượt quá số điểm)
                int spawnIndex = playerIndex % spawnPoints.Length;
                spawnPosition = spawnPoints[spawnIndex].position;
                Debug.Log($"[PlayerSpawner] Using spawn point {spawnIndex} for player {playerIndex}");
            }
            else
            {
                // Fallback: Spawn lệch nhau theo khoảng cách ngẫu nhiên
                float offsetX = (playerIndex % 4) * 3f; // Mỗi player lệch 3 đơn vị
                float offsetY = (playerIndex / 4) * 3f;
                spawnPosition = new Vector3(offsetX, -7.02f + offsetY, 0);
                Debug.Log($"[PlayerSpawner] Using offset spawn: {spawnPosition} for player {playerIndex}");
            }

            // Tạo dữ liệu spawn từ thông tin người chơi hiện tại
            PlayerSpawnData spawnData = new PlayerSpawnData
            {
                DisplayName = PlayerDataHolder1.PlayerName
            };

            // Truyền vào runner.Spawn
            NetworkObject obj = runner.Spawn(playerPrefab, spawnPosition, spawnRotation, player);

            // Local player object để ThongTin/StartInventory/BuffSkillNetwork luôn init đúng nhân vật mình
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

                // Gửi tên lên server
                avatar.RPC_SendDisplayNameToServer(PlayerDataHolder1.PlayerName);
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
