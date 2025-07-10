using UnityEngine;

public class FollowPreviewTarget : MonoBehaviour
{
    public Transform target;
    public Vector3 offset = new Vector3(0, 1, -3);

    void LateUpdate()
    {
        if (target != null)
        {
            transform.position = target.position + offset;
            transform.LookAt(target); // đảm bảo nhìn vào nhân vật
        }
    }
}
