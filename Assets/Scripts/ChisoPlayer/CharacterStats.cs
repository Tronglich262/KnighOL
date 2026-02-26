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

    public void InitFromPlayerStats(PlayerStats stats)
    {
        hp = stats.hp;
        strength = stats.strength;
        defense = stats.defense;
        agility = stats.agility;
        speed = stats.speed;
        spirit = stats.spirit;
        Intelligence = stats.intelligence;
    }

    public void RecalculateStatsFromEquipment(List<ItemStats> equippedItems)
    {
        finalStrength = strength;
        finalDefense = defense;
        finalAgility = agility;
        finalVitality = hp;
        finalIntelligence = Intelligence;
        finalIntelligence = 0;

        foreach (var item in equippedItems)
        {
            finalStrength += item.Strength;
            finalDefense += item.Defense;
            finalAgility += item.Agility;
            finalVitality += item.Vitality;
            finalIntelligence += item.Intelligence;
        }
    }
}
