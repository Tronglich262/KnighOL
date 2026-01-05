using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class TargetInfoPanel : MonoBehaviour
{
    [Header("UI")]
    public Image targetIcon;
    public TextMeshProUGUI targetName;
    public TextMeshProUGUI targetHpText; // ❤️ HP text

    [Header("Default")]
    public Sprite defaultEnemyIcon;
    public Sprite defaultNpcIcon;

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

    private void Update()
    {
        // realtime update HP text
        if (currentEnemyHp != null)
        {
            targetHpText.text =
                $"HP: {currentEnemyHp.CurrentHealth} / {currentEnemyHp.MaxHealth}";
        }
    }

    // =========================
    // ENEMY
    // =========================
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
            targetHpText.gameObject.SetActive(true);
            targetHpText.text =
                $"HP: {currentEnemyHp.CurrentHealth} / {currentEnemyHp.MaxHealth}";
        }
        else
        {
            targetHpText.gameObject.SetActive(false);
        }
    }

    // =========================
    // NPC
    // =========================
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

        // ❌ NPC không có HP
        currentEnemyHp = null;
        targetHpText.gameObject.SetActive(false);
    }

    // =========================
    // CLEAR
    // =========================
    public void Hide()
    {
        currentEnemyHp = null;
        gameObject.SetActive(false);
    }
}
