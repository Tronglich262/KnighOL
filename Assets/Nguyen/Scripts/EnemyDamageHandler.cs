using UnityEngine;
using Fusion;

public class EnemyDamageHandler : NetworkBehaviour
{
    [Networked]
    public int CurrentHealth { get; set; }

    public int MaxHealth = 100;
    private Animator animator;

    public override void Spawned()
    {
        animator = GetComponent<Animator>();

        if (HasStateAuthority)
        {
            CurrentHealth = MaxHealth;
        }
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
        Invoke(nameof(DisableEnemy), 1.0f);
    }

    private void DisableEnemy()
    {
        gameObject.SetActive(false);
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
}
