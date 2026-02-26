/*public static class DamageCalculator
{
    public static int CalculatePhysicalDamage(
        CharacterStats attacker,
        EnemyStats defender,
        float skillMultiplier)
    {
        float attack = attacker.finalStrength;

        float raw = attack * skillMultiplier;

        float damage = (raw * raw) / (raw + defender.Defense + 1);

        float critChance = attacker.finalAgility * 0.002f;

        if (Random.value < critChance)
        {
            damage *= 1.5f;
        }

        return Mathf.Max(1, Mathf.RoundToInt(damage));
    }
}*/