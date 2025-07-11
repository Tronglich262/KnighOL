using Fusion;
using UnityEngine;

public class PlayerLevelManager : NetworkBehaviour
{
    [Networked] public int Level { get; set; }
    [Networked] public int Exp { get; set; }
    [Networked] public int ExpToNextLevel { get; set; }

    // Tùy bạn: có thể thêm các event khi lên level
    public System.Action<int> OnLevelUp;

    public override void Spawned()
    {
        if (HasStateAuthority)
        {
            Level = 1;
            Exp = 0;
            ExpToNextLevel = GetExpForLevel(Level);
        }
    }

    public void AddExp(int amount)
    {
        if (!HasStateAuthority) return;

        Exp += amount;

        while (Exp >= ExpToNextLevel)
        {
            Exp -= ExpToNextLevel;
            Level++;
            ExpToNextLevel = GetExpForLevel(Level);

            Debug.Log($"LEVEL UP! New Level: {Level}");
            OnLevelUp?.Invoke(Level); // Gọi event nếu có
            // Tăng chỉ số, hiệu ứng, mở skill mới ... tại đây nếu muốn
        }
    }

    public int GetExpForLevel(int lv)
    {
        // Tùy bạn muốn tăng EXP thế nào, đây là ví dụ dễ chỉnh sửa
        return 100 + (lv - 1) * 50; // Level 1 cần 100 exp, lên 2 cần 150 exp, v.v.
        // return Mathf.RoundToInt(100 * Mathf.Pow(1.15f, lv-1)); // Hoặc hàm tăng lũy tiến
    }
}
