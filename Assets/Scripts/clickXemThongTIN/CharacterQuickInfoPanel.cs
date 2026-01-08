using TMPro;
using UnityEngine;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;
using System.Collections;

public class CharacterQuickInfoPanel : MonoBehaviour
{
    public static CharacterQuickInfoPanel Instance;

    public TMP_Text playerNameText;
    public Button xemThongTinButton;

    private PlayerAvatar _currentTarget;

    private void Awake()
    {
        Instance = this;
        gameObject.SetActive(false);
    }

    public void Show(PlayerAvatar target, string name = null)
    {
        _currentTarget = target;
        gameObject.SetActive(true);

        string displayName = !string.IsNullOrEmpty(name) ? name : target?.DisplayName.ToString();

        Debug.Log($"[QuickInfoPanel] Click vào: {target?.Object.InputAuthority} - DisplayName: {displayName}");

        if (!string.IsNullOrEmpty(displayName))
        {
            playerNameText.text = displayName;
        }
        else
        {
            playerNameText.text = "Đang tải...";
            StartCoroutine(WaitForDisplayName());
        }
    }




    private IEnumerator WaitForDisplayName()
    {
        float timeout = 5f;

        while (_currentTarget != null && string.IsNullOrWhiteSpace(_currentTarget.DisplayName.ToString()) && timeout > 0)
        {
            timeout -= Time.deltaTime;
            yield return null;
        }

        if (_currentTarget != null)
        {
            if (!string.IsNullOrWhiteSpace(_currentTarget.DisplayName.ToString()))
            {
                playerNameText.text = _currentTarget.DisplayName.ToString();
            }
            else
            {
                playerNameText.text = "Không rõ tên";
            }
        }
    }





    public void OnClickXemThongTin()
    {
        if (_currentTarget == null) return;

        // Ẩn panel nhanh
        gameObject.SetActive(false);

        // Gọi qua PreviewPanel như trước đây
        string json = _currentTarget.GetFullCharacterJson();
        CharacterPreviewPanel.Instance.ClearPreviewData();
        CharacterPreviewPanel.Instance.gameObject.SetActive(true);
        if (CharacterPreviewPanel.Instance.characterPreview != null)
        {
            // SkillButtonManager.Instance.Skillbutton.SetActive(false);
            WorldChatUIManager.Instance.Chat.SetActive(false);
            QuestDisplay.Instance.TatactiveallQuestDisplay();
            WorldChatUIManager.Instance.chatBar.SetActive(false);
            CharacterUIManager.Instance.CharacterButton.SetActive(false);
            SettingPanel.Instance.Setting.SetActive(false);

        }

        CharacterPreviewPanel.Instance.LoadCharacterFromJson(json);
    }
}
