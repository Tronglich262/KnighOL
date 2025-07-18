using System.Data;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PotentialStatsPanel : MonoBehaviour
{
    [Header("UI Ref")]
    public TMP_Text hpText, strengthText, speedText, agilityText, spiritText, defensetext, potentialText;
    public Button addHpBtn, addStrengthBtn, addSpeedBtn, addAgilityBtn, addSpiritBtn,addDefensebtn, confirmBtn, cancelBtn;

    private PlayerStats stats;
    private int addHp, addStrength, addSpeed, addAgility, addSpirit , addDefense;

    private void Awake()
    {
        // Gắn sự kiện cho các nút
        addHpBtn.onClick.AddListener(() => AddStat(ref addHp));
        addStrengthBtn.onClick.AddListener(() => AddStat(ref addStrength));
        addSpeedBtn.onClick.AddListener(() => AddStat(ref addSpeed));
        addAgilityBtn.onClick.AddListener(() => AddStat(ref addAgility));
        addSpiritBtn.onClick.AddListener(() => AddStat(ref addSpirit));
        addDefensebtn.onClick.AddListener(() => AddStat(ref addDefense));
        confirmBtn.onClick.AddListener(OnConfirm);
        if (cancelBtn != null) cancelBtn.onClick.AddListener(OnCancel);

        gameObject.SetActive(false); // Mặc định ẩn panel
    }

    public void Show()
    {
        ReloadStats();                 // Load lại chỉ số mới nhất từ server
    }

    public void Hide()
    {
    }

    public void ReloadStats()
    {
        StartCoroutine(AuthManager.Instance.GetPlayerStats(result =>
        {
            if (result != null)
            {
                Debug.Log("[PotentialStatsPanel] Stats loaded: " + JsonUtility.ToJson(result)); // <--- debug

                stats = result;
                ResetAdded();
                ThongTin.instance.UpdateCharacterStatsFromServer(result);
                UpdateUI();
            }
            else
            {
                Debug.LogError("[PotentialStatsPanel] Failed to load stats.");
            }
        }));
    }

    void ResetAdded()
    {
        addHp = addStrength = addSpeed = addAgility = addSpirit = addDefense = 0;
    }

    void UpdateUI()
    {
        Debug.Log($"UpdateUI: {stats.hp}+{addHp}, {stats.strength}+{addStrength}, ...");

        hpText.text = ($"Sinh Lực: {stats.hp + addHp} ").ToString();
        strengthText.text = ($"Sức Mạnh: {stats.strength + addStrength}").ToString();
        speedText.text = ($"Tốc Độ: {stats.speed + addSpeed}").ToString();
        agilityText.text = ($"Nhanh Nhẹn: {stats.agility + addAgility}").ToString();
        spiritText.text = ($"Tinh Thần: {stats.spirit + addSpirit}").ToString();
        defensetext.text = ($"Phòng Thủ: {stats.defense + addDefense}").ToString();
        int used = addHp + addStrength + addSpeed + addAgility + addSpirit;
        potentialText.text = (stats.potentialPoints - used).ToString();
    }

    void AddStat(ref int stat)
    {
        int used = addHp + addStrength + addSpeed + addAgility + addSpirit + addDefense;
        Debug.Log("AddStat called, current stat = " + stat + ", used = " + used + "/" + (stats != null ? stats.potentialPoints : -1));

        if (stats != null && used < stats.potentialPoints)
        {
            stat++;

            UpdateUI();
        }
    }

    void OnConfirm()
    {
        int total = addHp + addStrength + addSpeed + addAgility + addSpirit + addDefense;
        if (total == 0)
        {
            Debug.Log("Bạn chưa cộng điểm nào!");
            return;
        }
        StartCoroutine(AuthManager.Instance.AllocateStats(
            addHp, addStrength, addSpeed, addAgility, addSpirit, addDefense,
            success =>
            {
                if (success)
                {
                    Debug.Log("Cộng điểm thành công!");
                    // Lấy lại chỉ số mới nhất sau khi cộng điểm
                    StartCoroutine(AuthManager.Instance.GetPlayerStats(newStats =>
                    {
                        stats = newStats;
                        ThongTin.instance.UpdateCharacterStatsFromServer(newStats);
                        UpdateUI();
                    }));
                    Hide();
                }
                else
                {
                    Debug.LogError("Cộng điểm thất bại!");
                }
            }
        ));
    }


    void OnCancel()
    {
        Hide(); // Ẩn panel khi bấm Huỷ
    }
}
