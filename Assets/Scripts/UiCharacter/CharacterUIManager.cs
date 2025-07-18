using Assets.HeroEditor.Common.CommonScripts;
using Assets.HeroEditor.Common.ExampleScripts;
using Unity.VisualScripting.Antlr3.Runtime.Misc;
using UnityEngine;
using UnityEngine.UI;
public class CharacterUIManager : MonoBehaviour
{
    public GameObject characterPanel;
    public GameObject Tui;
    public GameObject TiemNang;
    public GameObject Kynang;
    public GameObject button;
    public GameObject button2;
    public GameObject codecharacterui1;
    public GameObject characterpreviewpanel;
    public GameObject CharacterButton;
    public GameObject nhiemvu;
    public GameObject kynangCharacter;
    public static CharacterUIManager Instance;

    public Image Vien;
    void Awake()
    {
        if (Instance == null) Instance = this;
        else if (Instance != this) Destroy(gameObject);
    }
    private void Start()
    {
        characterPanel.SetActive(false);
        characterpreviewpanel.SetActive(false);
        TiemNang.SetActive(false);
        Kynang.SetActive(false);
        button.SetActive(false);
        Tui.SetActive(false);
        button2.SetActive(false);
        codecharacterui1.SetActive(false);
        Vien.SetActive(false);
        nhiemvu.SetActive(false);
        kynangCharacter.SetActive(false);
    }
 
    public void TogglePanel()
    {
        bool isActive = characterPanel.activeSelf;

        if (isActive)
        {
            // Nếu đang mở, thì ẩn hết
            characterPanel.SetActive(false);
            Tui.SetActive(false);
            TiemNang.SetActive(false);
            Kynang.SetActive(false);
            button.SetActive(false);
            button2.SetActive(false);
            codecharacterui1.SetActive(false);
            Vien.SetActive(false);
            nhiemvu.SetActive(false);
            kynangCharacter.SetActive(false);

        }
        else
        {
            bool canShow =
                   (CharacterPreviewPanel.Instance == null || !CharacterPreviewPanel.Instance.gameObject.activeSelf) &&

         (!WorldChatUIManager.Instance.privateChatPanel.gameObject.activeSelf &&
          !WorldChatUIManager.Instance.chatPanel.gameObject.activeSelf &&
          !WorldChatUIManager.Instance.privateChatListPanel.gameObject.activeSelf &&
          !ShopTriggerTA.Instance.shopPanel.gameObject.activeSelf &&
          !ShopTriggerPK.Instance.shopPanel.gameObject.activeSelf &&
          !ShopTriggerVK.Instance.shopPanel.gameObject.activeSelf &&
          !ShopTriggerTP.Instance.shopPanel.gameObject.activeSelf &&
          !CanvasShop.Instante.canvasShop.gameObject.activeSelf &&
          !CanvasShop.Instante.canvasDaily.gameObject.activeSelf &&
          !CanvasShop.Instante.canvasShopPK.gameObject.activeSelf &&
          !CanvasShop.Instante.canvasShopvk.gameObject.activeSelf &&
          !CanvasShop.Instante.nv.gameObject.activeSelf);

            if (canShow)
            {
                // Nếu tất cả panel chat & preview đều đang tắt thì bật lên
                characterPanel.SetActive(true);
                Tui.SetActive(true);
                TiemNang.SetActive(false);
                Kynang.SetActive(false);
                nhiemvu.SetActive(false);
                kynangCharacter.SetActive(false);
                button.SetActive(true);
                button2.SetActive(true);
                codecharacterui1.SetActive(true);
                Vien.SetActive(true);
                CharacterButton.SetActive(false); // Ẩn nút CharacterButton khi mở panel
                WorldChatUIManager.Instance.ToggleTatCharbarAndChatPrivateList();
                QuestDisplay.Instance.questPanel.SetActive(false);
                bool checktoggle = MovementExample.Instante.checktoggle = true;
                SkillButtonManager.Instance.Skillbutton.SetActive(false);

            }
        }
    }

    public void ToggleThongtin()
    {
        characterPanel.SetActive(true);
        TiemNang.SetActive(false);
        Kynang.SetActive(false);
        nhiemvu.SetActive(false);
        Tui.SetActive(true);
        Vien.SetActive(true);
        kynangCharacter.SetActive(false);


    }
    public void ToggleTiemNang()
    {
        TiemNang.SetActive(true);
        Kynang.SetActive(false);
        characterPanel.SetActive(true);
        Tui.SetActive(false);
        Vien.SetActive(true);
        nhiemvu.SetActive(false);
        kynangCharacter.SetActive(false);
        TiemNang.GetComponent<PotentialStatsPanel>().Show();


    }
    public void ToggleKyNang()
    {
        kynangCharacter.SetActive(true);
        TiemNang.SetActive(false);
        characterPanel.SetActive(true);
        Vien.SetActive(true);
        nhiemvu.SetActive(false);
    }
    public void TuiButton()
    {
        Tui.SetActive(true);
        Kynang.SetActive(false);
        TiemNang.SetActive(false);
        characterPanel.SetActive(true);
        button.SetActive(true);
        Vien.SetActive(true);
        nhiemvu.SetActive(false);

    }
    public void nhiemvuButton()
    {
        nhiemvu.SetActive(true);
        Kynang.SetActive(false);
        TiemNang.SetActive(false);
        characterPanel.SetActive(true);
        button.SetActive(true);
        Vien.SetActive(true);

    }
    public void TTButton()
    {
        characterPanel.SetActive(true);
        Kynang.SetActive(true);
        TiemNang.SetActive(false);
        Tui.SetActive(false);
        button.SetActive(true);
        Vien.SetActive(true);
        nhiemvu.SetActive(false);
        if (ThongTin.instance != null && ThongTin.instance.gameObject.activeInHierarchy)
        {
            ThongTin.instance.StartCoroutine(
                AuthManager.Instance.GetPlayerStats(result =>
                {
                    ThongTin.instance.stats1 = result;
                    ThongTin.instance.UpdateStatsUI();
                })
            );
        }
    }
    //====phần thêm thông tin account khác======
    public void ShowPanelOnlyThongTin()
    {
        characterpreviewpanel.SetActive(true);   // Bật panel chính

    }
    public void ToggleTat()
    {
        
        characterpreviewpanel.SetActive(false);
        CharacterPreviewPanel.Instance.ClearPreviewData();
    }
    public void ToggleTatall()
    {
        CharacterButton.SetActive(true); 
        characterPanel.SetActive(false);
        TiemNang.SetActive(false);
        Kynang.SetActive(false);
        button.SetActive(false);
        Tui.SetActive(false);
        button2.SetActive(false);
        codecharacterui1.SetActive(false);
        Vien.SetActive(false);
        kynangCharacter.SetActive(false);
        nhiemvu.SetActive(false);
        QuestDisplay.Instance.questPanel.SetActive(true);
        WorldChatUIManager.Instance.ToggleBatCharbarAndChatPrivateList();
        bool checktoggle = MovementExample.Instante.checktoggle = false;
        SkillButtonManager.Instance.Skillbutton.SetActive(true);

    }
    //hàm tắt bật characterButton gọi qua các srcip ( cấm động )
    public void ToggleTatCharacterButton()
    {
        CharacterButton.SetActive(false);
    }
    public void ToggleBatCharacterButton()
    {
        CharacterButton.SetActive(true);
    }
}
