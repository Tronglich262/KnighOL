using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Networking;
using System.Text;
using System.Collections;

public class QuestDisplay : MonoBehaviour
{
    public Transform questListParent;
    public GameObject questItemPrefab;
    public static QuestDisplay Instance;
    public GameObject questPanel;
    public QuestResponse[] currentQuests;

    private void Awake()
    {
        Instance = this;
    }
    void Start()
    {
        ReloadQuests();
    }

    public void ReloadQuests()
    {
        StartCoroutine(AuthManager.Instance.GetUserQuests(ShowQuestsOnUI));
    }

    void ShowQuestsOnUI(QuestResponse[] quests)
    {
        currentQuests = quests;

        foreach (Transform child in questListParent)
            Destroy(child.gameObject);

        if (quests == null || quests.Length == 0)
        {
            CreateQuestItem("Không tải được danh sách nhiệm vụ!", false, -1, false);
            return;
        }

        int activeQuestCount = 0;
        foreach (var quest in quests)
        {
            string questText = $"- {quest.description}: {quest.progress}/{quest.targetAmount}";
            bool canClaim = quest.progress >= quest.targetAmount && !quest.is_completed;
            bool isDone = quest.is_completed;

            if (!isDone || canClaim)
                activeQuestCount++;

            if (canClaim)
                CreateQuestItem(questText + " (Hoàn thành! Nhấn nhận thưởng)", true, quest.quest_ID, false);
            else if (isDone)
                CreateQuestItem(questText + " (Đã nhận thưởng)", false, -1, true);
            else
                CreateQuestItem(questText + " (Chưa xong)", false, -1, false);
        }

        if (activeQuestCount == 0)
        {
            CreateQuestItem("Đã hoàn thành tất cả nhiệm vụ!", false, -1, false);
            if (questPanel != null)
                questPanel.SetActive(false);
        }
        else
        {
            if (questPanel != null)
                questPanel.SetActive(true);
        }
    }

    void CreateQuestItem(string text, bool showClaimButton = false, int questId = -1, bool isCompleted = false)
    {
        GameObject item = Instantiate(questItemPrefab, questListParent);

        TMP_Text tmp = item.GetComponentInChildren<TMP_Text>();
        if (tmp != null)
            tmp.text = text;

        Button claimBtn = item.GetComponentInChildren<Button>(true);

        if (claimBtn != null)
        {
            if (showClaimButton && questId != -1)
            {
                claimBtn.gameObject.SetActive(true);
                claimBtn.onClick.RemoveAllListeners();
                claimBtn.onClick.AddListener(() => ClaimReward(questId));
            }
            else
            {
                claimBtn.gameObject.SetActive(false);
            }
        }

        if (isCompleted && tmp != null)
        {
            tmp.color = Color.gray;
        }
    }

    void ClaimReward(int questId)
    {
        StartCoroutine(ClaimRewardCoroutine(questId));
    }

    IEnumerator ClaimRewardCoroutine(int questId)
    {
        ClaimQuestDto dto = new ClaimQuestDto { questId = questId };
        string json = JsonUtility.ToJson(dto);
        string url = AuthManager.Instance.apiUrl + "/quests/claim";

        UnityWebRequest req = new UnityWebRequest(url, "POST");
        byte[] bodyRaw = Encoding.UTF8.GetBytes(json);
        req.uploadHandler = new UploadHandlerRaw(bodyRaw);
        req.downloadHandler = new DownloadHandlerBuffer();
        req.SetRequestHeader("Content-Type", "application/json");
        req.SetRequestHeader("Authorization", "Bearer " + AuthManager.Instance.UserSession.Token);

        yield return req.SendWebRequest();

        Debug.Log("Server trả về: " + req.downloadHandler.text);

        if (req.result == UnityWebRequest.Result.Success)
        {
            // Reload lại quest UI
            ReloadQuests();

            // **Reload lại PlayerState**
            StartCoroutine(AuthManager.Instance.GetPlayerState((state) => {
                if (state != null)
                {
                    ItemDetailsUI.Instance.ShowEquipMessage($"+{state.gold}gold +{state.exp}exp");
                    // Nếu có dùng PlayerDataHolder1 thì gán lại luôn:
                    PlayerDataHolder1.CurrentPlayerState = state;
                }
            }));
        }
        else
        {
            ItemDetailsUI.Instance.ShowEquipMessage("Nhận thưởng thất bại: " + req.downloadHandler.text);
        }
    }

}

// DTO cho claim quest reward
[System.Serializable]
public class ClaimQuestDto
{
    public int questId;
}
