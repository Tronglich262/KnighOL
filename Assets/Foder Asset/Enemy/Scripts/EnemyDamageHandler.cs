using System.Collections.Generic;
using UnityEngine;
using Fusion;
using UnityEngine.UI;

public class EnemyDamageHandler : NetworkBehaviour
{
    [Networked, OnChangedRender(nameof(OnHealthChanged))]
    public int CurrentHealth { get; set; }

    public int EnemyId;
    public int MaxHealth = 1000;

    private Animator animator;
    public Slider healthBarSlider;

    private readonly List<PlayerRef> attackers = new();

    // =========================
    public override void Spawned()
    {
        animator = GetComponent<Animator>();

        if (healthBarSlider == null)
            healthBarSlider = GetComponentInChildren<Slider>();

        if (HasStateAuthority)
        {
            CurrentHealth = MaxHealth;
            attackers.Clear();
        }

        OnHealthChanged();
    }

    // =====================================================
    // CLIENT -> SERVER DAMAGE
    // =====================================================
    [Rpc(RpcSources.All, RpcTargets.StateAuthority)]
    public void RPC_TakeDamage(int amount, PlayerRef attacker, NetworkObject attackerObj)
    {
        TakeDamage(amount, attacker, attackerObj);
    }

    // =====================================================
    // SERVER HANDLE DAMAGE
    // =====================================================
    private void TakeDamage(int amount, PlayerRef attacker, NetworkObject attackerObj)
    {
        if (!HasStateAuthority || CurrentHealth <= 0)
            return;

        CurrentHealth -= amount;

        if (!attackers.Contains(attacker))
            attackers.Add(attacker);

        // Force aggro đúng người đánh
        if (attackerObj != null)
        {
            EnemyAI ai = GetComponent<EnemyAI>();
            if (ai != null)
                ai.ForceAggro(attackerObj.transform);
        }

        if (CurrentHealth <= 0)
        {
            CurrentHealth = 0;
            Die();
        }
        else
        {
            RPC_PlayHitEffect();
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
        Enemy deadEnemy = GetComponent<Enemy>();

        /*foreach (var player in GameObject.FindGameObjectsWithTag("Player"))
        {
            var targetSys = player.GetComponent<TargetingSystem>();
            if (targetSys != null && targetSys.CurrentTarget == GetComponent<Enemy>())
            {
                targetSys.ClearTarget();
            }
        }
*/


        RPC_PlayDeathAnim();
        Invoke(nameof(DisableEnemy), 1f);
    }


    private void DisableEnemy()
    {
        if (HasStateAuthority && Object != null && Object.IsValid)
            Runner.Despawn(Object);
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
    // ⚠️ FIX QUAN TRỌNG: KHÔNG DÙNG Object.HasInputAuthority
    [Rpc(RpcSources.StateAuthority, RpcTargets.All)]
    private void RPC_GiveExp(PlayerRef who, int exp, int enemyId)
    {
        // ✅ GIỐNG CODE CŨ – KHÔNG CHECK Runner.LocalPlayer
        if (AuthManager.Instance == null)
            return;

        var levelManager = FindObjectOfType<PlayerLevelManager>();
        if (levelManager != null)
            levelManager.AddExp(exp);

        AuthManager.Instance.UpdateQuestProgress(
            "KillEnemy",
            enemyId,
            1
        );

        Debug.Log($"[QUEST OK] enemyId={enemyId}");
    }



    // =====================================================
    private void OnHealthChanged()
    {
        if (healthBarSlider != null)
        {
            healthBarSlider.maxValue = MaxHealth;
            healthBarSlider.value = CurrentHealth;
        }
    }
}  