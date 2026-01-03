using UnityEngine;

public class TargetingSystem : MonoBehaviour
{
    public float searchRadius = 12f;
    public LayerMask enemyLayer;

    [Header("Indicator (Prefab)")]
    public TargetIndicator indicatorPrefab;
    private TargetIndicator indicatorInstance;

    public Enemy CurrentTarget { get; private set; }

    private void Start()
    {
        // ⭐ Spawn mũi tên local-only (KHÔNG NetworkObject)
        if (indicatorPrefab != null)
        {
            indicatorInstance = Instantiate(indicatorPrefab);
            indicatorInstance.gameObject.SetActive(false);
        }
        else
        {
            Debug.LogError("TargetingSystem: Chưa gán indicatorPrefab!");
        }
    }

    public Enemy GetNearestEnemy(Vector3 from)
    {
        Collider[] hits = Physics.OverlapSphere(from, searchRadius, enemyLayer);

        Enemy nearest = null;
        float minDist = float.MaxValue;

        foreach (var h in hits)
        {
            Enemy e = h.GetComponent<Enemy>();
            if (e == null || !e.IsAlive) continue;

            float d = Vector3.Distance(from, e.transform.position);
            if (d < minDist)
            {
                minDist = d;
                nearest = e;
            }
        }
        return nearest;
    }

    public void SetTarget(Enemy enemy)
    {
        CurrentTarget = enemy;

        if (indicatorInstance != null)
        {
            indicatorInstance.SetTarget(enemy != null ? enemy.transform : null);
        }
    }

    public void ClearTarget()
    {
        CurrentTarget = null;
        if (indicatorInstance != null) indicatorInstance.SetTarget(null);
    }
}
