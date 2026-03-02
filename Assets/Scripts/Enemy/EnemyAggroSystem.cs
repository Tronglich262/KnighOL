using Fusion;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Hệ thống aggro của enemy - quản lý threat/omen của các player
/// </summary>
public class EnemyAggroSystem : NetworkBehaviour
{
    private Dictionary<PlayerRef, float> threat = new();
    public Transform CurrentTarget { get; private set; }

    /// <summary>
    /// Thêm threat cho một player
    /// </summary>
    public void AddThreat(PlayerRef player, float value, Transform t)
    {
        if (!HasStateAuthority) return;

        if (!threat.ContainsKey(player))
            threat[player] = 0;

        threat[player] += value;
        RecalculateTarget(t);
    }

    /// <summary>
    /// Tính toán lại target dựa trên threat cao nhất
    /// </summary>
    void RecalculateTarget(Transform fallback)
    {
        float max = -1;
        PlayerRef best = default;

        foreach (var kv in threat)
        {
            if (kv.Value > max)
            {
                max = kv.Value;
                best = kv.Key;
            }
        }

        CurrentTarget = fallback;
    }

    /// <summary>
    /// Xóa tất cả threat
    /// </summary>
    public void Clear()
    {
        threat.Clear();
        CurrentTarget = null;
    }
}
