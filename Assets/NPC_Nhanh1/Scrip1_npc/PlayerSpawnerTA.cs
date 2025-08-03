using UnityEngine;
using Fusion;

public class PlayerSpawnerTA : SimulationBehaviour, IPlayerJoined
{
    [Header("Player Prefab (đã đăng ký trong Fusion NetworkPrefabConfig)")]
    public GameObject PlayerPrefab;

    public void PlayerJoined(PlayerRef player)
    {
        if (player == Runner.LocalPlayer)
        {
            Runner.Spawn(PlayerPrefab, new Vector3(14.2f, -7.1f, 0), Quaternion.identity, player);
        }
    }
}
