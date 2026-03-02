using Fusion;
using System.Collections;
using UnityEngine;

/// <summary>
/// Core component của enemy - quản lý spawn, chết, nhận damage
/// </summary>
public class EnemyCore : NetworkBehaviour
{
    public EnemyStats Stats;
    public EnemyAggroSystem Aggro;
    public EnemyAI AI;

    private EnemySpawner spawner;
    private EnemySpawnPoint spawnPoint;

    public GameObject damageTextPrefab;

    public override void Spawned()
    {
        Stats = GetComponent<EnemyStats>();
        Aggro = GetComponent<EnemyAggroSystem>();
        AI = GetComponent<EnemyAI>();
    }

    /// <summary>
    /// Khởi tạo enemy sau khi spawn
    /// </summary>
    public void Init(EnemySpawnPoint sp, EnemySpawner spawner)
    {
        this.spawnPoint = sp;
        this.spawner = spawner;

        Stats.Init(sp.monsterId);
        Aggro.Clear();
        AI.ResetState();
    }

    /// <summary>
    /// Xử lý khi enemy chết
    /// </summary>
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

    /// <summary>
    /// Nhận damage từ player - xử lý ở server
    /// </summary>
    public void ReceiveHit(int damage, PlayerRef attacker)
    {
        if (!HasStateAuthority) return;

        int finalDamage = Stats.TakeDamage(damage);

        RPC_ShowDamage(finalDamage);

        if (Stats.HP <= 0)
            Die();
    }

    /// <summary>
    /// Hiển thị damage text trên tất cả client
    /// </summary>
    [Rpc(RpcSources.StateAuthority, RpcTargets.All)]
    public void RPC_ShowDamage(int damage)
    {
        if (damageTextPrefab == null) return;

        Vector3 spawnPos = transform.position + Vector3.up * 2f;

        var obj = Instantiate(damageTextPrefab, spawnPos, Quaternion.identity);
        obj.GetComponent<DamageText>().Setup(damage);
    }

    /// <summary>
    /// Yêu cầu tính damage từ client
    /// </summary>
    [Rpc(RpcSources.All, RpcTargets.StateAuthority)]
    public void RPC_RequestHit(int damage, PlayerRef attacker)
    {
        if (!HasStateAuthority) return;

        int finalDamage = Stats.TakeDamage(damage);

        RPC_ShowDamage(finalDamage);

        if (Stats.HP <= 0)
            Die();
    }
}
