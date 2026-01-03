using UnityEngine;

public class TargetingSystem : MonoBehaviour
{
    public float searchRadius = 12f;
    public LayerMask enemyLayer;

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
}
