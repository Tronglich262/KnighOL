using Fusion;
using System.Collections;
using UnityEngine;

public class EnemyCore : NetworkBehaviour
{
    public EnemyStats Stats;
    public EnemyAggroSystem Aggro;
    public EnemyAI AI;

    private EnemySpawner spawner;
    private EnemySpawnPoint spawnPoint;

    public override void Spawned()
    {
        Stats = GetComponent<EnemyStats>();
        Aggro = GetComponent<EnemyAggroSystem>();
        AI = GetComponent<EnemyAI>();
    }

    public void Init(EnemySpawnPoint sp, EnemySpawner spawner)
    {
        this.spawnPoint = sp;
        this.spawner = spawner;

        Stats.Init(sp.monsterId);
        Aggro.Clear();
        AI.ResetState();
    }

    public void Die()
    {
        spawnPoint.IsOccupied = false;
        spawner.RequestRespawn(spawnPoint);
        StartCoroutine(DespawnDelay());
    }


    IEnumerator DespawnDelay()
    {
        yield return new WaitForSeconds(1.5f);
        if (Object != null && Object.IsValid)
            Runner.Despawn(Object);
    }

}
