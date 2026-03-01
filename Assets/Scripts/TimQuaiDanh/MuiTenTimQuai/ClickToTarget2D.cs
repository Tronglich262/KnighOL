using Fusion;
using UnityEngine;

public class ClickToTarget2D : MonoBehaviour
{
    private Enemy enemy;
    private PlayerInfo playerInfo;

    private void Awake()
    {
        enemy = GetComponent<Enemy>();
        playerInfo = GetComponent<PlayerInfo>();
    }

    private void OnMouseDown()
    {
        foreach (var p in GameObject.FindGameObjectsWithTag("Player"))
        {
            var localNO = p.GetComponent<NetworkObject>();
            if (localNO == null || !localNO.HasInputAuthority) continue;

            var ts = p.GetComponent<TargetingSystem>();
            if (ts == null) return;

            // ======================
            // ENEMY
            // ======================
            if (enemy != null)
            {
                ts.SetManualEnemy(enemy);
                return;
            }

            // ======================
            // PLAYER
            // ======================
            if (playerInfo != null)
            {
                // ❌ không target chính mình
                if (!playerInfo.CanBeTargetedBy(localNO))
                    return;

                // Transform player được click
                Transform playerTransform = playerInfo.GetComponent<Transform>();

                // 🔹 LUÔN SET TARGET TRƯỚC
                if (ts.CurrentVisualTarget != playerTransform)
                {
                    ts.SetManualPlayer(playerInfo);
                }

                // 🔹 HIỆN THÔNG TIN PLAYER NGAY LẬP TỨC (1 CLICK)
                var avatar = playerInfo.GetComponent<PlayerAvatar>();
                var nameTag = playerInfo.GetComponentInChildren<NameTagManager>();
                string nick = nameTag != null ? nameTag.Nickname : null;

                if (avatar != null && CharacterQuickInfoPanel.Instance != null)
                {
                    CharacterQuickInfoPanel.Instance.Show(avatar, nick);
                }
                return;
            }


            // ======================
            // NPC
            // ======================
            ts.SetManualVisual(transform);
            return;
        }
    }
}
