using Fusion;
using UnityEngine;

public class ClickToTarget2D : MonoBehaviour
{
    private Enemy enemy;

    private void Awake()
    {
        enemy = GetComponent<Enemy>();
    }

    private void OnMouseDown()
    {
        // tìm local player
        foreach (var p in GameObject.FindGameObjectsWithTag("Player"))
        {
            var no = p.GetComponent<NetworkObject>();
            if (no != null && no.HasInputAuthority)
            {
                var ts = p.GetComponent<TargetingSystem>();
                if (ts == null) return;

                if (enemy != null) ts.SetManualEnemy(enemy);
                else ts.SetManualVisual(transform);

                break;
            }
        }
    }
}
