using System.Collections.Generic;
using UnityEngine;
public class CharacterStats : MonoBehaviour
{
    // BASE (server)
    public int hp;
    public int strength;
    public int defense;
    public int agility;
    public int speed;
    public int spirit;
    public int Intelligence;

    // FINAL
    public int finalStrength;
    public int finalDefense;
    public int finalAgility;
    public int finalVitality;
    public int finalIntelligence;
    public int finalSpirit;

    // MANA (max = 80 + finalSpirit * 3, dùng chiêu trừ theo %)
    public int maxMana = 100;
    public int currentMana = 100;

    public void InitFromPlayerStats(PlayerStats stats)
    {
        hp = stats.hp;
        strength = stats.strength;
        defense = stats.defense;
        agility = stats.agility;
        speed = stats.speed;
        spirit = stats.spirit;
        Intelligence = stats.intelligence;
        finalSpirit = spirit;
        RecalculateMana();
        currentMana = maxMana;
    }

    public void RecalculateStatsFromEquipment(List<ItemStats> equippedItems)
    {
        finalStrength = strength;
        finalDefense = defense;
        finalAgility = agility;
        finalVitality = hp;
        finalIntelligence = Intelligence;
        finalIntelligence = 0;
        finalSpirit = spirit;

        foreach (var item in equippedItems)
        {
            finalStrength += item.Strength;
            finalDefense += item.Defense;
            finalAgility += item.Agility;
            finalVitality += item.Vitality;
            finalIntelligence += item.Intelligence;
        }

        RecalculateMana();
    }

    void RecalculateMana()
    {
        maxMana = Mathf.Max(50, 80 + finalSpirit * 3);
        currentMana = Mathf.Clamp(currentMana, 0, maxMana);
    }

    /// <summary>Trả về true nếu đủ mana để trả cost (percent 0f–1f).</summary>
    public bool HasEnoughMana(float percentOfMax)
    {
        int cost = Mathf.Max(1, Mathf.RoundToInt(maxMana * percentOfMax));
        return currentMana >= cost;
    }

    /// <summary>Trừ mana theo % max. Gọi trên client trước khi gửi RPC.</summary>
    public void ConsumeMana(float percentOfMax)
    {
        int cost = Mathf.Max(1, Mathf.RoundToInt(maxMana * percentOfMax));
        currentMana = Mathf.Max(0, currentMana - cost);
    }
}
