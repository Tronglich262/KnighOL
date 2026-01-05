using UnityEngine;

public class TargetIndicator : MonoBehaviour
{
    public Vector3 offset = new Vector3(0, 3.3f, 0);
    private Transform target;

    public void SetTarget(Transform t)
    {
        if (target == t)
            return;

        target = t;

        if (t == null)
        {
            if (gameObject.activeSelf)
                gameObject.SetActive(false);
        }
        else
        {
            if (!gameObject.activeSelf)
                gameObject.SetActive(true);
        }
    }

    private void LateUpdate()
    {
        if (target == null) return;
        transform.position = target.position + offset;
    }
}
