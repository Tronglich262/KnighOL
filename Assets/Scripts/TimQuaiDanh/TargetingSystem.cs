using Fusion;
using System.Collections.Generic;
using UnityEngine;

public class TargetingSystem : MonoBehaviour
{
    [Header("Layers (2D)")]
    public LayerMask enemyLayer;
    public LayerMask npcLayer;
    public LayerMask playerLayer; // Layer cho player khác

    [Header("Ranges")]
    public float enemyRadius = 12f;
    public float npcRadius = 8f;
    public float playerRadius = 15f; // Range cho việc target player
    public float loseTargetDistance = 15f;

    [Header("Tab Target")]
    public KeyCode tabKey = KeyCode.Tab;
    public bool includeNPCInTab = false;          // MMO thường Tab chỉ chọn enemy
    public bool includePlayerInTab = true;       // Tab cũng chọn được player khác
    public bool requireLineOfSight = false;       // nếu muốn check raycast
    public float tabCooldown = 0.10f;

    [Header("Soft Target")]
    public bool enableSoftTarget = true;          // auto highlight khi chưa có manual
    public float softTargetHysteresis = 1.5f;     // chống nhảy target liên tục

    [Header("Indicator")]
    public TargetIndicator indicatorPrefab;
    private TargetIndicator indicatorInstance;

    [Header("UI")]
    public TargetInfoPanel targetInfoPanel;

    // ===== Runtime =====
    public Enemy CurrentEnemy { get; private set; }             // combat target
    public Transform CurrentVisualTarget { get; private set; }  // enemy hoặc npc
    public bool HasManualTarget => mode == TargetMode.Manual;
    public PlayerInfo CurrentPlayer { get; private set; }

    private enum TargetMode { None, Soft, Manual }
    private TargetMode mode = TargetMode.None;

    private float lastTabTime;
    private Transform lastSoftCandidate;
    private float lastSoftDist = float.MaxValue;

    private void Start()
    {
        var netObj = GetComponent<NetworkObject>();

        // ❗ CHỈ LOCAL PLAYER
        if (netObj == null || !netObj.HasInputAuthority)
        {
            enabled = false;
            return;
        }

        indicatorInstance = Instantiate(indicatorPrefab);
        indicatorInstance.SetTarget(null);

        // ✅ BIND UI TỰ ĐỘNG
        targetInfoPanel = TargetInfoPanel.Instance;
    }


    private void Update()
    {
        // 0) Safety: target object bị despawn / destroy -> clear ngay
        ValidateTarget();

        // 1) TAB cycle (manual)
        if (Input.GetKeyDown(tabKey))
            TryTabTarget();

        // 2) Nếu đang manual -> giữ cứng cho tới khi mất điều kiện
        if (mode == TargetMode.Manual)
            return;

        // 3) Soft target (auto highlight) nếu bật
        if (enableSoftTarget)
            UpdateSoftTarget();
    }

    // =========================================================
    // VALIDATION (CỰC QUAN TRỌNG CHO FUSION)
    // =========================================================
    private void ValidateTarget()
    {
        if (CurrentVisualTarget == null)
        {
            if (mode != TargetMode.None) ClearTarget();
            return;
        }

        float dist = Vector2.Distance(transform.position, CurrentVisualTarget.position);
        if (dist > loseTargetDistance)
        {
            ClearTarget();
            return;
        }

        // Enemy chết / despawn -> clear
        if (CurrentEnemy != null)
        {
            // Enemy script của bạn có IsAlive => chuẩn
            if (!CurrentEnemy.IsAlive)
            {
                ClearTarget();
                return;
            }

            // Nếu enemy có NetworkObject bị despawn trên client => Object invalid
            var no = CurrentEnemy.GetComponent<NetworkObject>();
            if (no == null || !no || !no.IsValid)
            {
                ClearTarget();
                return;
            }
        }
        // PLAYER despawn
        if (CurrentPlayer != null)
        {
            var no = CurrentPlayer.GetComponent<NetworkObject>();
            if (no == null || !no || !no.IsValid)
            {
                ClearTarget();
                return;
            }
        }

    }

    // =========================================================
    // SOFT TARGET (AUTO HIGHLIGHT GẦN NHẤT, KHÔNG "CỨNG")
    // =========================================================
    private void UpdateSoftTarget()
    {
        Vector3 pos = transform.position;

        // ưu tiên enemy soft trước
        Enemy nearestEnemy = GetNearestEnemy(pos);
        if (nearestEnemy != null)
        {
            float d = Vector2.Distance(pos, nearestEnemy.transform.position);

            // hysteresis: chỉ đổi soft khi cái mới gần hơn đủ nhiều
            if (CurrentVisualTarget == null || mode == TargetMode.None)
            {
                SetSoftEnemy(nearestEnemy);
                lastSoftDist = d;
                lastSoftCandidate = nearestEnemy.transform;
                return;
            }

            if (lastSoftCandidate != null)
            {
                float cur = Vector2.Distance(pos, lastSoftCandidate.position);
                if (d + softTargetHysteresis < cur)
                {
                    SetSoftEnemy(nearestEnemy);
                    lastSoftDist = d;
                    lastSoftCandidate = nearestEnemy.transform;
                }
            }
            return;
        }

        // nếu không có enemy -> soft player
        PlayerInfo nearestPlayer = GetNearestPlayer(pos);
        if (nearestPlayer != null)
        {
            SetSoftPlayer(nearestPlayer);
            return;
        }

        // nếu không có player -> soft npc
        Transform nearestNpc = GetNearestNPC(pos);
        if (nearestNpc != null)
        {
            SetSoftVisual(nearestNpc);
            return;
        }

        // không có gì -> clear
        ClearTarget();
    }

    // =========================================================
    // TAB TARGET (MANUAL)
    // =========================================================
    private void TryTabTarget()
    {
        if (Time.time - lastTabTime < tabCooldown) return;
        lastTabTime = Time.time;

        var candidates = CollectTabCandidates();

        if (candidates.Count == 0)
        {
            ClearTarget();
            return;
        }

        // tìm index hiện tại
        int currentIndex = -1;
        if (CurrentVisualTarget != null)
        {
            for (int i = 0; i < candidates.Count; i++)
            {
                if (candidates[i].t == CurrentVisualTarget)
                {
                    currentIndex = i;
                    break;
                }
            }
        }

        int nextIndex = (currentIndex + 1) % candidates.Count;
        var next = candidates[nextIndex];

        // set manual
        if (next.enemy != null) SetManualEnemy(next.enemy);
        else if (next.player != null) SetManualPlayer(next.player);
        else SetManualVisual(next.t);
    }

    private List<TabCandidate> CollectTabCandidates()
    {
        Vector3 pos = transform.position;
        var list = new List<TabCandidate>(32);
        var localPlayer = GetComponent<NetworkObject>();

        // 1) enemy
        var enemies = Physics2D.OverlapCircleAll(pos, enemyRadius, enemyLayer);
        foreach (var col in enemies)
        {
            var e = col.GetComponent<Enemy>();
            if (e == null || !e.IsAlive) continue;

            if (requireLineOfSight && !HasLOS(e.transform)) continue;

            list.Add(new TabCandidate
            {
                t = e.transform,
                enemy = e,
                player = null
            });
        }

        // 2) player (tuỳ chọn)
        if (includePlayerInTab)
        {
            var players = Physics2D.OverlapCircleAll(pos, playerRadius, playerLayer);
            foreach (var col in players)
            {
                var playerInfo = col.GetComponent<PlayerInfo>();
                if (playerInfo == null) continue;

                // Không target chính mình
                if (localPlayer != null && localPlayer.HasInputAuthority)
                {
                    var playerNetObj = playerInfo.GetComponent<NetworkObject>();
                    if (playerNetObj != null && playerNetObj.HasInputAuthority)
                        continue;
                }

                if (requireLineOfSight && !HasLOS(playerInfo.transform)) continue;

                list.Add(new TabCandidate
                {
                    t = playerInfo.transform,
                    enemy = null,
                    player = playerInfo
                });
            }
        }

        // 3) npc (tuỳ chọn)
        if (includeNPCInTab)
        {
            var npcs = Physics2D.OverlapCircleAll(pos, npcRadius, npcLayer);
            foreach (var col in npcs)
            {
                var t = col.transform;
                if (t == null) continue;
                if (requireLineOfSight && !HasLOS(t)) continue;

                list.Add(new TabCandidate
                {
                    t = t,
                    enemy = null,
                    player = null
                });
            }
        }

        // sort: MMO thường sort theo "góc phía trước" rồi distance
        list.Sort((a, b) =>
        {
            float sa = ScoreCandidate(a.t);
            float sb = ScoreCandidate(b.t);
            return sa.CompareTo(sb);
        });

        return list;
    }

    private struct TabCandidate
    {
        public Transform t;
        public Enemy enemy;
        public PlayerInfo player;
    }

    // Score càng nhỏ càng ưu tiên
    // Ưu tiên mục tiêu ở “phía trước” (theo hướng nhìn/di chuyển), rồi tới khoảng cách
    private float ScoreCandidate(Transform t)
    {
        Vector2 to = (t.position - transform.position);
        float dist = to.magnitude;

        // hướng “forward” 2D: nếu bạn có script movement thì dùng facing direction,
        // ở đây fallback: Vector2.right (có thể sửa theo game bạn)
        Vector2 forward = Vector2.right;

        float angle = Vector2.Angle(forward, to.normalized); // 0..180
        // ưu tiên nhỏ angle, rồi dist
        return angle * 1000f + dist;
    }

    private bool HasLOS(Transform target)
    {
        // LOS đơn giản: raycast không chạm vật cản
        // bạn có thể thêm obstacleLayer nếu cần
        return true;
    }

    // =========================================================
    // API CHO CLICK / UI
    // =========================================================
    public void SetManualEnemy(Enemy enemy)
    {
        if (enemy == null) { ClearTarget(); return; }
        CurrentEnemy = enemy;
        mode = TargetMode.Manual;
        SetVisual(enemy.transform);
    }

    public void SetManualVisual(Transform t)
    {
        if (t == null) { ClearTarget(); return; }
        CurrentEnemy = null;
        mode = TargetMode.Manual;
        SetVisual(t);
    }

    public void ClearTarget()
    {
        CurrentEnemy = null;
        CurrentPlayer = null;
        CurrentVisualTarget = null;
        mode = TargetMode.None;

        if (indicatorInstance != null)
            indicatorInstance.SetTarget(null);

        if (targetInfoPanel != null)
            targetInfoPanel.Hide();
    }



    // =========================================================
    // INTERNAL SETTERS
    // =========================================================
    private void SetSoftEnemy(Enemy enemy)
    {
        CurrentEnemy = enemy;
        mode = TargetMode.Soft;
        SetVisual(enemy.transform);
    }

    private void SetSoftVisual(Transform t)
    {
        CurrentEnemy = null;
        CurrentPlayer = null;
        mode = TargetMode.Soft;
        SetVisual(t);
    }

    private void SetSoftPlayer(PlayerInfo player)
    {
        CurrentEnemy = null;
        CurrentPlayer = player;
        mode = TargetMode.Soft;
        SetVisual(player.transform);
    }

    private void SetVisual(Transform t)
    {
        CurrentVisualTarget = t;

        if (indicatorInstance != null)
            indicatorInstance.SetTarget(t);

        if (targetInfoPanel != null)
        {
            if (t == null)
            {
                targetInfoPanel.Hide();
            }
            else
            {
                // ENEMY
                EnemyInfo e = t.GetComponent<EnemyInfo>();
                if (e != null)
                {
                    targetInfoPanel.ShowEnemy(e);
                    return;
                }

                // NPC
                NpcShopId npc = t.GetComponent<NpcShopId>();
                if (npc != null)
                {
                    targetInfoPanel.ShowNPC(npc);
                    return;
                }

                // PLAYER
                PlayerInfo p = t.GetComponent<PlayerInfo>();
                if (p != null)
                {
                    targetInfoPanel.ShowPlayer(p);
                    return;
                }

                targetInfoPanel.Hide();
            }
        }
    }



    // =========================================================
    // FIND NEAREST
    // =========================================================
    private Enemy GetNearestEnemy(Vector3 from)
    {
        var hits = Physics2D.OverlapCircleAll(from, enemyRadius, enemyLayer);

        Enemy nearest = null;
        float minDist = float.MaxValue;

        foreach (var h in hits)
        {
            var e = h.GetComponent<Enemy>();
            if (e == null || !e.IsAlive) continue;

            float d = Vector2.Distance(from, e.transform.position);
            if (d < minDist)
            {
                minDist = d;
                nearest = e;
            }
        }
        return nearest;
    }

    private Transform GetNearestNPC(Vector3 from)
    {
        var hits = Physics2D.OverlapCircleAll(from, npcRadius, npcLayer);

        Transform nearest = null;
        float minDist = float.MaxValue;

        foreach (var h in hits)
        {
            var t = h.transform;
            if (t == null) continue;

            float d = Vector2.Distance(from, t.position);
            if (d < minDist)
            {
                minDist = d;
                nearest = t;
            }
        }
        return nearest;
    }

    private PlayerInfo GetNearestPlayer(Vector3 from)
    {
        var hits = Physics2D.OverlapCircleAll(from, playerRadius, playerLayer);

        PlayerInfo nearest = null;
        float minDist = float.MaxValue;
        var localPlayer = GetComponent<NetworkObject>();

        foreach (var h in hits)
        {
            var playerInfo = h.GetComponent<PlayerInfo>();
            if (playerInfo == null) continue;

            // Không target chính mình
            if (localPlayer != null && localPlayer.HasInputAuthority)
            {
                var playerNetObj = playerInfo.GetComponent<NetworkObject>();
                if (playerNetObj != null && playerNetObj.HasInputAuthority)
                    continue;
            }

            float d = Vector2.Distance(from, playerInfo.transform.position);
            if (d < minDist)
            {
                minDist = d;
                nearest = playerInfo;
            }
        }
        return nearest;
    }
    public void SetManualPlayer(PlayerInfo player)
    {
        if (player == null)
        {
            ClearTarget();
            return;
        }

        CurrentEnemy = null;
        CurrentPlayer = player;
        mode = TargetMode.Manual;
        SetVisual(player.transform);
    }

#if UNITY_EDITOR
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, enemyRadius);

        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.position, npcRadius);

        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, loseTargetDistance);
    }
#endif
}
