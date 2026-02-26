using Fusion;
using UnityEngine;
using System.Collections;

public class EnemySpawner : NetworkBehaviour
{
    public NetworkPrefabRef enemyPrefab;
    public EnemySpawnPoint[] spawnPoints;
    [Networked] private NetworkBool HasInitialized { get; set; }
    public override void Spawned()
    {
        if (!HasStateAuthority) return;

        if (HasInitialized) return;

        HasInitialized = true;

        foreach (var sp in spawnPoints)
        {
            Spawn(sp);
        }
    }
    void Spawn(EnemySpawnPoint sp)
    {
        var obj = Runner.Spawn(
            enemyPrefab,
            sp.spawnTransform.position,
            Quaternion.identity
        );

        sp.IsOccupied = true;

        obj.GetComponent<EnemyCore>().Init(sp, this);
    }

    public void RequestRespawn(EnemySpawnPoint sp)
    {
        if (!HasStateAuthority) return;
        StartCoroutine(RespawnRoutine(sp));
    }

    IEnumerator RespawnRoutine(EnemySpawnPoint sp)
    {
        yield return new WaitForSeconds(sp.respawnTime);
        Spawn(sp);
    }

}
