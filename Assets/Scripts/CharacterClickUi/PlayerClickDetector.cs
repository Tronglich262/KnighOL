using Unity.Jobs;
using UnityEngine;

public class ClickPlayerShowInfo : MonoBehaviour
{

    void Update()
    {
        // Nếu Panel preview đang mở thì bỏ qua click
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



        if (Input.GetMouseButtonDown(0))
        {
            Vector2 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            RaycastHit2D hit = Physics2D.Raycast(mousePos, Vector2.zero);

            if (hit.collider == null) return;
            if (!hit.collider.CompareTag("Player")) return;

            var playerAvatar = hit.collider.GetComponent<PlayerAvatar>();
            if (playerAvatar == null) return;

            string json = playerAvatar.GetFullCharacterJson();
            Debug.Log($"[PREVIEW_CLICK] Clicked {playerAvatar.name}, json = {json}");

            if (CharacterPreviewPanel.Instance == null) return;

            Debug.LogWarning(">>> Đang gọi CLEAR preview data!");
            CharacterPreviewPanel.Instance.ClearPreviewData();
            Debug.LogWarning(">>> Đang bật panel lên");
            CharacterPreviewPanel.Instance.gameObject.SetActive(true);
            Debug.LogWarning(">>> Đang LOAD JSON mới vào preview!");
            CharacterPreviewPanel.Instance.LoadCharacterFromJson(json);
        }
    }


}