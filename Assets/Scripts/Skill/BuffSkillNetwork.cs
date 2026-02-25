using UnityEngine;
using Fusion;

public class BuffSkillNetwork : NetworkBehaviour
{
    private const int BUFF_COUNT = 6;
    private const int ATTACK_COUNT = 6;
    private const int TOTAL_SKILL_COUNT = 12;
    public float[] skillCooldownTimes =
{
    10f, 10f, 10f, 10f, 10f, 10f, // 6 buff
    5f, 10f, 5f,10f,5f,10f                      // 6 attack
};
    // ================= NETWORK DATA =================
    private NetworkObject[] pendingTargets = new NetworkObject[ATTACK_COUNT];
    [Networked, Capacity(TOTAL_SKILL_COUNT)]
    public NetworkArray<TickTimer> BuffTimers => default;

    [Networked, Capacity(TOTAL_SKILL_COUNT)]
    public NetworkArray<TickTimer> Cooldowns => default;

    [Networked, Capacity(TOTAL_SKILL_COUNT)]
    public NetworkArray<NetworkBool> IsActive => default;

    [Networked, Capacity(TOTAL_SKILL_COUNT)]
    public NetworkArray<NetworkBool> IsCasting => default;

    // ================= PREFABS =================

    public NetworkPrefabRef[] buffEffectPrefabs = new NetworkPrefabRef[BUFF_COUNT];
    public NetworkPrefabRef[] castEffectPrefabs = new NetworkPrefabRef[ATTACK_COUNT];
    public NetworkPrefabRef[] hitEffectPrefabs = new NetworkPrefabRef[ATTACK_COUNT];

    private NetworkObject[] activeBuffEffects = new NetworkObject[BUFF_COUNT];
    private NetworkObject[] activeCastEffects = new NetworkObject[ATTACK_COUNT];

    // ================= PUBLIC CALL =================

    public void TryUseBuff(int skillIndex)
    {
        if (!HasInputAuthority) return;
        RPC_RequestUseBuff(skillIndex);
    }

    public void TryUseAttack(int skillIndex)
    {
        if (!HasInputAuthority) return;

        var ts = GetComponent<TargetingSystem>();
        NetworkObject targetNO = null;

        if (ts != null && ts.CurrentVisualTarget != null)
        {
            targetNO = ts.CurrentVisualTarget.GetComponent<NetworkObject>();
        }

        RPC_RequestUseAttack(skillIndex, targetNO);
    }

    // ================= BUFF RPC =================

    [Rpc(RpcSources.InputAuthority, RpcTargets.StateAuthority)]
    void RPC_RequestUseBuff(int skillIndex)
    {
        if (skillIndex < 0 || skillIndex >= BUFF_COUNT)
            return;

        if (Cooldowns[skillIndex].RemainingTime(Runner) > 0)
            return;

        if (IsActive[skillIndex])
            return;

        ActivateBuff(skillIndex);
    }

    void ActivateBuff(int index)
    {
        IsActive.Set(index, true);

        BuffTimers.Set(index,
            TickTimer.CreateFromSeconds(Runner, 5f));

        Cooldowns.Set(index,
            TickTimer.CreateFromSeconds(Runner, 10f));

        if (buffEffectPrefabs[index].IsValid)
        {
            var obj = Runner.Spawn(
                buffEffectPrefabs[index],
                transform.position,
                Quaternion.identity,
                Object.InputAuthority);

            var follow = obj.GetComponent<NetworkBuffFollow>();
            if (follow != null)
            {
                follow.SetTarget(Object, Vector3.zero);
            }

            activeBuffEffects[index] = obj;
        }
    }

    // ================= ATTACK RPC =================

    [Rpc(RpcSources.InputAuthority, RpcTargets.StateAuthority)]
    void RPC_RequestUseAttack(int skillIndex, NetworkObject targetNO)
    {
        if (skillIndex < BUFF_COUNT || skillIndex >= BUFF_COUNT + ATTACK_COUNT)
            return;

        if (Cooldowns[skillIndex].RemainingTime(Runner) > 0)
            return;

        if (IsCasting[skillIndex])
            return;

        StartCast(skillIndex, targetNO);
    }

    void StartCast(int skillIndex, NetworkObject targetNO)
    {
        int attackIndex = skillIndex - BUFF_COUNT;

        IsCasting.Set(skillIndex, true);

        BuffTimers.Set(skillIndex,
            TickTimer.CreateFromSeconds(Runner, 1f));

        // Lưu target theo từng skill
        pendingTargets[attackIndex] = targetNO;

        // ===== GIỮ LOGIC CAST CŨ =====
        Vector3 castPos = transform.position + Vector3.up * 2f;

        if (castEffectPrefabs[attackIndex].IsValid)
        {
            var obj = Runner.Spawn(
                castEffectPrefabs[attackIndex],
                castPos,
                Quaternion.identity,
                Object.InputAuthority);

            activeCastEffects[attackIndex] = obj;
        }
    }
    void ExecuteAttack(int skillIndex)
    {
        int attackIndex = skillIndex - BUFF_COUNT;

        Vector3 hitPos;

        NetworkObject targetNO = pendingTargets[attackIndex];

        if (targetNO != null && targetNO.IsValid)
        {
            hitPos = targetNO.transform.position + Vector3.up * 1.5f;
        }
        else
        {
            // fallback nếu target chết
            hitPos = transform.position + transform.forward * 2f + Vector3.up * 3f;
        }

        if (hitEffectPrefabs[attackIndex].IsValid)
        {
            Runner.Spawn(
                hitEffectPrefabs[attackIndex],
                hitPos,
                Quaternion.identity,
                Object.InputAuthority);
        }

        Cooldowns.Set(skillIndex,
            TickTimer.CreateFromSeconds(Runner, skillCooldownTimes[skillIndex]));

        IsCasting.Set(skillIndex, false);

        pendingTargets[attackIndex] = null;
    }
    // ================= NETWORK LOOP =================

    public override void FixedUpdateNetwork()
    {
        for (int i = 0; i < TOTAL_SKILL_COUNT; i++)
        {
            // ===== BUFF EXPIRE =====
            if (i < BUFF_COUNT &&
                IsActive[i] &&
                BuffTimers[i].Expired(Runner))
            {
                IsActive.Set(i, false);

                if (activeBuffEffects[i] != null)
                {
                    Runner.Despawn(activeBuffEffects[i]);
                    activeBuffEffects[i] = null;
                }
            }

            // ===== ATTACK CAST FINISH =====
            if (i >= 6 &&
                IsCasting[i] &&
                BuffTimers[i].Expired(Runner))
            {
                int attackIndex = i - 6;

                if (activeCastEffects[attackIndex] != null)
                {
                    Runner.Despawn(activeCastEffects[attackIndex]);
                    activeCastEffects[attackIndex] = null;
                }

                ExecuteAttack(i);
            }
        }
    }

    // ================= HELPER =================

    Vector3 GetGroundPointInFront(float distance)
    {
        Vector3 forwardPoint = transform.position + transform.forward * distance;

        Ray ray = new Ray(forwardPoint + Vector3.up * 5f, Vector3.down);
        RaycastHit hit;

        if (Physics.Raycast(ray, out hit, 20f))
        {
            return hit.point;
        }

        return forwardPoint;
    }
    Vector3 GetCastPosition(float distance, float heightOffset)
    {
        Vector3 forwardPoint = transform.position + transform.forward * distance;

        Ray ray = new Ray(forwardPoint + Vector3.up * 5f, Vector3.down);
        RaycastHit hit;

        Vector3 groundPoint = forwardPoint;

        if (Physics.Raycast(ray, out hit, 20f))
        {
            groundPoint = hit.point;
        }

        // 👇 thêm độ cao tùy chỉnh
        groundPoint.y += heightOffset;

        return groundPoint;
    }
}