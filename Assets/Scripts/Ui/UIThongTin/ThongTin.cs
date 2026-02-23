using System.Collections;
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

    private void Start()
    {
        StartCoroutine(WaitForPlayerStats());
    }

    IEnumerator WaitForPlayerStats()
    {
        GameObject player = null;

        // 1️⃣ Chờ player spawn
        while (player == null)
        {
            player = GameObject.FindWithTag("Player");
            yield return null;
        }

        // 2️⃣ Lấy base stats từ server
        yield return StartCoroutine(AuthManager.Instance.GetPlayerStats(result =>
        {
            stats1 = result;
        }));

        // 3️⃣ Init base stats
        var charStats = player.GetComponent<CharacterStats>();
        if (charStats != null)
        {
            charStats.InitFromPlayerStats(stats1);
        }

        // 4️⃣ Load trang bị từ CharacterJson
        var equipMgr = player.GetComponent<EquipmentStatManager>();
        if (equipMgr != null)
        {
            equipMgr.LoadFromCharacterJson(PlayerDataHolder1.CharacterJson);
        }
        else
        {
            Debug.LogError("❌ Player thiếu EquipmentStatManager");
        }

        // 5️⃣ Update UI (FINAL stats)
        UpdateStatsUI();
    }


    public void UpdateStatsUI()
    {
        GameObject player = GameObject.FindWithTag("Player");
        if (player == null) return;

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

        maxHP = stats.finalVitality;
        currentHP = Mathf.Clamp(currentHP, 0, maxHP);
        healthBar.SetHealth(currentHP, maxHP);
    }

    public void UpdateCharacterStatsFromServer(PlayerStats serverStats)
    {
        GameObject player = GameObject.FindWithTag("Player");
        if (player != null)
        {
            var charStats = player.GetComponent<CharacterStats>();
            if (charStats != null)
            {
                charStats.InitFromPlayerStats(serverStats);
                // Nếu có hệ thống trang bị:
                // charStats.RecalculateStatsFromEquipment(currentEquipList);
            }
        }
    }


}
