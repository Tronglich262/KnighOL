using System.Collections.Generic;
using UnityEngine;

public class CharacterStats : MonoBehaviour
{
    // Chỉ số gốc từ server/database (khởi tạo khi login/load nhân vật)
    public int hp;
    public int strength;
    public int defense;
    public int agility;
    public int speed;
    public int spirit;

    [Header("Final Stats (có thể bị cộng thêm từ đồ)")]
    public int finalStrength;
    public int finalDefense;
    public int finalAgility;
    public int finalIntelligence;
    public int finalVitality;

    // Gán các chỉ số từ dữ liệu PlayerStats
    public void InitFromPlayerStats(PlayerStats stats)
    {
        if (stats == null)
        {
            Debug.LogError("PlayerStats NULL khi InitFromPlayerStats!");
            return;
        }

        hp = stats.hp;
        strength = stats.strength;
        defense = stats.defense;
        agility = stats.agility;
        speed = stats.speed;
        spirit = stats.spirit;
    }

    // Tính lại chỉ số cộng thêm từ trang bị
    public void RecalculateStatsFromEquipment(List<ItemStats> equippedItems)
    {
        finalStrength = strength;
        finalDefense = defense;
        finalAgility = agility;
        finalIntelligence = 0;
        finalVitality = hp;

        foreach (var item in equippedItems)
        {
            finalStrength += item.Strength;
            finalDefense += item.Defense;
            finalAgility += item.Agility;
            finalIntelligence += item.Intelligence;
            finalVitality += item.Vitality;
        }
    }
}
