using UnityEngine;
using Fusion;

public class ArrowDamage : NetworkBehaviour
{
    public LayerMask enemyLayer;

    private PlayerRef owner;
    private NetworkObject attackerObject;
    private bool hasHit = false;

    public void Init(PlayerRef ownerRef, NetworkObject attacker)
    {
        owner = ownerRef;
        attackerObject = attacker;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!Object.HasStateAuthority) return;
        if (hasHit) return;

        if (((1 << other.gameObject.layer) & enemyLayer) == 0)
            return;

        var enemy = other.GetComponentInParent<EnemyCore>();
        if (enemy == null) return;


        hasHit = true;

        // 🎯 TÍNH DAMAGE GIỐNG MELEE
        int damage = CalculateBowDamage();

        enemy.RPC_RequestHit(damage, owner);
        Runner.Despawn(Object);
    }

    int CalculateBowDamage()
    {
        if (attackerObject == null) return 0;

        var stats = attackerObject.GetComponent<CharacterStats>();
        if (stats == null) return 0;

        // 🎯 Bow scale theo AGI nhiều hơn
        int agi = stats.agility + stats.finalAgility;
        int str = stats.strength + stats.finalStrength;

        int baseRandom = Random.Range(70, 110);

        // Bow formula
        int damage = Mathf.RoundToInt(
            agi * 1.4f +       // chính
            str * 0.3f +       // phụ
            baseRandom
        );

        return damage;
    }
}