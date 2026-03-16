using UnityEngine;
using Fusion;

public class PlayerAttackHit : NetworkBehaviour
{
    public int damage = 100;
    public float hitRadius = 1.2f;
    public LayerMask enemyLayer;
    public Transform hitPoint; // tay / weapon

    // =========================
    // GỌI TỪ ANIMATION EVENT
    // =========================
    public void Hit()
    {
        // ❗ CHỈ STATE AUTHORITY TÍNH DAME
        if (!HasStateAuthority) return;

        Vector2 center = hitPoint != null
            ? (Vector2)hitPoint.position
            : (Vector2)transform.position;

        Collider2D[] hits = Physics2D.OverlapCircleAll(
            center,
            hitRadius,
            enemyLayer
        );

        foreach (var hit in hits)
        {
            EnemyDamageHandler enemy = hit.GetComponent<EnemyDamageHandler>();
            if (enemy == null) continue;

            enemy.RPC_TakeDamage(
                damage,
                Object.InputAuthority, 
                Object
            );

            break; 
        }
    }

#if UNITY_EDITOR
    private void OnDrawGizmosSelected()
    {
        if (hitPoint == null) return;
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(hitPoint.position, hitRadius);
    }
#endif
}
