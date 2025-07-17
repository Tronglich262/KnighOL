using UnityEngine;
using TMPro;

public class Nhiemvuinfo : MonoBehaviour
{
    public TMP_Text nhiemVuText;  // Kéo text ở Inspector vào
    public static Nhiemvuinfo Instance;
    public void Awake()
    {
        Instance =  this;
    }
    void Start()
    {
        ShowNhiemVu(QuestDisplay.Instance.currentQuests);
    }
    void OnEnable()
    {
        if (QuestDisplay.Instance != null)
            ShowNhiemVu(QuestDisplay.Instance.currentQuests);
    }

    public void ShowNhiemVu(QuestResponse[] quests)
    {
        if (quests == null)
        {
            nhiemVuText.text = "Không có dữ liệu nhiệm vụ!";
            return;
        }
        nhiemVuText.text = "";
        foreach (var quest in quests)
        {
            // Ở đây show tất cả, không cần ẩn completed
            nhiemVuText.text += $"- {quest.description}: {quest.progress}/{quest.targetAmount} ({(quest.is_completed ? "Hoàn thành" : "Chưa xong")})\n";
        }
    }
}
