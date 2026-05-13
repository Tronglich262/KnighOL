using Newtonsoft.Json;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class QuestDisplay : MonoBehaviour
{
    public static QuestDisplay Instance;

    public Transform questListParent;
    public GameObject questItemPrefab;
    public GameObject questPanel;

    public GameObject AnQuest;
    public GameObject HienQuest;
    public GameObject nhiemvu;
    public GameObject todoi;

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
        StartCoroutine(CoGetUserQuests(ShowQuestsOnUI));
    }

    private IEnumerator CoGetUserQuests(System.Action<QuestResponse[]> onSuccess)
    {
        yield return ApiClientBase.GetOrCreate().Get<QuestResponse[]>(
            "Account/quests",
            quests =>
            {
                currentQuests = quests;
                onSuccess?.Invoke(quests);
            },
            error => Debug.LogError("Load quest failed: " + error)
        );
    }

    void ShowQuestsOnUI(QuestResponse[] quests)
    {
        currentQuests = quests;

        foreach (Transform child in questListParent)
            Destroy(child.gameObject);

        if (quests == null || quests.Length == 0)
        {
            CreateQuestItem("Kh�ng t?i du?c danh s�ch nhi?m v?!", false, -1, false);
            return;
        }

        int activeQuestCount = 0;
        foreach (var quest in quests)
        {
            if (quest.is_completed) continue;

            string questText = $"- {quest.description}: {quest.progress}/{quest.targetAmount}";
            bool canClaim = quest.progress >= quest.targetAmount && !quest.is_completed;

            if (canClaim)
                CreateQuestItem(questText + " (Ho�n th�nh! Nh?n nh?n thu?ng)", true, quest.quest_ID, false);
            else
                CreateQuestItem(questText + " (Chua xong)", false, -1, false);

            activeQuestCount++;
        }

        if (activeQuestCount == 0)
        {
            CreateQuestItem("�� ho�n th�nh t?t c? nhi?m v?!", false, -1, false);
            if (questPanel != null) questPanel.SetActive(false);
        }
        else
        {
            if (questPanel != null) questPanel.SetActive(true);
        }
    }

    void CreateQuestItem(string text, bool showClaimButton = false, int questId = -1, bool isCompleted = false)
    {
        GameObject item = Instantiate(questItemPrefab, questListParent);

        TMP_Text tmp = item.GetComponentInChildren<TMP_Text>();
        if (tmp != null) tmp.text = text;

        Button claimBtn = item.GetComponentInChildren<Button>(true);
        if (claimBtn != null)
        {
            claimBtn.gameObject.SetActive(showClaimButton && questId != -1);
            if (showClaimButton)
            {
                claimBtn.onClick.RemoveAllListeners();
                claimBtn.onClick.AddListener(() => ClaimReward(questId));
            }
        }

        if (isCompleted && tmp != null)
            tmp.color = Color.gray;
    }

    public void ClaimReward(int questId)
    {
        StartCoroutine(CoClaimReward(questId));
    }

    private IEnumerator CoClaimReward(int questId)
    {
        var dto = new ClaimQuestDto { questId = questId };

        yield return ApiClientBase.GetOrCreate().Post<object>(
            "Account/quests/claim",
            dto,
            _ =>
            {
                Debug.Log("Claim quest reward success!");
                ReloadQuests();
                ItemDetailsUI.Instance.ShowEquipMessage("Nh?n thu?ng th�nh c�ng!");
            },
            error => ItemDetailsUI.Instance.ShowEquipMessage("Nh?n thu?ng th?t b?i: " + error)
        );
    }

    public void ToggleQuestUI()
    {
        bool isActive = questPanel.activeSelf;
        questPanel.SetActive(!isActive);
        if (nhiemvu != null) nhiemvu.SetActive(!isActive);
        if (todoi != null) todoi.SetActive(!isActive);
        if (AnQuest != null) AnQuest.SetActive(!isActive);
        if (HienQuest != null) HienQuest.SetActive(isActive);
    }

    public void TatactiveallQuestDisplay()
    {
        if (questPanel != null) questPanel.SetActive(false);
        if (nhiemvu != null) nhiemvu.SetActive(false);
        if (todoi != null) todoi.SetActive(false);
        if (AnQuest != null) AnQuest.SetActive(false);
    }

    public void BatactiveallQuestDisplay()
    {
        if (questPanel != null) questPanel.SetActive(true);
        if (nhiemvu != null) nhiemvu.SetActive(true);
        if (todoi != null) todoi.SetActive(true);
        if (AnQuest != null) AnQuest.SetActive(true);
    }
}

[System.Serializable]
public class ClaimQuestDto
{
    public int questId;
}
