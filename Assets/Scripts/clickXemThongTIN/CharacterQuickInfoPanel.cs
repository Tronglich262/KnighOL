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

        if (!string.IsNullOrEmpty(name))
        {
            playerNameText.text = name;
        }
        else if (!string.IsNullOrEmpty(target.DisplayName.ToString()))
        {
            playerNameText.text = target.DisplayName.ToString();
        }
        else
        {
            playerNameText.text = "Đang tải...";
            StartCoroutine(WaitForDisplayName());
        }
    }

    private IEnumerator WaitForDisplayName()
    {
        float timeout = 5f; // Tăng timeout
        while (_currentTarget != null && string.IsNullOrEmpty(_currentTarget.DisplayName.ToString()))
        {
            timeout -= Time.deltaTime;
            yield return null;
        }

        if (_currentTarget != null)
        {
            if (!string.IsNullOrEmpty(_currentTarget.DisplayName.ToString()))
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
        CharacterPreviewPanel.Instance.LoadCharacterFromJson(json);
    }
}
