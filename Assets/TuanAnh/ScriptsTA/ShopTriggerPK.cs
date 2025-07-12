using Assets.HeroEditor.Common.ExampleScripts;
using System.Collections;
using UnityEngine;
using UnityEngine.Networking;

/// <summary>
/// Script này quản lý việc mở Shop PK.
/// Chỉ khi người chơi đứng trong vùng Trigger mới click được shop.
/// </summary>
public class ShopTriggerPK : MonoBehaviour
{
    [Header("Thiết lập Shop")]
    [Tooltip("Gán bảng UI Shop từ Canvas vào đây")]
    public GameObject shopPanel;

    [Header("Trigger kiểm tra vùng shop")]
    private bool isPlayerInZone = false;
    private Transform playerTransform;

    public static ShopTriggerPK Instance;

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
        if (ShopTriggerTA.Instance.shopPanel != null && ShopTriggerTA.Instance.shopPanel.activeSelf)
            return;
        if (ShopTriggerTP.Instance.shopPanel != null && ShopTriggerTP.Instance.shopPanel.activeSelf)
            return;
        if (ShopTriggerVK.Instance.shopPanel != null && ShopTriggerVK.Instance.shopPanel.activeSelf)
            return;
        if (CanvasShop.Instante.canvasShopPK != null && CanvasShop.Instante.canvasShopPK.activeSelf)
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
                    StartCoroutine(LoadShopPK());
                    bool checkToggle = MovementExample.Instante.checktoggle  =  true;
                    bool nextState = !shopPanel.activeSelf;
                    shopPanel.SetActive(nextState);
                    SkillButtonManager.Instance.Skillbutton.SetActive(false);
                    if (CanvasShop.Instante.canvasShop != null)
                        CanvasShop.Instante.canvasShop.SetActive(false);

                    if (CanvasShop.Instante.canvasDaily != null)
                        CanvasShop.Instante.canvasDaily.SetActive(false);

                    if (CanvasShop.Instante.canvasShopPK != null)
                        CanvasShop.Instante.canvasShopPK.SetActive(false);

                    if (CanvasShop.Instante.canvasShopvk != null)
                        CanvasShop.Instante.canvasShopvk.SetActive(false);

                    if (CanvasShop.Instante.nv != null)
                        CanvasShop.Instante.nv.SetActive(false);
                }
                else
                {
                    Debug.LogWarning("ShopPanel chưa được gán trong Inspector!");
                }
            }
        }

    }
    IEnumerator LoadShopPK()
    {
        int npcId = 1; // ví dụ Shop PK là id 1
        string url = $"https://localhost:7124/api/account/npc-shop/{npcId}";
        using (UnityWebRequest www = UnityWebRequest.Get(url))
        {
            yield return www.SendWebRequest();
            if (www.result == UnityWebRequest.Result.Success)
            {
                string json = "{\"items\":" + www.downloadHandler.text + "}";
                var list = JsonUtility.FromJson<NpcShopItemList>(json);
                // Gọi hàm show UI và truyền list.items vào (shopPanel)
                ShopPKUIManager.Instance.ShowShop(list.items);
            }
            else
            {
                Debug.LogError("Không lấy được shop: " + www.error);
            }
        }
    }
    public void OnClickCapeTab()
    {
        ShopPKUIManager.Instance.FilterShopByType("Cape");
        Debug.Log("Cape tab clicked");
    }
    public void OnClickMaskTab()
    {
        ShopPKUIManager.Instance.FilterShopByType("Mask");
    }
    public void OnClickGlassesTab()
    {
        ShopPKUIManager.Instance.FilterShopByType("Glasses");
    }
    public void OnClickHairTab()
    {
        ShopPKUIManager.Instance.FilterShopByType("Hair");
    }
    public void OnClickBackTab()
    {
        ShopPKUIManager.Instance.FilterShopByType("Back");
    }


}
