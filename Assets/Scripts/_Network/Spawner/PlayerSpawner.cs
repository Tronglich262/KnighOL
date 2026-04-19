using Assets.HeroEditor.Common.CharacterScripts;
using Fusion;
using Fusion.Sockets;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class PlayerSpawner : SimulationBehaviour, INetworkRunnerCallbacks
{
    [Header("Prefabs")]
    public NetworkObject playerPrefab;
    public GameObject characterCanvasPrefab;

    [Header("Spawn Points")]
    public Transform[] spawnPoints;

    [Header("Optional References")]
    [SerializeField] private PlayerCloneController cloneController;

    public static NetworkObject LocalPlayerObject;

    private GameObject localCanvasInstance;

    public void OnPlayerJoined(NetworkRunner runner, PlayerRef player)
    {
        var sync = FindFirstObjectByType<NicknameSyncManager>();
        if (sync != null)
            sync.OnPlayerJoined(runner, player);

        if (player != runner.LocalPlayer)
            return;

        EnsureLocalCanvas();

        Vector3 spawnPosition = ResolveSpawnPosition(runner);
        Quaternion spawnRotation = Quaternion.identity;

        NetworkObject obj = runner.Spawn(playerPrefab, spawnPosition, spawnRotation, player);
        LocalPlayerObject = obj;

        SetupClonePreview(obj);
        SetupAvatar(obj);
        RegisterOnlineToken(runner, player);
    }

    private void EnsureLocalCanvas()
    {
        if (localCanvasInstance != null)
            return;

        localCanvasInstance = Instantiate(characterCanvasPrefab);
        localCanvasInstance.SetActive(true);

        if (localCanvasInstance.GetComponent<LocalPlayerStatsLoader>() == null)
            localCanvasInstance.AddComponent<LocalPlayerStatsLoader>();

        if (InventoryManager.Instance != null)
        {
            var inventoryUi = localCanvasInstance.GetComponentInChildren<InventoryUIManager>(true);
            InventoryManager.Instance.uiManager = inventoryUi;
        }
    }

    private Vector3 ResolveSpawnPosition(NetworkRunner runner)
    {
        int playerIndex = runner.ActivePlayers.Count() - 1;

        if (spawnPoints != null && spawnPoints.Length > 0)
        {
            int spawnIndex = playerIndex % spawnPoints.Length;
            return spawnPoints[spawnIndex].position;
        }

        float offsetX = (playerIndex % 4) * 3f;
        float offsetY = (playerIndex / 4) * 3f;
        return new Vector3(offsetX, -7.02f + offsetY, 0f);
    }

    private void SetupClonePreview(NetworkObject playerObject)
    {
        PlayerCloneController ctrl = cloneController;

        if (ctrl == null)
            ctrl = FindFirstObjectByType<PlayerCloneController>(FindObjectsInactive.Include);

        if (ctrl == null)
        {
            Debug.LogWarning("[PlayerSpawner] Không tìm thấy PlayerCloneController trong scene.");
            return;
        }

        ctrl.SetTarget(playerObject);

        var cloneCharacter = ctrl.GetComponent<Character>();
        if (cloneCharacter == null)
        {
            Debug.LogWarning("[PlayerSpawner] Clone preview không có component Character.");
            return;
        }

        string json = PlayerDataHolder1.CharacterJson;

        if (ItemDetailsUI.Instance != null)
        {
            ItemDetailsUI.Instance.playerClone = ctrl.gameObject;
            ItemDetailsUI.Instance.character = cloneCharacter;
        }

        if (CharacterUIManager1.Instance != null)
        {
            CharacterUIManager1.Instance.character = cloneCharacter;
        }

        cloneCharacter.FromJson(json);
        ctrl.LoadJson(json);
    }

    private void SetupAvatar(NetworkObject playerObject)
    {
        var avatar = playerObject.GetComponent<PlayerAvatar>();
        if (avatar == null)
        {
            Debug.LogWarning("[PlayerSpawner] Player object không có PlayerAvatar.");
            return;
        }

        string json = PlayerDataHolder1.CharacterJson;
        string nickname = PlayerDataHolder1.PlayerName;

        avatar.UpdateCharacterJson(json);
        avatar.RPC_SendDisplayNameToServer(nickname);

        var nameTag = playerObject.GetComponentInChildren<NameTagManager>(true);
        if (nameTag != null && playerObject.HasInputAuthority)
        {
            nameTag.RPC_SetNickname(nickname);
        }
    }

    private void RegisterOnlineToken(NetworkRunner runner, PlayerRef currentPlayer)
    {
        if (OnlineAccountManager.Instance == null)
            return;

        string token = PlayerDataHolder1.Token;
        if (string.IsNullOrEmpty(token))
            return;

        if (OnlineAccountManager.Instance.OnlineTokens.TryGetValue(token, out PlayerRef oldPlayer))
        {
            if (!oldPlayer.Equals(currentPlayer) &&
                runner.TryGetPlayerObject(oldPlayer, out NetworkObject oldPlayerObj))
            {
                oldPlayerObj.GetComponent<PlayerAvatar>()?.RPC_KickToLogin();
            }
        }

        OnlineAccountManager.Instance.OnlineTokens[token] = currentPlayer;
    }

    public void OnPlayerLeft(NetworkRunner runner, PlayerRef player)
    {
        if (OnlineAccountManager.Instance == null)
            return;

        string keyToRemove = null;

        foreach (var kvp in OnlineAccountManager.Instance.OnlineTokens)
        {
            if (kvp.Value == player)
            {
                keyToRemove = kvp.Key;
                break;
            }
        }

        if (!string.IsNullOrEmpty(keyToRemove))
        {
            OnlineAccountManager.Instance.OnlineTokens.Remove(keyToRemove);
            Debug.Log("[PlayerSpawner] Đã xóa token khi client rời game.");
        }
    }

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