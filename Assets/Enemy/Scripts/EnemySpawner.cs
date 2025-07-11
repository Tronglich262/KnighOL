using UnityEngine;
using Fusion;

public class EnemySpawner : NetworkBehaviour
{
    public NetworkPrefabRef enemyPrefab;
    public Transform[] spawnPoints; // Nhớ sửa tên biến cho đúng!

    public override void Spawned()
    {
        if (Runner.IsServer || Runner.IsSharedModeMasterClient)
        {
            Debug.Log("Đang spawn enemy từ EnemySpawner!");

            // Duyệt qua tất cả điểm spawn, mỗi điểm spawn 1 enemy
            foreach (var point in spawnPoints)
            {
                Runner.Spawn(enemyPrefab, point.position, Quaternion.identity);
            }
        }
    }
}
