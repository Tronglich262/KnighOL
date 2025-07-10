using System.Collections;
using TMPro;
using Assets.HeroEditor.FantasyInventory.Scripts.Interface.Elements;
using UnityEngine;

public class ThongTin : MonoBehaviour
{
    public TextMeshProUGUI Nametext;
    public TextMeshProUGUI strength;
    public TextMeshProUGUI defense;
    public TextMeshProUGUI agility;
    public TextMeshProUGUI vitality;
    //chi so item
    public TextMeshProUGUI strengthitem;
    public TextMeshProUGUI defenseitem;
    public TextMeshProUGUI agilityitem;
    public TextMeshProUGUI vitalityitem;
    public static ThongTin instance;
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

        while (player == null)
        {
            player = GameObject.FindWithTag("Player");
            yield return null;
        }
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
        if (stats != null)
        {
            Nametext.text = "Tên: " + PlayerDataHolder1.PlayerName;
            strength.text = "Sức mạnh: " + stats.strength;
            defense.text = "Phòng thủ: " + stats.defense;
            agility.text = "Nhanh nhẹn: " + stats.agility;
            vitality.text = "Sinh lực: " + stats.vitality;

            strengthitem.text = "Sức mạnh trang bị: " + stats.finalStrength;
            defenseitem.text = "Phòng thủ trang bị: " + stats.finalDefense;
            agilityitem.text = "Nhanh nhẹn trang bị: " + stats.finalAgility;
            vitalityitem.text = "Sinh lực trang bị: " + stats.finalVitality;
        }
        else
        {
            Debug.LogWarning("Player không có CharacterStats.");
        }
    }
}
