using Assets.HeroEditor.Common.CharacterScripts;
using Fusion;
using Fusion.Sockets;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerSpawner : SimulationBehaviour, INetworkRunnerCallbacks
{
    public NetworkObject playerPrefab;
    public GameObject characterCanvasPrefab;
    public static NetworkObject LocalPlayerObject;


    public void OnPlayerJoined(NetworkRunner runner, PlayerRef player)
    {
        // Bỏ điều kiện if (player == runner.LocalPlayer)
        var sync = FindFirstObjectByType<NicknameSyncManager>();
        if (sync != null)
        {
            sync.OnPlayerJoined(runner, player); // Luôn gọi ở mọi client khi có player mới vào
        }
        if (player == runner.LocalPlayer)
        {



            //  UI riêng cho client
            var canvas = Instantiate(characterCanvasPrefab);
            canvas.SetActive(true);

            // Gán UI vào hệ thống Inventory
            var uiManager = canvas.GetComponentInChildren<InventoryUIManager>();
            InventoryManager.Instance.uiManager = uiManager;

            //  Spawn nhân vật thật (Networked)
            Vector3 spawnPosition = new Vector3(0, -7.02f, 0);
            Quaternion spawnRotation = Quaternion.identity;

            NetworkObject obj = runner.Spawn(playerPrefab, spawnPosition, spawnRotation, player);
            LocalPlayerObject = obj; // Lưu lại reference player object của mình

            // Lấy các thành phần cần thiết
            var character = obj.GetComponent<Character>();
            var avatar = obj.GetComponent<PlayerAvatar>();
            var stats = obj.GetComponent<CharacterStats>();

           

            // ✅ Tìm PlayerClone đã đặt sẵn trong scene
            GameObject clone = GameObject.Find("CloneUI");
            if (clone != null)
            {
                // Gán target cho PlayerClone để sau này gửi JSON về đúng player thật
                var cloneCtrl = clone.GetComponent<PlayerCloneController>();
                if (cloneCtrl != null)
                {
                    cloneCtrl.SetTarget(obj); // Gán đúng player thật
                }

                // Gán clone vào UI để sử dụng khi Equip
                ItemDetailsUI.Instance.playerClone = clone;
                ItemDetailsUI.Instance.character = clone.GetComponent<Character>(); // dùng clone để hiển thị thử đồ
                CharacterUIManager1.Instance.character = clone.GetComponent<Character>();

                // Load dữ liệu từ account hiện tại
                string json = PlayerDataHolder1.CharacterJson;
                clone.GetComponent<Character>().FromJson(json);
                clone.GetComponent<PlayerCloneController>().LoadJson(json);



            }
            else
            {
                Debug.LogWarning("❌ Không tìm thấy PlayerClone trong scene.");
            }

            //  Gửi JSON hiện tại xuống player thật
            if (avatar != null)
            {
                Debug.Log("🟢 UpdateCharacterJson ban đầu");
                avatar.UpdateCharacterJson(PlayerDataHolder1.CharacterJson, avatar.Character);
            }


            // name
            string nickname = PlayerDataHolder1.PlayerName; // <-- lấy từ DB đã lưu
            var nameTag = obj.GetComponentInChildren<NameTagManager>();
            if (nameTag != null && obj.HasInputAuthority)
            {
                nameTag.RPC_SetNickname(nickname);
            }

            //  Quản lý token đăng nhập
            string token = PlayerDataHolder1.Token;
            if (OnlineAccountManager.Instance.OnlineTokens
             .TryGetValue(token, out PlayerRef oldPlayer))
            {
                if (!oldPlayer.Equals(player))
                {
                    if (runner.TryGetPlayerObject(oldPlayer, out NetworkObject oldPlayerObj))
                    {
                        var oldAvatar = oldPlayerObj.GetComponent<PlayerAvatar>();
                        if (oldAvatar != null)
                        {
                            oldAvatar.RPC_KickToLogin();
                        }
                    }
                }
            }

            OnlineAccountManager.Instance.OnlineTokens[token] = player;
        }

    }
   






    public void OnPlayerLeft(NetworkRunner runner, PlayerRef player)
    {
        string tokenToRemove = null;

        foreach (var kvp in OnlineAccountManager.Instance.OnlineTokens)
        {
            if (kvp.Value == player)
            {
                tokenToRemove = kvp.Key;
                break;
            }
        }

        if (!string.IsNullOrEmpty(tokenToRemove))
        {
            OnlineAccountManager.Instance.OnlineTokens.Remove(tokenToRemove);
            Debug.Log(" Đã xóa token khi client rời game");
        }


    }

    // Các callback khác giữ nguyên
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
