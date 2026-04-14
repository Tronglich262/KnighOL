using TMPro;
using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class CharacterQuickInfoPanel : MonoBehaviour
{
    public static CharacterQuickInfoPanel Instance;

    public TMP_Text playerNameText;
    public Button xemThongTinButton;

    private PlayerAvatar _currentTarget;
    private Coroutine _waitNameCoroutine;

    private void Awake()
    {
        Instance = this;
        gameObject.SetActive(false);
    }

    public void Show(PlayerAvatar target, string name = null)
    {
        _currentTarget = target;
        gameObject.SetActive(true);

        if (_waitNameCoroutine != null)
        {
            StopCoroutine(_waitNameCoroutine);
            _waitNameCoroutine = null;
        }

        string displayName = !string.IsNullOrEmpty(name) ? name : target?.DisplayName.ToString();

        if (!string.IsNullOrEmpty(displayName))
        {
            playerNameText.text = displayName;
        }
        else
        {
            playerNameText.text = "Đang tải...";
            _waitNameCoroutine = StartCoroutine(WaitForDisplayName());
        }
    }

    private IEnumerator WaitForDisplayName()
    {
        float timeout = 5f;

        while (_currentTarget != null &&
               string.IsNullOrWhiteSpace(_currentTarget.DisplayName.ToString()) &&
               timeout > 0f)
        {
            timeout -= Time.deltaTime;
            yield return null;
        }

        if (_currentTarget != null)
        {
            string finalName = _currentTarget.DisplayName.ToString();
            playerNameText.text = !string.IsNullOrWhiteSpace(finalName) ? finalName : "Không rõ tên";
        }

        _waitNameCoroutine = null;
    }

    public void OnClickXemThongTin()
    {
        if (_currentTarget == null) return;

        gameObject.SetActive(false);

        string json = _currentTarget.GetFullCharacterJson();

        CharacterPreviewPanel.Instance.ClearPreviewData();
        CharacterPreviewPanel.Instance.gameObject.SetActive(true);

        if (CharacterPreviewPanel.Instance.characterPreview != null)
        {
            WorldChatUIManager.Instance.Chat.SetActive(false);
            QuestDisplay.Instance.TatactiveallQuestDisplay();
            WorldChatUIManager.Instance.chatBar.SetActive(false);
            CharacterUIManager.Instance.CharacterButton.SetActive(false);
            SettingPanel.Instance.Setting.SetActive(false);
        }

        CharacterPreviewPanel.Instance.LoadCharacterFromJson(json);
    }
}