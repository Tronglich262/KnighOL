using Fusion;
using UnityEngine;

public class EnemyClickable : MonoBehaviour
{
    private Enemy enemy;

    private void Awake()
    {
        enemy = GetComponent<Enemy>();
    }

    private void OnMouseDown()
    {
        if (enemy == null) return;

        // tìm player local (HasInputAuthority)
        foreach (var p in GameObject.FindGameObjectsWithTag("Player"))
        {
            var net = p.GetComponent<NetworkObject>();
            if (net != null && net.HasInputAuthority)
            {
                var targeting = p.GetComponent<TargetingSystem>();
                if (targeting != null)
                {
                    targeting.SetTarget(enemy);
                    Debug.Log("Selected enemy: " + enemy.name);
                }
                break;
            }
        }
    }
}
