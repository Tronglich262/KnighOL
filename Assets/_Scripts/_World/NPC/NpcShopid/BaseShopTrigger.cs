using Assets.HeroEditor.Common.ExampleScripts;
using System.Collections;
using UnityEngine;
using UnityEngine.Networking;

public abstract class BaseShopTrigger : MonoBehaviour
{
    [Header("Thiết lập Shop")]
    [Tooltip("Gán bảng UI Shop từ Canvas vào đây")]
    public GameObject shopPanel;

    protected abstract int NpcId { get; }
    protected abstract BaseShopUIManager ShopUIManager { get; }

    private bool isPlayerInZone = false;

    protected virtual void Awake() { }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
            isPlayerInZone = true;
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
            isPlayerInZone = false;
    }

    protected virtual void Update()
    {
        if (!isPlayerInZone) return;

        // Bỏ qua nếu đang mở UI khác
        if (IsAnyOtherUIPanelOpen()) return;

        if (Input.GetMouseButtonDown(0))
        {
            Vector2 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            RaycastHit2D hit = Physics2D.Raycast(mousePos, Vector2.zero);

            if (hit.collider != null && hit.collider.gameObject == gameObject)
            {
                StartCoroutine(OpenShop());
            }
        }
    }

    private bool IsAnyOtherUIPanelOpen()
    {
        return CharacterPreviewPanel.Instance?.gameObject.activeSelf == true ||
               CharacterUIManager1.Instance?.gameObject.activeSelf == true ||
               WorldChatUIManager.Instance?.chatPanel?.activeSelf == true ||
               WorldChatUIManager.Instance?.privateChatPanel?.activeSelf == true ||
               WorldChatUIManager.Instance?.privateChatListPanel?.activeSelf == true ||
               CharacterUIManager.Instance?.TiemNang?.gameObject.activeSelf == true ||
               CharacterUIManager.Instance?.Tui?.gameObject.activeSelf == true ||
               CharacterUIManager.Instance?.Kynang?.gameObject.activeSelf == true ||
               ShopTriggerTA.Instance?.shopPanel?.activeSelf == true ||
               false;
    }

    private IEnumerator OpenShop()
    {
        string endpoint = $"account/npc-shop/{NpcId}";
        string url = ApiConfigManager.Instance.GetFullUrl(endpoint);

        using (UnityWebRequest www = UnityWebRequest.Get(url))
        {
            yield return www.SendWebRequest();

            if (www.result == UnityWebRequest.Result.Success)
            {
                string json = "{\"items\":" + www.downloadHandler.text + "}";
                var list = JsonUtility.FromJson<NpcShopItemList>(json);

                yield return StartCoroutine(ShopUIManager.ShowShop(list.items));

                if (shopPanel != null)
                    shopPanel.SetActive(true);

                // Tắt UI khác (giữ nguyên logic cũ)
                WorldChatUIManager.Instance?.Chat?.SetActive(false);
                QuestDisplay.Instance?.TatactiveallQuestDisplay();
                WorldChatUIManager.Instance?.chatBar?.SetActive(false);
                CharacterUIManager.Instance?.CharacterButton?.SetActive(false);
                SettingPanel.Instance?.Setting?.SetActive(false);

                if (CanvasShop.Instante != null)
                {
                    CanvasShop.Instante.canvasShop?.SetActive(false);
                    CanvasShop.Instante.canvasShopPK?.SetActive(false);
                    CanvasShop.Instante.canvasShopvk?.SetActive(false);
                }

                if (SkillButtonManager.Instance?.skill != null)
                    SkillButtonManager.Instance.ToggleSkills(false);
                if (FpsGame.Instance?.BangHienThiThongTin != null)
                    FpsGame.Instance.ToggleTatBanghienthithongtin();
            }
            else
            {
                Debug.LogError($"Không load được shop NPC {NpcId}: " + www.error);
            }
        }
    }

    public virtual void CloseShop()
    {
        if (shopPanel != null) shopPanel.SetActive(false);
    }
}