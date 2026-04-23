using Assets.HeroEditor.Common.ExampleScripts;
using System.Collections;
using UnityEngine;
using UnityEngine.Networking;

/// <summary>
/// Script này quản lý việc mở Shop.
/// Chỉ khi người chơi đứng trong vùng Trigger mới click được shop.
/// </summary>
public class ShopTriggerTP : MonoBehaviour
{
    [Header("Thiết lập Shop")]
    [Tooltip("Gán bảng UI Shop từ Canvas vào đây")]
    public GameObject shopPanel;

    [Header("Trigger kiểm tra vùng shop")]
    private bool isPlayerInZone = false;
    private Transform playerTransform;

    public static ShopTriggerTP Instance;

    private void Awake()
    {
        Instance = this;
    }

    // Trigger khi player vào vùng
    private void OnTriggerEnter2D(Collider2D other)
    {
        // Nếu là player đúng tag thì mới cho phép
        if (other.CompareTag("Player"))
        {
            isPlayerInZone = true;
            playerTransform = other.transform; // Lưu lại transform player
        }
    }

    // Trigger khi player rời khỏi vùng
    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            isPlayerInZone = false;
            playerTransform = null;
        }
    }

    void Update()
    {
        // Chỉ xử lý click nếu đang đứng trong vùng
        if (!isPlayerInZone)
            return;

        // Kiểm tra các UI khác như code cũ của bạn
        if (CharacterPreviewPanel.Instance != null && CharacterPreviewPanel.Instance.gameObject.activeSelf)
            return;
        if (CharacterUIManager1.Instance != null && CharacterUIManager1.Instance.gameObject.activeSelf)
            return;
        if (WorldChatUIManager.Instance.chatPanel != null && WorldChatUIManager.Instance.chatPanel.gameObject.activeSelf)
            return;
        if (WorldChatUIManager.Instance.privateChatPanel != null && WorldChatUIManager.Instance.privateChatPanel.gameObject.activeSelf)
            return;
        if (WorldChatUIManager.Instance.privateChatListPanel != null && WorldChatUIManager.Instance.privateChatListPanel.gameObject.activeSelf)
            return;
        if (CharacterUIManager.Instance.TiemNang != null && CharacterUIManager.Instance.TiemNang.gameObject.activeSelf)
            return;
        if (CharacterUIManager.Instance.Tui != null && CharacterUIManager.Instance.Tui.gameObject.activeSelf)
            return;
        if (CharacterUIManager.Instance.Kynang != null && CharacterUIManager.Instance.Kynang.gameObject.activeSelf)
            return;
        if (ShopTriggerPK.Instance.shopPanel != null && ShopTriggerPK.Instance.shopPanel.activeSelf)
            return;
        if (ShopTriggerTA.Instance.shopPanel != null && ShopTriggerTA.Instance.shopPanel.activeSelf)
            return;
        if (ShopTriggerVK.Instance.shopPanel != null && ShopTriggerVK.Instance.shopPanel.activeSelf)
            return;
        if (CanvasShop.Instante.canvasShop != null && CanvasShop.Instante.canvasShop.activeSelf)
            return;


        // --- Chỉ cho phép xử lý click nếu đang trong vùng trigger ---
        if (Input.GetMouseButtonDown(0))
        {
            Vector2 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            RaycastHit2D hit = Physics2D.Raycast(mousePos, Vector2.zero);

            if (hit.collider != null && hit.collider.gameObject == gameObject)
            {
                if (shopPanel != null)
                {
                    StartCoroutine(LoadShopTP());

                 //   bool checktoggle = MovementExample.Instante.checktoggle = true;
                    bool nextState = !shopPanel.activeSelf;
                    shopPanel.SetActive(nextState);
                 //   SkillButtonManager.Instance.Skillbutton.SetActive(false);  
                    WorldChatUIManager.Instance.Chat.SetActive(false);
                    QuestDisplay.Instance.TatactiveallQuestDisplay();
                    WorldChatUIManager.Instance.chatBar.SetActive(false);
                    CharacterUIManager.Instance.CharacterButton.SetActive(false);
                    SettingPanel.Instance.Setting.SetActive(false);

                    if (CanvasShop.Instante.canvasShop != null)
                        CanvasShop.Instante.canvasShop.SetActive(false);

                    if (CanvasShop.Instante.canvasShopPK != null)
                        CanvasShop.Instante.canvasShopPK.SetActive(false);

                    if (CanvasShop.Instante.canvasShopvk != null)
                        CanvasShop.Instante.canvasShopvk.SetActive(false);

                    if (SkillButtonManager.Instance.skill != null)
                        SkillButtonManager.Instance.ToggleSkills(false);
                    if (FpsGame.Instance.BangHienThiThongTin != null)
                        FpsGame.Instance.ToggleTatBanghienthithongtin();
                }
                else
                {
                    Debug.LogWarning("ShopPanel chưa được gán trong Inspector!");
                }
            }
        }
    }
    IEnumerator LoadShopTP()
    {
        int npcId = 2;
        string url = $"https://localhost:7124/api/account/npc-shop/{npcId}";

        using (UnityWebRequest www = UnityWebRequest.Get(url))
        {
            yield return www.SendWebRequest();

            if (www.result == UnityWebRequest.Result.Success)
            {
                string json = "{\"items\":" + www.downloadHandler.text + "}";
                var list = JsonUtility.FromJson<NpcShopItemList>(json);

                //  Gọi coroutine từ UI Manager
                yield return StartCoroutine(ShopTPUIManager.Instance.ShowShop(list.items));

                //  Mở UI sau khi dữ liệu sẵn sàng
                shopPanel.SetActive(true);
          //      MovementExample.Instante.checktoggle = true;
              //  SkillButtonManager.Instance.Skillbutton.SetActive(false);
                WorldChatUIManager.Instance.Chat.SetActive(false);
                QuestDisplay.Instance.questPanel.SetActive(false);
              //  CanvasShop.Instante.HideAllCanvas();
            }
            else
            {
                Debug.LogError("Không lấy được shop: " + www.error);
            }
        }
    }

    public void OnClickVestTab()
    {
        ShopTPUIManager.Instance.StartFilterShopByType("Vest");
    }
    public void OnClickPauldronsTab()
    {
        ShopTPUIManager.Instance.StartFilterShopByType("Pauldrons");
    }
    public void OnClickGlovesTab()
    {
        ShopTPUIManager.Instance.StartFilterShopByType("Gloves");
    }
    public void OnClickBeltTab()
    {
        ShopTPUIManager.Instance.StartFilterShopByType("Belt");
    }
    public void OnClickBootsTab()
    {
        ShopTPUIManager.Instance.StartFilterShopByType("Boots");
    }

}
