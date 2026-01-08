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
            var no = p.GetComponent<NetworkObject>();
            if (no == null || !no.HasInputAuthority) continue;

            var ts = p.GetComponent<TargetingSystem>();
            if (ts == null) return;

            // ===== ENEMY =====
            if (enemy != null)
            {
                ts.SetManualEnemy(enemy);
                return;
            }

            // ===== PLAYER =====
            if (playerInfo != null)
            {
                var selfNo = no;
                if (!playerInfo.CanBeTargetedBy(selfNo))
                    return; // ❌ không target bản thân

                ts.SetManualPlayer(playerInfo);
                return;
            }

            // ===== NPC =====
            ts.SetManualVisual(transform);
            return;
        }
    }
}
