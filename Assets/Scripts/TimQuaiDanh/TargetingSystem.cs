using Fusion;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Hệ thống targeting cho game MMO - quản lý việc chọn mục tiêu (enemy, NPC, player khác)
/// </summary>
public class TargetingSystem : MonoBehaviour
{
    [Header("Layers (2D)")]
    public LayerMask enemyLayer;
    public LayerMask npcLayer;
    public LayerMask playerLayer;

    [Header("Ranges")]
    public float enemyRadius = 12f;
    public float npcRadius = 8f;
    public float playerRadius = 15f;
    public float loseTargetDistance = 15f;

    [Header("Tab Target")]
    public KeyCode tabKey = KeyCode.Tab;
    public bool includeNPCInTab = false;
    public bool includePlayerInTab = true;
    public bool requireLineOfSight = false;
    public float tabCooldown = 0.10f;

    [Header("Soft Target")]
    public bool enableSoftTarget = true;
    public float softTargetHysteresis = 1.5f;

    [Header("Indicator")]
    public TargetIndicator indicatorPrefab;
    private TargetIndicator indicatorInstance;

    [Header("UI")]
    public TargetInfoPanel targetInfoPanel;

    // ===== Runtime =====
    public Enemy CurrentEnemy { get; private set; }
    public Transform CurrentVisualTarget { get; private set; }
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

        // Chi cho phep local player
        if (netObj == null || !netObj.HasInputAuthority)
        {
            enabled = false;
            return;
        }

        indicatorInstance = Instantiate(indicatorPrefab);
        indicatorInstance.SetTarget(null);

        targetInfoPanel = TargetInfoPanel.Instance;
    }

    private void Update()
    {
        // Kiem tra target hien tai con hop le khong
        ValidateTarget();

        // TAB cycle (manual target)
        if (Input.GetKeyDown(tabKey))
            TryTabTarget();

        // Neu dang manual -> giu cung cho den khi mat dieu kien
        if (mode == TargetMode.Manual)
            return;

        // Soft target (auto highlight) neu bat
        if (enableSoftTarget)
            UpdateSoftTarget();
    }

    /// <summary>
    /// Kiem tra target hien tai con hop le khong (bi despawn, chet, hay qua xa)
    /// </summary>
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

        // Enemy chet / despawn -> clear
        if (CurrentEnemy != null)
        {
            if (!CurrentEnemy.IsAlive)
            {
                ClearTarget();
                return;
            }

            // Neu enemy co NetworkObject bi despawn tren client => Object invalid
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

    /// <summary>
    /// Cap nhat soft target - tu dong highlight gan nhat nhung khong "cung"
    /// Uu tien: Enemy -> Player -> NPC
    /// </summary>
    private void UpdateSoftTarget()
    {
        Vector3 pos = transform.position;

        // Uu tien enemy soft truoc
        Enemy nearestEnemy = GetNearestEnemy(pos);
        if (nearestEnemy != null)
        {
            float d = Vector2.Distance(pos, nearestEnemy.transform.position);

            // Hysteresis: chi doi soft khi cai moi gan hon du nhieu
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

        // Neu khong co enemy -> soft player
        PlayerInfo nearestPlayer = GetNearestPlayer(pos);
        if (nearestPlayer != null)
        {
            SetSoftPlayer(nearestPlayer);
            return;
        }

        // Neu khong co player -> soft npc
        Transform nearestNpc = GetNearestNPC(pos);
        if (nearestNpc != null)
        {
            SetSoftVisual(nearestNpc);
            return;
        }

        // Khong co gi -> clear
        ClearTarget();
    }

    /// <summary>
    /// Xu ly Tab target - chon manual target tiep theo trong danh sach
    /// </summary>
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

        // Tim index hien tai
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

        // Set manual
        if (next.enemy != null) SetManualEnemy(next.enemy);
        else if (next.player != null) SetManualPlayer(next.player);
        else SetManualVisual(next.t);
    }

    /// <summary>
    /// Thu thap danh sach cac target kha dung cho Tab
    /// </summary>
    private List<TabCandidate> CollectTabCandidates()
    {
        Vector3 pos = transform.position;
        var list = new List<TabCandidate>(32);
        var localPlayer = GetComponent<NetworkObject>();

        // 1) Enemy
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

        // 2) Player (tuy chon)
        if (includePlayerInTab)
        {
            var players = Physics2D.OverlapCircleAll(pos, playerRadius, playerLayer);
            foreach (var col in players)
            {
                var playerInfo = col.GetComponent<PlayerInfo>();
                if (playerInfo == null) continue;

                // Khong target chinh minh
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

        // 3) NPC (tuy chon)
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

        // Sort: uu tien theo goc phia truoc roi khoang cach
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

    /// <summary>
    /// Tinh diem uu tien cho target - diem cang nhom uu tien cang cao
    /// Uu tien muc tieu o "phia truoc" (theo huong nhin/di chuyen), roi toi khoang cach
    /// </summary>
    private float ScoreCandidate(Transform t)
    {
        Vector2 to = (t.position - transform.position);
        float dist = to.magnitude;

        // Huong "forward" 2D: mac dinh Vector2.right
        Vector2 forward = Vector2.right;

        float angle = Vector2.Angle(forward, to.normalized);
        // Uu tien nho angle, roi dist
        return angle * 1000f + dist;
    }

    /// <summary>
    /// Kiem tra line of sight (tam nhin) den target
    /// </summary>
    private bool HasLOS(Transform target)
    {
        // LOS don gian: raycast khong cham vat can
        return true;
    }

    // =========================================================
    // API CHO CLICK / UI
    // =========================================================

    /// <summary>
    /// Dat manual target la enemy (combat target)
    /// </summary>
    public void SetManualEnemy(Enemy enemy)
    {
        if (enemy == null) { ClearTarget(); return; }
        CurrentEnemy = enemy;
        mode = TargetMode.Manual;
        SetVisual(enemy.transform);
    }

    /// <summary>
    /// Dat manual target la NPC (chi hien thi, khong combat)
    /// </summary>
    public void SetManualVisual(Transform t)
    {
        if (t == null) { ClearTarget(); return; }
        CurrentEnemy = null;
        mode = TargetMode.Manual;
        SetVisual(t);
    }

    /// <summary>
    /// Dat manual target la player khac
    /// </summary>
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

    /// <summary>
    /// Xoa target hien tai
    /// </summary>
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

    /// <summary>
    /// Cap nhat hien thi visual target (indicator + UI)
    /// </summary>
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
                // Enemy
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

                // Player
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

    /// <summary>
    /// Tim enemy gan nhat trong pham vi
    /// </summary>
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

    /// <summary>
    /// Tim NPC gan nhat trong pham vi
    /// </summary>
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

    /// <summary>
    /// Tim player khac gan nhat trong pham vi
    /// </summary>
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

            // Khong target chinh minh
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
