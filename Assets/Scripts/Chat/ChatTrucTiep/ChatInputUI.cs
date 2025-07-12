using UnityEngine;
using TMPro;
using UnityEngine.UI; // <-- nhớ import namespace này

public class ChatInputUI : MonoBehaviour
{
    public TMP_InputField inputField;
    public Button sendButton; // <-- Thêm trường button
    private PlayerChat playerChat;
    private  ChatInputUI Instate;
    public void Awake()
    {
        Instate = this;
    }
    private void Start()
    {
        // Gắn hàm OnSendButtonClick cho button (nếu quên kéo trong Inspector)
        if (sendButton != null)
            sendButton.onClick.AddListener(OnSendButtonClick);
    }

    public void SetPlayerChat(PlayerChat pc)
    {
        playerChat = pc;
    }

    void Update()
    {
        if (inputField.isFocused && Input.GetKeyDown(KeyCode.KeypadEnter) && playerChat != null)
        {
            if (!string.IsNullOrWhiteSpace(inputField.text))
            {
                playerChat.SendChat(inputField.text);
                inputField.text = "";
            }
        }
    }

    public void OnSendButtonClick()
    {
        Debug.Log("Bấm Send!");
        playerChat = FindMyPlayerChat();
        if (playerChat != null && !string.IsNullOrWhiteSpace(inputField.text))
        {
            Debug.Log("playerChat.SendChat: " + inputField.text);
            playerChat.SendChat(inputField.text);
            inputField.text = "";
            inputField.ActivateInputField();
        }
        else
        {
            Debug.LogWarning("playerChat NULL hoặc input rỗng!");
        }
        WorldChatUIManager.Instance.ToggleTatchatlive();
    }
    //tìm playerchat trong scene 
    public PlayerChat FindMyPlayerChat()
    {
        foreach (var pc in FindObjectsOfType<PlayerChat>())
        {
            if (pc.Object != null && pc.Object.HasInputAuthority)
                return pc;
        }
        return null;
    }

}
