/*using UnityEngine;
using UnityEngine.UI;

public class CharacterEquipmentUI : MonoBehaviour
{
    public Image helmetSlot;
    public Image shieldSlot;
    public Image weaponSlot;
    public Image capeSlot;
    public Image backSlot;
    public Sprite defaultSprite;

    public void ShowCharacter(CharacterEquipmentData data)
    {
        helmetSlot.sprite = LoadSprite(data.Helmet);
        shieldSlot.sprite = LoadSprite(data.Shield);
        weaponSlot.sprite = LoadSprite(data.PrimaryMeleeWeapon);
        capeSlot.sprite = LoadSprite(data.Cape);
        backSlot.sprite = LoadSprite(data.Back);
    }

    private Sprite LoadSprite(string path)
    {
        if (string.IsNullOrEmpty(path)) return defaultSprite;

        var cleanPath = path.Split('#')[0].Split('[')[0].Trim();
        cleanPath = cleanPath.Replace('.', '/');

        Sprite sprite = Resources.Load<Sprite>(cleanPath);
        return sprite != null ? sprite : defaultSprite;
    }
}
*/