using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Panel hien thi thong tin cua target (enemy, NPC, player khac)
/// </summary>
public class TargetInfoPanel : MonoBehaviour
{
    [Header("UI")]
    public Image targetIcon;
    public TextMeshProUGUI targetName;
    public TextMeshProUGUI targetHpText;

    [Header("Default")]
    public Sprite defaultEnemyIcon;
    public Sprite defaultNpcIcon;
    [Header("Default")]
    public Sprite defaultPlayerIcon;

    public static TargetInfoPanel Instance { get; private set; }

    private EnemyDamageHandler currentEnemyHp;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        Hide();
    }

   

    /// <summary>
    /// Hien thi thong tin enemy
    /// </summary>
    public void ShowEnemy(EnemyInfo enemyInfo)
    {
        if (enemyInfo == null)
        {
            Hide();
            return;
        }

        gameObject.SetActive(true);

        targetName.text = enemyInfo.EnemyName;

        if (enemyInfo.EnemyIcon != null)
            targetIcon.sprite = enemyInfo.EnemyIcon;
        else
            targetIcon.sprite = defaultEnemyIcon;

        // bind HP
        currentEnemyHp = enemyInfo.GetComponent<EnemyDamageHandler>();

        if (currentEnemyHp != null)
        {
            var stats = currentEnemyHp.GetComponent<EnemyStats>();
            if (stats != null)
            {
                targetHpText.gameObject.SetActive(true);
                targetHpText.text = $"HP: {stats.HP} / {stats.MaxHP}";
            }
        }
        else
        {
            targetHpText.gameObject.SetActive(false);
        }
    }

    /// <summary>
    /// Cap nhat HP khi enemy nhan damage
    /// </summary>
    public void NotifyHPChanged(EnemyStats stats)
    {
        if (currentEnemyHp == null) return;

        var curStats = currentEnemyHp.GetComponent<EnemyStats>();
        if (curStats != stats) return;

        targetHpText.gameObject.SetActive(true);
        targetHpText.text = $"HP: {stats.HP} / {stats.MaxHP}";
    }

    // =========================
    // NPC
    // =========================

    /// <summary>
    /// Hien thi thong tin NPC
    /// </summary>
    public void ShowNPC(NpcShopId npc)
    {
        if (npc == null)
        {
            Hide();
            return;
        }

        gameObject.SetActive(true);

        targetName.text = npc.npcShopName;

        if (npc.npcShopIcon != null)
            targetIcon.sprite = npc.npcShopIcon;
        else
            targetIcon.sprite = defaultNpcIcon;

        // NPC không có HP
        currentEnemyHp = null;
        targetHpText.gameObject.SetActive(false);
    }

    // =========================
    // CLEAR
    // =========================

    /// <summary>
    /// An panel
    /// </summary>
    public void Hide()
    {
        currentEnemyHp = null;
        gameObject.SetActive(false);
    }

    /// <summary>
    /// Hien thi thong tin player khac
    /// </summary>
    public void ShowPlayer(PlayerInfo player)
    {
        if (player == null)
        {
            Hide();
            return;
        }

        gameObject.SetActive(true);

        // Lay ten tu NameTagManager
        targetName.text = player.PlayerName;

        if (player.playerIcon != null)
            targetIcon.sprite = player.playerIcon;
        else
            targetIcon.sprite = defaultPlayerIcon;

        // Player khong co HP kieu enemy
        currentEnemyHp = null;
        targetHpText.gameObject.SetActive(false);
    }
}
