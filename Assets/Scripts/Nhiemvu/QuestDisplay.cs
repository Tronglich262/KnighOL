using TMPro;
using Unity.VisualScripting;
using UnityEngine;

public class QuestDisplay : MonoBehaviour
{
    public TMP_Text questListText;
    public static QuestDisplay Instance;
    public GameObject questPanel;
    public QuestResponse[] currentQuests;
    public void Awake()
    {
        Instance = this;
    }
    void Start()
    {
        StartCoroutine(AuthManager.Instance.GetUserQuests(ShowQuestsOnUI));
    }

    void ShowQuestsOnUI(QuestResponse[] quests)
    {
        currentQuests = quests;
        if (quests == null)
        {
            questListText.text = "Không tải được danh sách nhiệm vụ!";
            return;
        }

        questListText.text = "";
        int activeQuestCount = 0;
        foreach (var quest in quests)
        {
            if (!quest.is_completed) // <-- Chỉ show nhiệm vụ chưa hoàn thành
            {
                questListText.text += $"- {quest.description}: {quest.progress}/{quest.targetAmount} (Chưa xong)\n";
                activeQuestCount++;
            }
        }

        if (activeQuestCount == 0)
        {
            questListText.text = "Đã hoàn thành tất cả nhiệm vụ!";
            // Ẩn panel nếu muốn (tuỳ ý)
            if (questPanel != null)
                questPanel.SetActive(false);
        }
        else
        {
            if (questPanel != null)
                questPanel.SetActive(true);
        }
    }

    public void ReloadQuests()
    {
        StartCoroutine(AuthManager.Instance.GetUserQuests(ShowQuestsOnUI));
    }
}
    