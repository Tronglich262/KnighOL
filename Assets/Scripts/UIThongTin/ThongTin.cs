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
    //chi so item
    public TextMeshProUGUI strengthitem;
    public TextMeshProUGUI defenseitem;
    public TextMeshProUGUI agilityitem;
    public TextMeshProUGUI vitalityitem;

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

        // Chờ player xuất hiện trên scene
        while (player == null)
        {
            player = GameObject.FindWithTag("Player");
            yield return null;
        }

        // Lấy dữ liệu PlayerStats từ server, rồi mới update UI
        yield return StartCoroutine(AuthManager.Instance.GetPlayerStats(result =>
        {
            stats1 = result; // Gán vào biến stats1
        }));

        UpdateStatsUI();
    }

    public void UpdateStatsUI()
    {
        GameObject player = GameObject.FindWithTag("Player");
        if (player == null)
        {
            Debug.LogWarning("Không tìm thấy Player để cập nhật UI.");
            return;
        }

        var stats = player.GetComponent<CharacterStats>();
        if (stats1 != null)
        {
            Nametext.text = "Tên: " + PlayerDataHolder1.PlayerName;
            vitality.text = "Sinh lực: " + stats1.hp;
            strength.text = "Sức mạnh: " + stats1.strength;
            defense.text = "Phòng thủ: " + stats1.defense;  // nếu có trường này trong PlayerStats
            agility.text = "Nhanh nhẹn: " + stats1.agility;
            Speed.text = "Tốc độ: " + stats1.speed;
            Spirit.text = "Tinh thần: " + stats1.spirit;
        }


        // Hiển thị chỉ số trang bị (từ script CharacterStats gắn trên Player)
        if (stats != null)
        {
            strengthitem.text = "Sức mạnh trang bị: " + stats.finalStrength;
            defenseitem.text = "Phòng thủ trang bị: " + stats.finalDefense;
            agilityitem.text = "Nhanh nhẹn trang bị: " + stats.finalAgility;
            vitalityitem.text = "Sinh lực trang bị: " + stats.finalVitality;

            if (healthBar != null)
            {

                maxHP = stats1.hp + stats.finalVitality;
                currentHP = maxHP; // Set máu đầy khi vừa cập nhật
                healthBar.SetHealth(currentHP, maxHP);
            }
        }
        else
        {
            Debug.LogWarning("Player không có CharacterStats.");
        }
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
