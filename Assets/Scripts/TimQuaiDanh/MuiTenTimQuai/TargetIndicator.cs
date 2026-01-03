using UnityEngine;

public class TargetIndicator : MonoBehaviour
{
    public Vector3 offset = new Vector3(0, 1.5f, 0);
    private Transform target;

    public void SetTarget(Transform t)
    {
        target = t;
        gameObject.SetActive(t != null);
    }

    private void LateUpdate()
    {
        if (target == null)
        {
            gameObject.SetActive(false);
            return;
        }

        transform.position = target.position + offset;
    }
}
