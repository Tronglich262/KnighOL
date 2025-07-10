using UnityEngine;
using Fusion;

public class EnemySpawner : NetworkBehaviour
{
    public NetworkPrefabRef enemyPrefab;
    public Transform spawnPoint;

    public override void Spawned()
    {
        Debug.Log($"Spawner Spawned - IsServer: {Runner.IsServer}, IsSharedMasterClient: {Runner.IsSharedModeMasterClient}");

        // Cho phép cả Host hoặc Master Client trong chế độ Shared được spawn
        if (Runner.IsServer || Runner.IsSharedModeMasterClient)
        {
            Debug.Log("Đang spawn enemy từ EnemySpawner!");
            Runner.Spawn(enemyPrefab, spawnPoint.position, Quaternion.identity);
        }
        else
        {
            Debug.Log("Không có quyền spawn (KHÔNG phải Server hoặc MasterClient).");
        }
    }
}
