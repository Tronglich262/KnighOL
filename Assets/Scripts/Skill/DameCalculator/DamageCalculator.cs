using UnityEngine;

public static class DamageCalculator
{
    public static int CalculatePhysicalDamage(
        CharacterStats attacker,
        EnemyStats defender,
        float skillMultiplier)
    {
        if (attacker == null || defender == null)
            return 1;

        // Công thức fantasy kiểu MMORPG
        float attack = attacker.finalStrength;

        // Scale theo skill
        float rawDamage = attack * skillMultiplier;

        // Công thức mềm mượt, không âm
        float damage = (rawDamage * rawDamage) /
                       (rawDamage + defender.Defense + 1);

        // Crit theo Agility
        float critChance = attacker.finalAgility * 0.002f; // 100 agi = 20%
        if (Random.value < critChance)
        {
            damage *= 1.5f;
        }

        return Mathf.Max(1, Mathf.RoundToInt(damage));
    }

    public static int CalculateMagicDamage(
        CharacterStats attacker,
        EnemyStats defender,
        float skillMultiplier)
    {
        float magic = attacker.finalIntelligence;

        float rawDamage = magic * skillMultiplier;

        float damage = (rawDamage * rawDamage) /
                       (rawDamage + defender.Defense * 0.5f + 1);

        return Mathf.Max(1, Mathf.RoundToInt(damage));
    }
}