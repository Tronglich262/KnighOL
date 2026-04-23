using System.Collections.Generic;
using UnityEngine;
using Fusion;
using UnityEngine.UI;

public class EnemyDamageHandler : NetworkBehaviour
{
    

    public int EnemyId;
    public int MaxHealth = 1000;

    private Animator animator;
    public Slider healthBarSlider;

    private readonly List<PlayerRef> attackers = new();

    // 🔥 MMO core
    private EnemyAggroSystem aggro;

    // =========================
    private EnemyStats stats;
    private EnemyCore core;
    private int lastHP = -1;

    public override void Spawned()
    {
        animator = GetComponent<Animator>();
        aggro = GetComponent<EnemyAggroSystem>();
        stats = GetComponent<EnemyStats>();
        core = GetComponent<EnemyCore>();

        if (HasStateAuthority)
        {
            stats.HP = stats.MaxHP;
            attackers.Clear();
            aggro.Clear();
        }

        UpdateHealthUI(stats.HP, stats.MaxHP);
    }



    // =====================================================
    // CLIENT → STATE AUTHORITY
    // =====================================================
    [Rpc(RpcSources.All, RpcTargets.StateAuthority)]
    public void RPC_TakeDamage(int amount, PlayerRef attacker, NetworkObject attackerObj)
    {
        if (!HasStateAuthority || stats.HP <= 0)
            return;

        stats.HP -= amount;

        if (!attackers.Contains(attacker))
            attackers.Add(attacker);

        if (attackerObj != null)
            aggro.AddThreat(attacker, amount, attackerObj.transform);

        if (stats.HP <= 0)
        {
            stats.HP = 0;
            Die();
        }
        else
        {
            RPC_PlayHitEffect();
        }
    }
    public void UpdateHealthUI(int hp, int maxHp)
    {
        if (healthBarSlider != null)
        {
            healthBarSlider.maxValue = maxHp;
            healthBarSlider.value = hp;
        }
    }


    // =====================================================
    private void Die()
    {
        Debug.Log($"[Enemy] Die | EnemyId={EnemyId}");

        foreach (var attacker in attackers)
        {
            RPC_GiveExp(attacker, 50, EnemyId);
        }

        RPC_PlayDeathAnim();

        // 🔥 QUAN TRỌNG: giao quyền cho Core
        core.Die();
    }


    
    // =====================================================
    [Rpc(RpcSources.StateAuthority, RpcTargets.All)]
    private void RPC_PlayHitEffect() { }

    [Rpc(RpcSources.StateAuthority, RpcTargets.All)]
    private void RPC_PlayDeathAnim()
    {
        if (animator != null)
            animator.SetTrigger("Die");
    }

    // =====================================================
    [Rpc(RpcSources.StateAuthority, RpcTargets.All)]
    private void RPC_GiveExp(PlayerRef who, int exp, int enemyId)
    {
        if (AuthManager.Instance == null)
            return;

        var levelManager = FindAnyObjectByType<PlayerLevelManager>();
        if (levelManager != null)
            levelManager.AddExp(exp);

        AuthManager.Instance.UpdateQuestProgress(
            "KillEnemy",
            enemyId,
            1
        );
    }
    public override void Render()
    {
        if (stats == null) return;

        // 🔥 CHỈ UPDATE KHI HP THAY ĐỔI
        if (stats.HP != lastHP)
        {
            lastHP = stats.HP;
            UpdateHealthUI(stats.HP, stats.MaxHP);

            // 🔥 update target panel nếu đang target
            var panel = TargetInfoPanel.Instance;
            if (panel != null)
                panel.NotifyHPChanged(stats);
        }
    }

    // =====================================================

}
