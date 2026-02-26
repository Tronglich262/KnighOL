using Assets.HeroEditor.Common.ExampleScripts;
using Fusion;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class BuffSkillNetwork : NetworkBehaviour
{
    private const int BUFF_COUNT = 6;
    private const int ATTACK_COUNT = 6;
    private const int TOTAL_SKILL_COUNT = 13;
    private const int BASE_INDEX = 12;

    private CharacterStats stats;

    public float[] skillCooldownTimes =
    {
        10f, 10f, 10f, 10f, 10f, 10f,
        5f, 10f, 5f, 10f, 5f, 10f,
        3f
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

    [Networked] public NetworkId CurrentTargetId { get; set; }

    public AOESkillData[] aoeSkills = new AOESkillData[ATTACK_COUNT];

    public NetworkPrefabRef[] buffEffectPrefabs = new NetworkPrefabRef[BUFF_COUNT];
    public NetworkPrefabRef[] castEffectPrefabs = new NetworkPrefabRef[ATTACK_COUNT];
    public NetworkPrefabRef[] hitEffectPrefabs = new NetworkPrefabRef[ATTACK_COUNT];

    private NetworkObject[] activeBuffEffects = new NetworkObject[BUFF_COUNT];
    private NetworkObject[] activeCastEffects = new NetworkObject[ATTACK_COUNT];

    // =================================================

    public override void Spawned()
    {
        stats = GetComponent<CharacterStats>();
    }

    // =================================================
    // PUBLIC CALL
    // =================================================

    public void TryUseBuff(int skillIndex)
    {
        if (!HasInputAuthority) return;
        RPC_RequestUseBuff(skillIndex);
    }

    public void TryUseAttack(int skillIndex)
    {
        if (!HasInputAuthority) return;

        var ts = GetComponent<TargetingSystem>();
        NetworkId targetId = default;

        if (ts != null && ts.CurrentVisualTarget != null)
        {
            var no = ts.CurrentVisualTarget.GetComponent<NetworkObject>();
            if (no != null)
                targetId = no.Id;
        }

        RPC_RequestUseAttack(skillIndex, targetId);
    }

    public void TryUseBaseSkill()
    {
        if (!HasInputAuthority) return;

        var ts = GetComponent<TargetingSystem>();
        if (ts == null || ts.CurrentVisualTarget == null) return;

        var no = ts.CurrentVisualTarget.GetComponent<NetworkObject>();
        if (no == null) return;

        RPC_RequestUseBaseSkill(no.Id);
    }

    // =================================================
    // BUFF
    // =================================================

    [Rpc(RpcSources.InputAuthority, RpcTargets.StateAuthority)]
    void RPC_RequestUseBuff(int skillIndex)
    {
        if (skillIndex < 0 || skillIndex >= BUFF_COUNT) return;
        if (Cooldowns[skillIndex].RemainingTime(Runner) > 0) return;
        if (IsActive[skillIndex]) return;

        ActivateBuff(skillIndex);
    }
    [Rpc(RpcSources.InputAuthority, RpcTargets.StateAuthority)]
    void RPC_SetTarget(NetworkId id)
    {
        CurrentTargetId = id;
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

            activeBuffEffects[index] = obj;
        }
    }

    // =================================================
    // ATTACK
    // =================================================

    [Rpc(RpcSources.InputAuthority, RpcTargets.StateAuthority)]
    void RPC_RequestUseAttack(int skillIndex, NetworkId targetId)
    {
        if (skillIndex < BUFF_COUNT || skillIndex >= BUFF_COUNT + ATTACK_COUNT)
            return;

        if (Cooldowns[skillIndex].RemainingTime(Runner) > 0)
            return;

        if (IsCasting[skillIndex])
            return;

        NetworkObject targetNO = null;
        if (Runner.TryFindObject(targetId, out NetworkObject found))
            targetNO = found;

        StartCast(skillIndex, targetNO);
    }

    void StartCast(int skillIndex, NetworkObject targetNO)
    {
        int attackIndex = skillIndex - BUFF_COUNT;

        IsCasting.Set(skillIndex, true);

        BuffTimers.Set(skillIndex,
            TickTimer.CreateFromSeconds(Runner, 1f));

        pendingTargets[attackIndex] = targetNO;

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
        if (!HasStateAuthority) return;

        int attackIndex = skillIndex - BUFF_COUNT;
        var data = aoeSkills[attackIndex];
        if (data == null) return;

        Vector3 center = pendingTargets[attackIndex] != null
            ? pendingTargets[attackIndex].transform.position
            : transform.position;

        Collider2D[] hits = Physics2D.OverlapCircleAll(
            center,
            data.radius,
            LayerMask.GetMask("Enemy")
        );

        foreach (var hit in hits)
        {
            var enemy = hit.GetComponentInParent<EnemyCore>();
            if (enemy == null) continue;

            int damage = CalculateSkillDamage(data);
            enemy.RPC_RequestHit(damage, Object.InputAuthority);
        }

        if (hitEffectPrefabs[attackIndex].IsValid)
        {
            Runner.Spawn(
                hitEffectPrefabs[attackIndex],
                center,
                Quaternion.identity,
                Object.InputAuthority);
        }

        Cooldowns.Set(skillIndex,
            TickTimer.CreateFromSeconds(Runner, skillCooldownTimes[skillIndex]));

        IsCasting.Set(skillIndex, false);
        pendingTargets[attackIndex] = null;
    }

    // =================================================
    // BASE ATTACK
    // =================================================

    [Rpc(RpcSources.InputAuthority, RpcTargets.StateAuthority)]
    void RPC_RequestUseBaseSkill(NetworkId targetId)
    {
        if (Cooldowns[BASE_INDEX].RemainingTime(Runner) > 0)
            return;

        CurrentTargetId = targetId;

        ActivateBaseSkill(BASE_INDEX);
    }

    void ActivateBaseSkill(int index)
    {
        var attacker = GetComponent<AttackingExample>();
        if (attacker != null)
            attacker.UseSkill(0);

        ExecuteBaseAttack();

        Cooldowns.Set(index,
            TickTimer.CreateFromSeconds(Runner, skillCooldownTimes[index]));
    }

    void ExecuteBaseAttack()
    {
        if (!HasStateAuthority) return;
        if (!Runner.TryFindObject(CurrentTargetId, out NetworkObject targetNO)) return;

        var enemy = targetNO.GetComponent<EnemyCore>();
        if (enemy == null) return;

        int statPart = GetServerStat();
        int baseSkillDamage = Random.Range(80, 110);
        int damage = statPart + Mathf.RoundToInt(baseSkillDamage * 1.2f);

        enemy.RPC_RequestHit(damage, Object.InputAuthority);
    }

    // =================================================
    // NETWORK LOOP
    // =================================================

    public override void FixedUpdateNetwork()
    {
        for (int i = 0; i < TOTAL_SKILL_COUNT; i++)
        {
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

            if (i >= BUFF_COUNT && i < BUFF_COUNT + ATTACK_COUNT &&
                IsCasting[i] &&
                BuffTimers[i].Expired(Runner))
            {
                int attackIndex = i - BUFF_COUNT;

                if (activeCastEffects[attackIndex] != null)
                {
                    Runner.Despawn(activeCastEffects[attackIndex]);
                    activeCastEffects[attackIndex] = null;
                }

                ExecuteAttack(i);
            }
        }
    }

    // =================================================
    // DAMAGE CALCULATION (SERVER ONLY)
    // =================================================

    int GetServerStat()
    {
        if (stats == null) return 0;
        return stats.strength + stats.finalStrength;
    }

    int CalculateSkillDamage(AOESkillData data)
    {
        int statPart = GetServerStat();
        int skillDamage = Random.Range(data.minDamage, data.maxDamage);
        int scaledSkill = Mathf.RoundToInt(skillDamage * 1.2f);
        return statPart + scaledSkill;
    }
}