using Assets.HeroEditor.Common.CommonScripts;
using Assets.HeroEditor.Common.ExampleScripts;
using Fusion;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.Jobs;
using UnityEngine;
using UnityEngine.UI;

public class WorldChatUIManager : NetworkBehaviour
{
    // Dictionary lưu avatar theo tên player
    private Dictionary<string, Sprite> playerAvatars = new();
    [Header("Avatar mặc định (khi chưa có)")]
    public Sprite defaultAvatarSprite;


    private HashSet<string> unreadPartners = new();
    public static WorldChatUIManager Instance;

    [Header("ChatBar (thanh nhỏ)")]
    public GameObject chatBar;
    public TMP_Text[] barLines;
    public Button chatBarButton;

    [Header("ChatPanel (bảng lớn)")]
    public GameObject chatPanel;
    public Transform chatContent;
    public GameObject chatMessagePrefab;
    public TMP_InputField inputField;
    public Button sendButton, closeButton;
    public ScrollRect scrollRect;

    [Header("Private Chat (chat riêng)")]
    public GameObject privateChatPanel;
    public TMP_Text privateTitle;
    public TMP_InputField privateInput;
    public Button privateSendBtn, privateCloseBtn;
    public Transform privateContent;
    public GameObject privateMsgPrefab;
    public Image privateMsgNotifyImage;

    [Header("Private Chat List")]
    public GameObject privateChatListPanel;
    public Transform privateChatListContent;
    public GameObject privateChatNameButtonPrefab;
    public Button showPrivateChatListBtn;

    [Header("Private Chat ngoai")]
    public GameObject chatlive;


    [Header("========")]
    public ChatInputUI chatInputUI;
    public GameObject Chat;


    private readonly List<(string sender, string message)> chatHistory = new();
    private const int maxBarLines = 2;
    private const int maxPanelLines = 50;

    // Key: partnerName, Value: List<(sender, message)>
    private readonly Dictionary<string, List<(string sender, string message)>> privateChatLogs = new();
    private string currentPrivateTargetName = null;
    private HashSet<string> privatePartners = new();
    private Dictionary<string, GameObject> partnerButtons = new();
    public ScrollRect privateScrollRect;
    //cam player
    public Camera avatarCamera;
    public RenderTexture avatarRT;
    private void Awake() => Instance = this;

    private void Start()
    {
        chatBarButton.onClick.AddListener(ShowChatPanel);
        closeButton.onClick.AddListener(ShowOnlyChatBar);
        sendButton.onClick.AddListener(OnSendClicked);
        inputField.onSubmit.AddListener(OnSendEnter);

        privateSendBtn.onClick.AddListener(OnSendPrivate);
        privateCloseBtn.onClick.AddListener(ShowChatPanel);

        if (showPrivateChatListBtn != null)
            showPrivateChatListBtn.onClick.AddListener(ShowPrivateChatList);
        ShowOnlyChatBar();
        UpdateChatBar();
    }

    void ShowOnlyChatBar()
    {
        chatPanel.SetActive(false);
        privateChatPanel.SetActive(false);
        ToggleBatCharbarAndChatPrivateList();
        privateChatListPanel.SetActive(false);
        SkillButtonManager.Instance.Skillbutton.SetActive(true);
        QuestDisplay.Instance.questPanel.SetActive(true);
        currentPrivateTargetName = null;
        
    }

    void ShowChatPanel()
    {
        //check  nếu đang bật thì k cho mở chát , chặn click
        bool isPreviewPanelActive = CharacterPreviewPanel.Instance != null && CharacterPreviewPanel.Instance.gameObject.activeSelf;
        bool isCharacterUIActive = CharacterUIManager1.Instance != null && CharacterUIManager1.Instance.gameObject.activeSelf;
        bool isthongtinUI = CharacterUIManager.Instance.Kynang != null && CharacterUIManager.Instance.Kynang.gameObject.activeSelf;
        bool isInventoryUI = CharacterUIManager.Instance.Tui != null && CharacterUIManager.Instance.Tui.gameObject.activeSelf;
        bool isTiemNang = CharacterUIManager.Instance.TiemNang != null && CharacterUIManager.Instance.TiemNang.gameObject.activeSelf;
        bool isShopPanelActive = ShopTriggerTA.Instance != null && ShopTriggerTA.Instance.shopPanel != null && ShopTriggerTA.Instance.shopPanel.gameObject.activeSelf;
        bool isShopPanelPKActive = ShopTriggerPK.Instance != null && ShopTriggerPK.Instance.shopPanel != null && ShopTriggerPK.Instance.shopPanel.gameObject.activeSelf;
        bool isShopPanelTPTPActive = ShopTriggerTP.Instance != null && ShopTriggerTP.Instance.shopPanel != null && ShopTriggerTP.Instance.shopPanel.gameObject.activeSelf;
        bool isShopPanelVKActive = ShopTriggerVK.Instance != null && ShopTriggerVK.Instance.shopPanel != null && ShopTriggerVK.Instance.shopPanel.gameObject.activeSelf;
        bool isCanvasShoptp = CanvasShop.Instante.canvasShop != null && CanvasShop.Instante.canvasShop != null && CanvasShop.Instante.canvasShop.gameObject.activeSelf;
        bool isCanvasShopvk = CanvasShop.Instante.canvasShopvk != null && CanvasShop.Instante.canvasShopvk != null && CanvasShop.Instante.canvasShopvk.gameObject.activeSelf;
        bool isCanvasShoppk = CanvasShop.Instante.canvasShopPK != null && CanvasShop.Instante.canvasShopPK != null && CanvasShop.Instante.canvasShopPK.gameObject.activeSelf;
        bool isCanvasShopdiemdanh = CanvasShop.Instante.canvasDaily != null && CanvasShop.Instante.canvasDaily != null && CanvasShop.Instante.canvasDaily.gameObject.activeSelf;
        bool isCanvasShopnv = CanvasShop.Instante.nv != null && CanvasShop.Instante.nv != null && CanvasShop.Instante.nv.gameObject.activeSelf;

        if (isPreviewPanelActive || isCharacterUIActive || isShopPanelActive || isthongtinUI || isInventoryUI || isTiemNang || isShopPanelPKActive || isShopPanelTPTPActive || isShopPanelVKActive
            || isCanvasShoptp || isCanvasShopvk || isCanvasShoppk || isCanvasShopdiemdanh || isCanvasShopnv)
            return;
        chatPanel.SetActive(true);
        SkillButtonManager.Instance.Skillbutton.SetActive(false);
        privateChatPanel.SetActive(false);
        privateChatListPanel.SetActive(false);
        QuestDisplay.Instance.questPanel.SetActive(false);
        ToggleTatCharbarAndChatPrivateList();
        foreach (Transform child in chatContent) Destroy(child.gameObject);
        foreach (var (sender, message) in chatHistory) AddMessageToPanel(sender, message);
        inputField.ActivateInputField();
        currentPrivateTargetName = null;

    }

    void ShowPrivateChat(string nickName)
    {
        chatBar.SetActive(false);
        chatPanel.SetActive(false);
        privateChatPanel.SetActive(true);
        privateChatListPanel.SetActive(false);

        currentPrivateTargetName = nickName;
        unreadPartners.Remove(nickName);
        UpdatePrivateMsgNotify();
        UpdatePartnerButtonNotify(nickName, false);
        privateTitle.text = $"Chat với   {nickName}";
        ShowPrivateChatLog(nickName);
        privateInput.text = "";
        privateInput.ActivateInputField();
    }

    void ShowPrivateChatList()
    {
        bool isPreviewPanelActive = CharacterPreviewPanel.Instance != null && CharacterPreviewPanel.Instance.gameObject.activeSelf;
        bool isCharacterUIActive = CharacterUIManager1.Instance != null && CharacterUIManager1.Instance.gameObject.activeSelf;
        bool isthongtinUI = CharacterUIManager.Instance.Kynang != null && CharacterUIManager.Instance.Kynang.gameObject.activeSelf;
        bool isInventoryUI = CharacterUIManager.Instance.Tui != null && CharacterUIManager.Instance.Tui.gameObject.activeSelf;
        bool isTiemNang = CharacterUIManager.Instance.TiemNang != null && CharacterUIManager.Instance.TiemNang.gameObject.activeSelf;
        bool isShopPanelActive = ShopTriggerTA.Instance != null && ShopTriggerTA.Instance.shopPanel != null && ShopTriggerTA.Instance.shopPanel.gameObject.activeSelf;
        bool isShopPanelPKActive = ShopTriggerPK.Instance != null && ShopTriggerPK.Instance.shopPanel != null && ShopTriggerPK.Instance.shopPanel.gameObject.activeSelf;
        bool isShopPanelTPTPActive = ShopTriggerTP.Instance != null && ShopTriggerTP.Instance.shopPanel != null && ShopTriggerTP.Instance.shopPanel.gameObject.activeSelf;
        bool isShopPanelVKActive = ShopTriggerVK.Instance != null && ShopTriggerVK.Instance.shopPanel != null && ShopTriggerVK.Instance.shopPanel.gameObject.activeSelf;
        bool isCanvasShoptp = CanvasShop.Instante.canvasShop != null && CanvasShop.Instante.canvasShop != null && CanvasShop.Instante.canvasShop.gameObject.activeSelf;
        bool isCanvasShopvk = CanvasShop.Instante.canvasShopvk != null && CanvasShop.Instante.canvasShopvk != null && CanvasShop.Instante.canvasShopvk.gameObject.activeSelf;
        bool isCanvasShoppk = CanvasShop.Instante.canvasShopPK != null && CanvasShop.Instante.canvasShopPK != null && CanvasShop.Instante.canvasShopPK.gameObject.activeSelf;
        bool isCanvasShopdiemdanh = CanvasShop.Instante.canvasDaily != null && CanvasShop.Instante.canvasDaily != null && CanvasShop.Instante.canvasDaily.gameObject.activeSelf;
        bool isCanvasShopnv = CanvasShop.Instante.nv != null && CanvasShop.Instante.nv != null && CanvasShop.Instante.nv.gameObject.activeSelf;


        if (isPreviewPanelActive || isCharacterUIActive || isShopPanelActive || isthongtinUI || isInventoryUI || isTiemNang || isShopPanelPKActive || isShopPanelTPTPActive || isShopPanelVKActive
            || isCanvasShoptp || isCanvasShopvk || isCanvasShoppk || isCanvasShopdiemdanh || isCanvasShopnv)
            return;
        chatBar.SetActive(false);
        chatPanel.SetActive(false);
        privateChatPanel.SetActive(false);
        Chat.SetActive(false);
        SettingPanel.Instance.Setting.SetActive(false);
        SkillButtonManager.Instance.Skillbutton.SetActive(false);
        WorldChatUIManager.Instance.Chat.SetActive(false);
        QuestDisplay.Instance.questPanel.SetActive(false);
        privateChatListPanel.SetActive(true);
        currentPrivateTargetName = null;
    }

    private void OnSendClicked() => TrySendChat();
    private void OnSendEnter(string text) => TrySendChat();

    void TrySendChat()
    {
        //StartCoroutine(DelayAndBroadcastAvatarThenSendChat()); // Sử dụng Coroutine
        StartCoroutine(DelayAndBroadcastAvatarThenSendChat());
    }

    [Rpc(RpcSources.All, RpcTargets.All)]
    public void RPC_SendChat(string sender, string message, RpcInfo info = default)
    {
        AddMessage(sender, message);
    }

    public void AddMessage(string sender, string message)
    {
        chatHistory.Add((sender, message));
        if (chatHistory.Count > maxPanelLines)
            chatHistory.RemoveAt(0);

        UpdateChatBar();

        if (chatPanel.activeSelf)
            AddMessageToPanel(sender, message);
        UpdatePrivateMsgNotify();
    }

    private void UpdateChatBar()
    {
        // Luôn lấy dòng chat cuối cùng
        TMP_Text barLineText = barLines[0];
        var avatarImg = chatBar.transform.Find("AvatarImage")?.GetComponent<Image>();
        if (chatHistory.Count > 0)
        {
            var (sender, message) = chatHistory[chatHistory.Count - 1];
            barLineText.text = $"{sender}: {message}";

            if (avatarImg != null)
            {
                var sprite = GetAvatarOfPlayer(sender);
                avatarImg.sprite = sprite;
                avatarImg.color = Color.white;
                Debug.Log($"[ChatBar] Set avatar for sender '{sender}' => Sprite: {(sprite == null ? "NULL" : sprite.name)} | AvatarImg: {avatarImg.name}");
            }
            else
            {
                Debug.LogWarning("[ChatBar] Không tìm thấy AvatarImage");
            }
        }
        else
        {
            barLineText.text = "";
            if (avatarImg != null)
            {
                avatarImg.sprite = defaultAvatarSprite;
                avatarImg.color = Color.white;
                Debug.Log("[ChatBar] Reset avatar img to default");
            }
        }
    }







    private void AddMessageToPanel(string sender, string message)
    {
        bool isMine = sender == PlayerDataHolder1.PlayerName;

        var go = Instantiate(chatMessagePrefab, chatContent);
        var avatarImg = go.transform.Find("AvatarImage")?.GetComponent<Image>();
        var nickBtn = go.transform.Find("NickNameButton")?.GetComponent<Button>();
        var nickText = go.transform.Find("NickNameButton/NickNameText")?.GetComponent<TMP_Text>();
        var msgText = go.transform.Find("MessageText")?.GetComponent<TMP_Text>();
        var layout = go.GetComponent<HorizontalLayoutGroup>();
        var rect = go.GetComponent<RectTransform>();

        if (nickText != null) nickText.text = sender;
        if (msgText != null) msgText.text = message;
        // Thêm avatar
        // Gán avatar
        if (avatarImg != null)
        {
            avatarImg.sprite = GetAvatarOfPlayer(sender);
        }
        // Canh trái/phải tin nhắn chat thế giới
        if (isMine)
        {
            rect.anchorMin = new Vector2(1, rect.anchorMin.y);
            rect.anchorMax = new Vector2(1, rect.anchorMax.y);
            rect.pivot = new Vector2(1, 0.5f);
            rect.anchoredPosition = new Vector2(-10, rect.anchoredPosition.y);
            if (layout) layout.childAlignment = TextAnchor.MiddleRight;
        }
        else
        {
            rect.anchorMin = new Vector2(0, rect.anchorMin.y);
            rect.anchorMax = new Vector2(0, rect.anchorMax.y);
            rect.pivot = new Vector2(0, 0.5f);
            rect.anchoredPosition = new Vector2(10, rect.anchoredPosition.y);
            if (layout) layout.childAlignment = TextAnchor.MiddleLeft;
        }

        if (nickBtn != null)
        {
            nickBtn.onClick.RemoveAllListeners();
            nickBtn.onClick.AddListener(() => ShowPrivateChat(sender));
        }

        Canvas.ForceUpdateCanvases();
        scrollRect.verticalNormalizedPosition = 0f;
    }

    // ==== CHAT RIÊNG ====
    public void OnSendPrivate()
    {
        string msg = privateInput.text.Trim();
        if (!string.IsNullOrEmpty(msg) && !string.IsNullOrEmpty(currentPrivateTargetName))
        {
            if (NicknameSyncManager.NameToPlayerRef.TryGetValue(currentPrivateTargetName, out PlayerRef target))
            {
                AddPrivateMessage(currentPrivateTargetName, PlayerDataHolder1.PlayerName, msg); // Ghi cho mình (bên phải)
                RPC_PrivateMessage(target, PlayerDataHolder1.PlayerName, msg);
            }
            else
            {
                Debug.LogError($"Không tìm thấy PlayerRef cho tên {currentPrivateTargetName}! Hãy thử lại hoặc refresh nickname.");
            }
            privateInput.text = "";
            privateInput.ActivateInputField();
        }
    }

    // Truyền đúng sender, message
    [Rpc(RpcSources.All, RpcTargets.All)]
    public void RPC_PrivateMessage([RpcTarget] PlayerRef target, string sender, string message, RpcInfo info = default)
    {
        AddPrivateMessage(sender, sender, message); // Chỉ ghi log nhận ở máy target
    }

    // Ghi log và hiển thị đúng hướng (bên phải nếu là mình)
    public void AddPrivateMessage(string partner, string sender, string message)
    {
        EnsurePartnerButton(partner);

        if (!privateChatLogs.ContainsKey(partner))
            privateChatLogs[partner] = new List<(string, string)>();

        privateChatLogs[partner].Add((sender, message));

        // Notify
        if (currentPrivateTargetName != partner)
        {
            unreadPartners.Add(partner);
            UpdatePartnerButtonNotify(partner, true);
        }
        else
        {
            unreadPartners.Remove(partner);
            UpdatePartnerButtonNotify(partner, false);
        }

        if (privateChatPanel.activeSelf && currentPrivateTargetName == partner)
            ShowPrivateChatLog(partner);

        UpdatePrivateMsgNotify();
    }

    public void ShowPrivateChatLog(string otherName)
    {
        if (privateContent == null) return;

        foreach (Transform child in privateContent)
            Destroy(child.gameObject);

        if (privateChatLogs.TryGetValue(otherName, out var log))
        {
            foreach (var (sender, message) in log)
            {
                var go = Instantiate(privateMsgPrefab, privateContent);
                var text = go.GetComponentInChildren<TMP_Text>();
                if (text == null) continue;
                text.text = (sender == PlayerDataHolder1.PlayerName ? $"Me: {message}" : $"{sender}: {message}");

                // Căn phải nếu là mình, trái nếu là người khác
                var rect = go.GetComponent<RectTransform>();
                var layout = go.GetComponent<HorizontalLayoutGroup>();
                if (sender == PlayerDataHolder1.PlayerName)
                {
                    rect.pivot = new Vector2(1, 0.5f);
                    rect.anchorMin = new Vector2(1, 0);
                    rect.anchorMax = new Vector2(1, 1);
                    rect.anchoredPosition = new Vector2(-10, 0);
                    if (layout) layout.childAlignment = TextAnchor.MiddleRight;
                }
                else
                {
                    rect.pivot = new Vector2(0, 0.5f);
                    rect.anchorMin = new Vector2(0, 0);
                    rect.anchorMax = new Vector2(0, 1);
                    rect.anchoredPosition = new Vector2(10, 0);
                    if (layout) layout.childAlignment = TextAnchor.MiddleLeft;
                }
            }
        }
        Canvas.ForceUpdateCanvases();
        if (privateScrollRect != null)
            privateScrollRect.verticalNormalizedPosition = 0f;
    }

    void EnsurePartnerButton(string partnerName)
    {
        if (privatePartners.Contains(partnerName)) return;
        privatePartners.Add(partnerName);

        var go = Instantiate(privateChatNameButtonPrefab, privateChatListContent);
        go.GetComponentInChildren<TMP_Text>().text = partnerName;
        go.GetComponent<Button>().onClick.AddListener(() => ShowPrivateChat(partnerName));
        partnerButtons[partnerName] = go;
    }

    public void ToggleTat()
    {
        privateChatListPanel.SetActive(false);
        chatBar.SetActive(true);
        Chat.SetActive(true);
        SettingPanel.Instance.Setting.SetActive(true);
       SkillButtonManager.Instance.Skillbutton.SetActive(true);
        WorldChatUIManager.Instance.Chat.SetActive(true);
        QuestDisplay.Instance.questPanel.SetActive(true);
    }

    void UpdatePrivateMsgNotify()
    {
        if (privateMsgNotifyImage != null)
            privateMsgNotifyImage.gameObject.SetActive(unreadPartners.Count > 0);
    }

    void UpdatePartnerButtonNotify(string partnerName, bool hasNotify)
    {
        if (partnerButtons.TryGetValue(partnerName, out var btnGO))
        {
            var notifyImg = btnGO.transform.Find("NotifyImage")?.gameObject;
            if (notifyImg != null)
                notifyImg.SetActive(hasNotify);
        }
    }
    //lấy hình ảnh nhân vật
    public Sprite GetPlayerAvatarSprite()
    {
        RenderTexture currentRT = RenderTexture.active;
        RenderTexture.active = avatarRT; // avatarRT là RenderTexture của Camera clone

        Texture2D tex = new Texture2D(avatarRT.width, avatarRT.height, TextureFormat.RGBA32, false);
        tex.ReadPixels(new Rect(0, 0, avatarRT.width, avatarRT.height), 0, 0);
        tex.Apply();
        RenderTexture.active = currentRT;

        return Sprite.Create(tex, new Rect(0, 0, tex.width, tex.height), new Vector2(0.5f, 0.5f));
    }
    // Hàm gọi khi gửi chat hoặc nhận chat

    // Lấy avatar cho player
    public Sprite GetAvatarOfPlayer(string playerName)
    {
        if (playerAvatars.TryGetValue(playerName, out var sprite) && sprite != null)
            return sprite;
        return defaultAvatarSprite;
    }
    public byte[] GetAvatarBytes()
    {
        RenderTexture currentRT = RenderTexture.active;
        RenderTexture.active = avatarRT;
        Texture2D tex = new Texture2D(avatarRT.width, avatarRT.height, TextureFormat.RGBA32, false);
        tex.ReadPixels(new Rect(0, 0, avatarRT.width, avatarRT.height), 0, 0);
        tex.Apply();
        RenderTexture.active = currentRT;
        byte[] bytes = tex.EncodeToPNG();
        UnityEngine.Object.Destroy(tex);
        return bytes;
    }
    public void BroadcastAvatarToOthers()
    {
        byte[] avatarBytes = GetAvatarBytes();
        RPC_UpdateAvatar(PlayerDataHolder1.PlayerName, avatarBytes);
    }


    // 3. RPC nhận avatar từ client khác
    [Rpc(RpcSources.All, RpcTargets.All)]
    public void RPC_UpdateAvatar(string playerName, byte[] avatarBytes, RpcInfo info = default)
    {
        var tex = new Texture2D(2, 2);
        tex.LoadImage(avatarBytes);
        Sprite sprite = Sprite.Create(tex, new Rect(0, 0, tex.width, tex.height), new Vector2(0.5f, 0.5f));
        if (playerAvatars.ContainsKey(playerName))
            playerAvatars[playerName] = sprite;
        else
            playerAvatars.Add(playerName, sprite);
    }
    public void UpdateMyAvatar()
    {
        var sprite = GetPlayerAvatarSprite();
        playerAvatars[PlayerDataHolder1.PlayerName] = sprite;
    }
    //deley gửi hình ảnh
    private IEnumerator DelayAndBroadcastAvatarThenSendChat()
    {
        // Nếu vừa đổi đồ, phải đợi ít nhất 1 frame để RenderTexture update
        yield return null; // Delay 1 frame
        UpdateMyAvatar();

        string msg = inputField.text.Trim();
        if (!string.IsNullOrEmpty(msg))
        {
            string sender = PlayerDataHolder1.PlayerName;
            RPC_SendChat(sender, msg);
            inputField.text = "";
            inputField.ActivateInputField();
        }
    }

    public void ToggleTatChat()
    {
        chatlive.SetActive(!chatlive.activeSelf);
        // Nếu vừa bật chat lên (từ false thành true)
        if (chatlive.activeSelf)
        {
            // Gọi đúng qua instance, KHÔNG dùng tên class!
            var myPlayerChat = chatInputUI.FindMyPlayerChat();
            chatInputUI.SetPlayerChat(myPlayerChat);
        }
    }
    
   
    //hàm tắt bật characterButton gọi qua các srcip ( cấm động )
    public void ToggleTatCharbarAndChatPrivateList()
    {
        chatBar.SetActive(false);
        Chat.SetActive(false);
        SettingPanel.Instance.Setting.SetActive(false);
       // SkillButtonManager.Instance.Skillbutton.SetActive(false);
    }
    public void ToggleBatCharbarAndChatPrivateList()
    {
        chatBar.SetActive(true);
        Chat.SetActive(true);
        SettingPanel.Instance.Setting.SetActive(true);
       // SkillButtonManager.Instance.Skillbutton.SetActive(true);


    }
    public void ToggleTatchatlive()
    {
        chatlive.SetActive(false);
    }


}
