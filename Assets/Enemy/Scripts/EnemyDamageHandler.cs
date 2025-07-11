using UnityEngine;
using Fusion;
using UnityEngine.UI;

public class EnemyDamageHandler : NetworkBehaviour
{
    [Networked, OnChangedRender(nameof(OnHealthChanged))]
    public int CurrentHealth { get; set; }

    public int MaxHealth = 1000;
    private Animator animator;

    // Tham chiếu tới HealthBar (Slider) trên đầu quái
    public Slider healthBarSlider;

    public override void Spawned()
    {
        animator = GetComponent<Animator>();

        if (healthBarSlider == null)
        {
            // Tự tìm Slider trong con (nếu chưa gán trong Inspector)
            healthBarSlider = GetComponentInChildren<Slider>();
        }

        if (HasStateAuthority)
        {
            CurrentHealth = MaxHealth;
        }
        OnHealthChanged(); // Cập nhật ngay lúc spawn
    }

    public void TakeDamage(int amount)
    {
        if (!HasStateAuthority || CurrentHealth <= 0) return;

        CurrentHealth -= amount;

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
        RPC_PlayDeathAnim();
        Invoke(nameof(DisableEnemy), 1.0f); // Đợi animation xong rồi xoá

    }

    private void DisableEnemy()
    {
        if (Object != null && Object.IsValid && HasStateAuthority)
        {
            Runner.Despawn(Object); // Sẽ xóa enemy trên toàn bộ client
        }
    }

    [Rpc(RpcSources.StateAuthority, RpcTargets.All)]
    private void RPC_PlayHitEffect()
    {
        // Optional: chơi animation bị đánh
    }

    [Rpc(RpcSources.StateAuthority, RpcTargets.All)]
    private void RPC_PlayDeathAnim()
    {
        if (animator != null)
            animator.SetTrigger("Die");
    }

    // Nhận sát thương qua RPC
    [Rpc(RpcSources.All, RpcTargets.StateAuthority)]
    public void RPC_TakeDamage(int amount, PlayerRef attacker, RpcInfo info = default)
    {
        TakeDamage(amount, attacker);
    }

    public void TakeDamage(int amount, PlayerRef attacker)
    {
        if (!HasStateAuthority || CurrentHealth <= 0) return;

        CurrentHealth -= amount;

        // Lấy PlayerLevelManager theo PlayerRef
        var attackerObj = FindPlayerByRef(attacker); // Viết thêm hàm này!

        if (attackerObj != null)
        {
            var levelManager = attackerObj.GetComponent<PlayerLevelManager>();
            if (levelManager != null)
            {
                levelManager.AddExp(amount);
            }
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
    private GameObject FindPlayerByRef(PlayerRef playerRef)
    {
        foreach (var playerObj in FindObjectsOfType<NetworkObject>())
        {
            if (playerObj.HasInputAuthority && playerObj.InputAuthority == playerRef)
                return playerObj.gameObject;
        }
        return null;
    }

    // Hàm này sẽ tự động gọi mỗi khi CurrentHealth thay đổi (cho tất cả client)
    private void OnHealthChanged()
    {
        if (healthBarSlider != null)
        {
            healthBarSlider.maxValue = MaxHealth;
            healthBarSlider.value = CurrentHealth;
        }
    }
}
