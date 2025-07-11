using UnityEngine;
using TMPro;
using Fusion;

public class PlayerLevelUI : MonoBehaviour
{
    public TextMeshProUGUI levelText;
    public PlayerLevelManager playerLevel;

    void Start()
    {
        StartCoroutine(FindAndAssignLocalPlayerLevelManager());
    }

    System.Collections.IEnumerator FindAndAssignLocalPlayerLevelManager()
    {
        while (playerLevel == null)
        {
            // Tìm tất cả PlayerLevelManager trong scene
            var allPlayers = FindObjectsOfType<PlayerLevelManager>();

            // Lọc ra player nào là local (HasInputAuthority)
            foreach (var p in allPlayers)
            {
                // Có thể phải check thêm IsSpawned nếu game đang loading
                if (p.HasInputAuthority)
                {
                    playerLevel = p;
                    break;
                }
            }

            // Nếu chưa tìm thấy, đợi frame sau tìm tiếp
            if (playerLevel == null)
                yield return null;
        }
    }

    void Update()
    {
        if (playerLevel == null) return;
        if (playerLevel.Object == null || !playerLevel.Object.IsValid) return; // BẮT BUỘC!

        float expPercent = playerLevel.ExpToNextLevel > 0
            ? (float)playerLevel.Exp / playerLevel.ExpToNextLevel * 100f
            : 0;

        levelText.text = $"Lv:   {playerLevel.Level} {expPercent:F1}%";
    }
}

