using Fusion;
using System;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SettingPanel : MonoBehaviour
{
    public static SettingPanel Instance;
    public GameObject Setting;
    private void Awake()
    {
            Instance = this;
    }
    public GameObject settingPanel;
    public NetworkRunner runner;
    void Start()
    {
        settingPanel.SetActive(false);
    }

    public void showSettingPanel()
    {
        settingPanel.SetActive(true);
        WorldChatUIManager.Instance.privateChatListPanel.SetActive(false);
        WorldChatUIManager.Instance.chatBar.SetActive(false);
        WorldChatUIManager.Instance.Chat.SetActive(false);
        Setting.SetActive(false);
        SkillButtonManager.Instance.ToggleSkills(false);
        FpsGame.Instance.ToggleTatBanghienthithongtin();
        WorldChatUIManager.Instance.Chat.SetActive(false);
        QuestDisplay.Instance.questPanel.SetActive(false);
        QuestDisplay.Instance.TatactiveallQuestDisplay();
        QuestDisplay.Instance.HienQuest.SetActive(false);
    }

    public void offSettingPanel()
    {
        settingPanel.SetActive(false);
        WorldChatUIManager.Instance.privateChatListPanel.SetActive(false);
        WorldChatUIManager.Instance.chatBar.SetActive(true);
        WorldChatUIManager.Instance.Chat.SetActive(true);
        Setting.SetActive(true);
        SkillButtonManager.Instance.ToggleSkills(true);
        FpsGame.Instance.ToggleBatBanghienthithongtin();
        WorldChatUIManager.Instance.Chat.SetActive(true);
        QuestDisplay.Instance.questPanel.SetActive(true);
        QuestDisplay.Instance.BatactiveallQuestDisplay();
        QuestDisplay.Instance.AnQuest.SetActive(true);
    }
    /*public async void ToggleDangXuat()
    {
        // Despawn player object của mình nếu còn
        if (runner != null && PlayerSpawner.LocalPlayerObject != null)
        {
            runner.Despawn(PlayerSpawner.LocalPlayerObject);
            PlayerSpawner.LocalPlayerObject = null; // Tránh lỗi double despawn nếu logout nhiều lần
        }

        // Shutdown runner (ngắt kết nối)
        if (runner != null)
        {
            await runner.Shutdown();
        }

        // Xóa session nếu có
        PlayerPrefs.DeleteKey("accessToken");

        // Quay về login/menu
        SceneManager.LoadScene("Login");
    }*/
}
