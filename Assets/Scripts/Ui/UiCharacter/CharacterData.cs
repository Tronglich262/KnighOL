using UnityEngine;

[System.Serializable]
public class CharacterData
{
    public string Helmet;
    public string Vest;
    public string Pauldrons;
    public string Gloves;
    public string Boots;

    public string MeleeWeapon1H;
    public string MeleeWeapon2H;

    public string PrimaryMeleeWeapon;
    public string SecondaryMeleeWeapon;

    public string Bow;
    public string Hair;
    public string Belt;
    public string Cape;
    public string Back;
    public string Mask;
    public string Glasses;
    public string Shield;

    public string Body;
    public string[] Armor = new string[1];
    public string Head = "Head/Male/Head1";
    public string Ears;

    public string WeaponType;

    private const string DefaultHead = "Head/Male/Head1";

    public void Equip(ItemStats stats)
    {
        if (stats == null || string.IsNullOrEmpty(stats.Type) || string.IsNullOrEmpty(stats.itemId))
        {
            Debug.LogWarning($"Equip failed: stats invalid. Type={stats?.Type}, itemId={stats?.itemId}");
            return;
        }

        string itemId = ItemIdUtility.Normalize(stats.itemId);
        EnsureDefaultHead();

        switch (stats.Type)
        {
            case EquipKeys.Helmet: Helmet = itemId; break;
            case EquipKeys.Vest: Vest = itemId; break;
            case EquipKeys.Pauldrons: Pauldrons = itemId; break;
            case EquipKeys.Gloves: Gloves = itemId; break;
            case EquipKeys.Boots: Boots = itemId; break;
            case EquipKeys.Shield: Shield = itemId; break;
            case EquipKeys.Cape: Cape = itemId; break;
            case EquipKeys.Mask: Mask = itemId; break;
            case EquipKeys.Glasses: Glasses = itemId; break;
            case EquipKeys.Belt: Belt = itemId; break;
            case EquipKeys.Back: Back = itemId; break;
            case EquipKeys.Hair: Hair = itemId; break;
            case EquipKeys.Armor: SetArmor(itemId); break;

            case EquipKeys.MeleeWeapon1H:
            case EquipKeys.MeleeWeapon2H:
            case EquipKeys.Bow:
                ClearWeaponSlots();
                AssignWeapon(stats.Type, itemId);
                break;

            default:
                Debug.LogWarning($"Không hỗ trợ trang bị: {stats.Type}");
                break;
        }
    }

    private void SetArmor(string itemId)
    {
        if (Armor == null || Armor.Length != 1)
            Armor = new string[1];

        Armor[0] = itemId;
    }

    private void AssignWeapon(string type, string itemId)
    {
        switch (type)
        {
            case EquipKeys.MeleeWeapon1H:
                MeleeWeapon1H = itemId;
                PrimaryMeleeWeapon = itemId;
                WeaponType = EquipKeys.Weapon_Melee1H;
                break;

            case EquipKeys.MeleeWeapon2H:
                MeleeWeapon2H = itemId;
                PrimaryMeleeWeapon = itemId;
                WeaponType = EquipKeys.Weapon_Melee2H;
                break;
            case EquipKeys.Bow:
                Bow = itemId;
                WeaponType = EquipKeys.Weapon_Bow;
                break;
        }
    }

    private void ClearWeaponSlots()
    {
        MeleeWeapon1H = null;
        MeleeWeapon2H = null;
        Bow = null;

        PrimaryMeleeWeapon = null;
        SecondaryMeleeWeapon = null;
        WeaponType = null;
    }

    private void EnsureDefaultHead()
    {
        if (string.IsNullOrEmpty(Head))
            Head = DefaultHead;
    }

    public string ToJson()
    {
        return JsonUtility.ToJson(this);
    }
}