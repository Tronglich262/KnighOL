using UnityEngine;

public class MarketNPC : MonoBehaviour
{
    public GameObject shopUIPanel;
    private bool playerInRange = false;

    void Start()
    {
        if (shopUIPanel != null) shopUIPanel.SetActive(false);
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            playerInRange = true;
            // Hiện thông báo UI: "Nhấn E để mở Market"
        }
    }

    void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            playerInRange = false;
            if (shopUIPanel != null) shopUIPanel.SetActive(false);
            // Ẩn thông báo UI
        }
    }
    public void ToggleTat()
    {
        shopUIPanel.SetActive(false);
       // SkillButtonManager.Instance.Skillbutton.SetActive(true);
        QuestDisplay.Instance.BatactiveallQuestDisplay();
        WorldChatUIManager.Instance.Chat.SetActive(true);
        WorldChatUIManager.Instance.chatBar.SetActive(true);
        CharacterUIManager.Instance.CharacterButton.SetActive(true);
        SkillButtonManager.Instance.ToggleSkills(true);
        FpsGame.Instance.ToggleBatBanghienthithongtin();
    }
    void Update()
    {
        if (playerInRange && Input.GetKeyDown(KeyCode.E))
        {
            if (shopUIPanel != null) shopUIPanel.SetActive(true);
         //   SkillButtonManager.Instance.Skillbutton.SetActive(false);
            QuestDisplay.Instance.TatactiveallQuestDisplay();
            WorldChatUIManager.Instance.Chat.SetActive(false);
            WorldChatUIManager.Instance.chatBar.SetActive(false);
            SettingPanel.Instance.Setting.SetActive(false);
            CharacterUIManager.Instance.CharacterButton.SetActive(false);
                SkillButtonManager.Instance.ToggleSkills(false);
                FpsGame.Instance.ToggleTatBanghienthithongtin();


        }
    }
}
