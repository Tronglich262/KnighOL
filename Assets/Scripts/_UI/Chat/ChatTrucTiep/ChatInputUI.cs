using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class ChatInputUI : MonoBehaviour
{
    public TMP_InputField inputField;
    public Button sendButton;

    private PlayerChat playerChat;

    private void Start()
    {
        if (sendButton != null)
            sendButton.onClick.AddListener(OnSendButtonClick);

        if (inputField != null)
            inputField.onSubmit.AddListener(_ => OnSendButtonClick());
    }

    public void SetPlayerChat(PlayerChat pc)
    {
        if (pc != null)
            playerChat = pc;
    }

    public void OnSendButtonClick()
    {
        if (playerChat != null && !string.IsNullOrWhiteSpace(inputField.text))
        {
            playerChat.SendChat(inputField.text);
            inputField.text = "";
            inputField.ActivateInputField();
        }
        else
        {
            Debug.LogWarning("playerChat NULL hoặc input rỗng!");
        }

        if (WorldChatUIManager.Instance != null)
            WorldChatUIManager.Instance.ToggleTatchatlive();
    }

    public PlayerChat FindMyPlayerChat()
    {
        if (playerChat != null)
            return playerChat;

        foreach (var pc in FindObjectsByType<PlayerChat>(FindObjectsSortMode.None))
        {
            if (pc.Object != null && pc.Object.HasInputAuthority)
            {
                playerChat = pc;
                return pc;
            }
        }

        return null;
    }
}