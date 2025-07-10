using Assets.HeroEditor.FantasyInventory.Scripts.Interface.Elements;
using System.Collections.Generic;
using System.Windows.Forms;
using UnityEngine;

public class CharacterStats : MonoBehaviour
{
    //tại chỉ số của Player 
    [Header("Base Stats")]
    public int strength = 1000;
    public int defense = 10;
    public int agility = 10;
    public int vitality = 100;
    //chỉ số tổng của các trang bị 
    [Header("Final Stats (có thể bị cộng thêm từ đồ)")]
    public int finalStrength;
    public int finalDefense;
    public int finalAgility;
    public int finalIntelligence;
    public int finalVitality;
    // hàm tính tổng của trang bị hiển thị ra bảng
    public void RecalculateStatsFromEquipment(List<ItemStats> equippedItems)
    {
        finalStrength = 0;
        finalDefense = 0;
        finalAgility = 0;
        finalIntelligence = 0;
        finalVitality = 0;

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
