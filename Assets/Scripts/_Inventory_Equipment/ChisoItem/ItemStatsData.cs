using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

[CreateAssetMenu(fileName = "ItemStats", menuName = "RPG/ItemStats", order = 1)]
public class ItemStats : ScriptableObject
{
    public int Item_ID;
    public string itemId;
    public int LevelRequired; 
    public Sprite Icon;
    public string Name;
    public string Type;
    public string Description;
    public string Rarity;
    public string Value;// tính trong mua bán 0 là k mua bán được . 1 là mua bán được
    public int Strength;  // sức mạnh
    public int Defense;   // phòng thủ
    public int Agility;   // nhanh nhẹn
    public int Intelligence; // tinh thần ( thông minh)
    public int Vitality;   // hp
    //chuyền dữ liệu cho ItemStartsDTO theo dạng json để truyền lên database

    public ItemStatsDTO ToDTO()
    {
        return new ItemStatsDTO
        {
            Item_ID = this.Item_ID,
            Name = this.Name,
            Type = this.Type,
            Description = this.Description,
            Rarity = this.Rarity,
            Value = this.Value,
            //CHỈ SỐ
            Strength = this.Strength,
            Defense = this.Defense,
            Agility = this.Agility,
            Intelligence = this.Intelligence,
            Vitality = this.Vitality,
            
            
        };
    }
}
