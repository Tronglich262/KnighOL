using TMPro;
using UnityEngine;

public class ThongTin : MonoBehaviour
{
    public TextMeshProUGUI Nametext;
    public TextMeshProUGUI strength;
    public TextMeshProUGUI defense;
    public TextMeshProUGUI agility;
    public TextMeshProUGUI vitality;
    public TextMeshProUGUI Speed;
    public TextMeshProUGUI Spirit;
    public TextMeshProUGUI Intelligence;
    //chi so item
    public TextMeshProUGUI strengthitem;
    public TextMeshProUGUI defenseitem;
    public TextMeshProUGUI agilityitem;
    public TextMeshProUGUI vitalityitem;
    public TextMeshProUGUI Intelligenceitem;

    public static ThongTin instance;
    public HealthBar healthBar;
    public int maxHP = 0;
    public int currentHP = 0;

    public PlayerStats stats1;   // ĐÂY sẽ được gán từ server

    public int maxMana = 100;
    public int currentMana = 100;

    public void Awake()
    {
        instance = this;
    }

    private void OnEnable()
    {
        UpdateStatsUI();
    }

    public void UpdateStatsUI()
    {
        if (PlayerSpawner.LocalPlayerObject == null) return;
        GameObject player = PlayerSpawner.LocalPlayerObject.gameObject;

        var stats = player.GetComponent<CharacterStats>();
        if (stats == null) return;

        Nametext.text = "Tên: " + PlayerDataHolder1.PlayerName;

        strength.text = "Sức mạnh: " + stats.finalStrength;
        defense.text = "Phòng thủ: " + stats.finalDefense;
        agility.text = "Nhanh nhẹn: " + stats.finalAgility;
        vitality.text = "Sinh lực: " + stats.finalVitality;
        Intelligence.text = "Trí Tuệ: " + stats.finalIntelligence;

        Speed.text = "Tốc độ: " + stats.speed;
        Spirit.text = "Tinh thần: " + stats.spirit;

        // Nếu muốn hiển thị bonus riêng
        strengthitem.text = "Sức mạnh trang bị: " + (stats.finalStrength - stats.strength);
        defenseitem.text = "Phòng thủ trang bị: " + (stats.finalDefense - stats.defense);
        agilityitem.text = "Nhanh nhẹn trang bị: " + (stats.finalAgility - stats.agility);
        vitalityitem.text = "Sinh lực trang bị: " + (stats.finalVitality - stats.hp);
        Intelligenceitem.text = "trí tuệ trang bị: " + (stats.finalIntelligence - stats.Intelligence);

        maxHP = Mathf.Max(1, stats.finalVitality);
        currentHP = maxHP;
        if (healthBar != null)
            healthBar.SetHealth(currentHP, maxHP);
    }

    public void UpdateCharacterStatsFromServer(PlayerStats serverStats)
    {
        if (PlayerSpawner.LocalPlayerObject != null)
        {
            GameObject player = PlayerSpawner.LocalPlayerObject.gameObject;
            var charStats = player.GetComponent<CharacterStats>();
            if (charStats != null)
            {
                charStats.InitFromPlayerStats(serverStats);
            }
        }
    }


}
