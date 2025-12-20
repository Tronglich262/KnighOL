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

    // Lưu các attacker đã gây dame
    private List<PlayerRef> attackers = new List<PlayerRef>();

    public override void Spawned()
    {
        animator = GetComponent<Animator>();
        if (healthBarSlider == null)
        {
            healthBarSlider = GetComponentInChildren<Slider>();
        }

        if (HasStateAuthority)
        {
            CurrentHealth = MaxHealth;
            attackers.Clear();
        }
        OnHealthChanged();
    }

    // Nhận dame từ client -> gửi về server authority
    [Rpc(RpcSources.All, RpcTargets.StateAuthority)]
    public void RPC_TakeDamage(int amount, PlayerRef attacker, RpcInfo info = default)
    {
        TakeDamage(amount, attacker);
    }

    public void TakeDamage(int amount, PlayerRef attacker)
    {
        if (!HasStateAuthority || CurrentHealth <= 0) return;

        CurrentHealth -= amount;

        // Lưu attacker nếu chưa có
        if (!attackers.Contains(attacker))
            attackers.Add(attacker);

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

    private void Die()
    {
        Debug.Log("[EnemyDamageHandler] Enemy died.");

        // Chia EXP cho tất cả attacker
        foreach (var attacker in attackers)
        {
            // Gửi RPC cộng EXP về đúng client
            RPC_GiveExp(attacker, 50, EnemyId); // 50 là EXP, bạn thay theo ý muốn
        }

        RPC_PlayDeathAnim();
        Invoke(nameof(DisableEnemy), 1.0f);
    }
    private void DisableEnemy()
    {
        if (Object != null && Object.IsValid && HasStateAuthority)
        {
            Runner.Despawn(Object); // Xoá enemy trên toàn bộ client
        }
    }


    [Rpc(RpcSources.StateAuthority, RpcTargets.All)]
    private void RPC_PlayHitEffect()
    {
        // animation khi bị đánh (nếu cần)
    }

    [Rpc(RpcSources.StateAuthority, RpcTargets.All)]
    private void RPC_PlayDeathAnim()
    {
        if (animator != null)
            animator.SetTrigger("Die");
    }

    // RPC gửi exp về đúng player
    [Rpc(RpcSources.StateAuthority, RpcTargets.All)]
    private void RPC_GiveExp(PlayerRef who, int exp, int enemyId)
    {
        if (Runner.LocalPlayer == who)
        {
            // Chỉ client đúng PlayerRef mới cộng EXP
            var levelManager = FindObjectOfType<PlayerLevelManager>();
            if (levelManager != null)
            {
                levelManager.AddExp(exp);
            }
            // Gọi cập nhật nhiệm vụ
            AuthManager.Instance?.UpdateQuestProgress("KillEnemy", enemyId, 1); // Sử dụng enemyId truyền vào
        }
    }


    private void OnHealthChanged()
    {
        if (healthBarSlider != null)
        {
            healthBarSlider.maxValue = MaxHealth;
            healthBarSlider.value = CurrentHealth;
        }
    }
}
