using Fusion;
using System.Collections.Generic;
using UnityEngine;

public class EnemyAggroSystem : NetworkBehaviour
{
    private Dictionary<PlayerRef, float> threat = new();
    public Transform CurrentTarget { get; private set; }

    public void AddThreat(PlayerRef player, float value, Transform t)
    {
        if (!HasStateAuthority) return;

        if (!threat.ContainsKey(player))
            threat[player] = 0;

        threat[player] += value;
        RecalculateTarget(t);
    }

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

    public void Clear()
    {
        threat.Clear();
        CurrentTarget = null;
    }
}
