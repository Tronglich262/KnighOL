using UnityEngine;
using Fusion;

public class NetworkBuffFollow : NetworkBehaviour
{
    [Networked] private NetworkId TargetId { get; set; }
    [Networked] private Vector3 Offset { get; set; }   // 👈 offset sync qua network

    private Transform target;

    public void SetTarget(NetworkObject targetObj, Vector3 offset)
    {
        if (Object.HasStateAuthority)
        {
            TargetId = targetObj.Id;
            Offset = offset;
        }

        target = targetObj.transform;
    }

    public override void Render()
    {
        if (target == null && TargetId.IsValid)
        {
            if (Runner.TryFindObject(TargetId, out NetworkObject obj))
            {
                target = obj.transform;
            }
        }

        if (target != null)
        {
            transform.position = target.position + Offset;
        }
    }
}