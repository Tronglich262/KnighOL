using Fusion;
using UnityEngine;

public class AssignEnemyAuthorityAtStart : NetworkBehaviour
{
    public override void Spawned()
    {
        if (!HasStateAuthority && Runner.GameMode == GameMode.Shared)
        {
            Object.AssignInputAuthority(Runner.LocalPlayer);
            Runner.SetPlayerAlwaysInterested(Runner.LocalPlayer, Object, true);
        }
    }
}
