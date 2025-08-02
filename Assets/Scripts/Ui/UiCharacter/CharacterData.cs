using System.Diagnostics;

[System.Serializable]
public class CharacterData
{
    public string Helmet;  //mu
   // public string Armor;      //ao
    public string Vest;// ao trong
    public string Pauldrons;//quan
    public string Gloves;//bao tay
    public string Boots;//giay
    public string MeleeWeapon1H; // 1 kiem
    public string MeleeWeapon2H; // 1 kiem to
    public string Firearms1H;  // Súng một tay
    public string Firearms2H;  // Súng hai tay
    public string PrimaryMeleeWeapon; // Vũ khí chính
    public string SecondaryMeleeWeapon; // Vũ khí phụ (nếu có)
    public string Bow; // Cung
    public string Hair;//toc
    public string Belt;//nhan
    public string Cape;//ao choang
    public string Back; // Bao cung
    public string Mask;//bit mat
    public string Glasses; //kinh
    public string Shield;//khien
    public string Body;
    public string[] Armor;
    public string Head;
    public string Ears;

    // sung 1 tay or 2 tay 
    public string WeaponType;
    public string Firearms;
    public string FirearmParams;
    public void Equip(ItemStats stats)
    {
        if (string.IsNullOrEmpty(PlayerDataHolder1.Character.Head))
        {
            PlayerDataHolder1.Character.Head = "Head/Male/Head1"; // hoặc Head bạn có
        }

        switch (stats.Type)
        {

            case "Helmet":
                Helmet = stats.itemId;
                break;
            case "Armor":
                Armor = new string[] { stats.itemId };
                break;
            case "Vest":
                Vest = stats.itemId;
                break;
            case "Pauldrons":
                Pauldrons = stats.itemId;
                break;
            case "Gloves":
                Gloves = stats.itemId;
                break;
            case "Boots":
                Boots = stats.itemId;
                break;
            case "Shield":
                Shield = stats.itemId;
                break;
            case "Cape":
                Cape = stats.itemId;
                break;
            case "Mask":
                Mask = stats.itemId;
                break;
            case "Glasses":
                Glasses = stats.itemId;
                break;
            case "Belt":
                Belt = stats.itemId;
                break;
            case "Back":
                Back = stats.itemId;
                break;
            case "Hair":
                Hair = stats.itemId;
                break;

            // Vũ khí
            case "MeleeWeapon1H":
                MeleeWeapon1H = stats.itemId;
                PrimaryMeleeWeapon = stats.itemId;
                WeaponType = "Melee1H";
                break;
            case "MeleeWeapon2H":
                MeleeWeapon2H = stats.itemId;
                PrimaryMeleeWeapon = stats.itemId;
                WeaponType = "Melee2H";
                break;
            case "Firearms1H":
                Firearms1H = stats.itemId;
                Firearms = stats.itemId;
                WeaponType = "Firearms1H";
                break;
            case "Firearms2H":
                Firearms2H = stats.itemId;
                Firearms = stats.itemId;
                WeaponType = "Firearms2H";
                break;
            case "Bow":
                Bow = stats.itemId;
                WeaponType = "Bow";
                break;
            


            default:
                UnityEngine.Debug.LogWarning($" Không hỗ trợ trang bị: {stats.Type}");
                break;
        }
    }
    public string ToJson()
    {
        return UnityEngine.JsonUtility.ToJson(this);
    }


}
