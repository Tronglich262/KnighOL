using Assets.HeroEditor.Common.ExampleScripts;
using Fusion;
using UnityEngine;

public class PlayerMapState : NetworkBehaviour
{
    [Networked] public int CurrentMapRaw { get; set; }

    // 🔒 khoá cứng input + movement
    [Networked] public NetworkBool FreezeMovement { get; set; }

    // spawn pending
    [Networked] public Vector3 PendingTeleportPos { get; set; }
    [Networked] public Quaternion PendingTeleportRot { get; set; }

    // ⏱ unlock theo tick (Fusion cũ hỗ trợ)
    [Networked] public TickTimer FreezeTimer { get; set; }

    private MovementExample movement;
    private CharacterController controller;

    public override void Spawned()
    {
        movement = GetComponent<MovementExample>();
        controller = GetComponent<CharacterController>();

        if (HasStateAuthority && CurrentMapRaw == 0)
            CurrentMapRaw = (int)MapId.Town;
    }

    // =========================
    // RPC CHANGE MAP
    // =========================
    [Rpc(RpcSources.InputAuthority, RpcTargets.StateAuthority)]
    public void RPC_ChangeMap(MapId newMap, Vector3 spawnPos, Quaternion spawnRot)
    {
        if (FreezeMovement) return;

        FreezeMovement = true;
        CurrentMapRaw = (int)newMap;

        PendingTeleportPos = spawnPos;
        PendingTeleportRot = spawnRot;

        // 🔒 khoá 0.3s (~6 tick)
        FreezeTimer = TickTimer.CreateFromSeconds(Runner, 0.3f);

        movement?.ForceStop();
    }

    public override void FixedUpdateNetwork()
    {
        if (!HasStateAuthority) return;

        // TELEPORT 1 LAN
        if (FreezeMovement)
        {
            if (controller != null)
                controller.enabled = false;

            transform.SetPositionAndRotation(
                PendingTeleportPos,
                PendingTeleportRot
            );

            if (controller != null)
                controller.enabled = true;

            movement?.ForceStop();
        }

        // ⏱ mở khoá SAU khi timer hết
        if (FreezeMovement && FreezeTimer.Expired(Runner))
        {
            FreezeMovement = false;
        }
    }
}
