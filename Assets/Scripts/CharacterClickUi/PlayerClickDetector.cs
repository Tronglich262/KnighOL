using Unity.Jobs;
using UnityEngine;

public class ClickPlayerShowInfo : MonoBehaviour
{

    void Update()
    {
        if (IsAnyUIPanelOpen()) return;

        if (Input.GetMouseButtonDown(0))
        {
            Vector2 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            RaycastHit2D hit = Physics2D.Raycast(mousePos, Vector2.zero);

            if (hit.collider != null && hit.collider.CompareTag("Player"))
            {
                var playerAvatar = hit.collider.GetComponentInParent<PlayerAvatar>();
                if (playerAvatar != null && !playerAvatar.IsLocalPlayer())
                {
                    CharacterQuickInfoPanel.Instance.Show(playerAvatar);
                }
            }
        }
    }

    bool IsAnyUIPanelOpen()
    {
        return
            (CharacterPreviewPanel.Instance?.gameObject.activeSelf ?? false)
            || (CharacterUIManager1.Instance?.gameObject.activeSelf ?? false)

            || (WorldChatUIManager.Instance?.chatPanel?.gameObject.activeSelf ?? false)
            || (WorldChatUIManager.Instance?.privateChatPanel?.gameObject.activeSelf ?? false)
            || (WorldChatUIManager.Instance?.privateChatListPanel?.gameObject.activeSelf ?? false)

            || (CharacterUIManager.Instance?.TiemNang?.gameObject.activeSelf ?? false)
            || (CharacterUIManager.Instance?.Tui?.gameObject.activeSelf ?? false)
            || (CharacterUIManager.Instance?.Kynang?.gameObject.activeSelf ?? false);
    }



}