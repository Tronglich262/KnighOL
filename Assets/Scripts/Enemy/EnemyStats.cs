using Fusion;
using UnityEngine;

public class EnemyStats : NetworkBehaviour
{
    [Networked] public int MaxHP { get; set; }
    [Networked] public int HP { get; set; }
    [Networked] public int Attack { get; set; }

    // % giảm sát thương (0 -> 1)
    [Networked] public float DamageReduction { get; set; }

    public void Init(int monsterId)
    {
        MaxHP = 500;
        HP = MaxHP;
        Attack = 50;

        // Ví dụ quái cùi giảm 10%
        DamageReduction = 0.1f;
    }

    public int TakeDamage(int damage)
    {
        if (!Object.HasStateAuthority) return 0;

        float reducedDamage = damage * (1f - DamageReduction);

        int finalDamage = Mathf.RoundToInt(reducedDamage);

        HP -= finalDamage;

        if (HP < 0)
            HP = 0;

        return finalDamage;
    }
}