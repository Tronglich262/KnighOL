using UnityEngine;
using Fusion;

public class ArrowDamage : NetworkBehaviour
{
    [Header("Damage")]
    public int baseDamage = 50;
    public LayerMask enemyLayer;

    private PlayerRef owner;      // ai bắn mũi tên
    private bool hasHit = false;  // tránh hit nhiều lần

    // =========================
    // INIT TỪ BOW (SERVER)
    // =========================
    public void Init(PlayerRef ownerRef)
    {
        owner = ownerRef;
    }

    // =========================
    // COLLISION
    // =========================
    private void OnTriggerEnter(Collider other)
    {
        if (!Object.HasStateAuthority) return;
        if (hasHit) return;

        // chỉ va chạm enemy
        if (((1 << other.gameObject.layer) & enemyLayer) == 0)
            return;

        EnemyDamageHandler enemy = other.GetComponent<EnemyDamageHandler>();
        if (enemy == null) return;

        hasHit = true;

        // gây damage
        enemy.RPC_TakeDamage(
            baseDamage,
            owner,
            null // arrow không cần NetworkObject attacker
        );

        // huỷ mũi tên
        Runner.Despawn(Object);
    }
}
