using Fusion;
using UnityEngine;

/// <summary>
/// Quản lý hiệu ứng xấu trên enemy: Stun, Burn, Dizzy.
/// Icon hiển thị trên đầu enemy (gán DebuffIconDisplay để hiển thị).
/// </summary>
public class EnemyDebuffManager : NetworkBehaviour
{
    [Networked] private TickTimer StunTimer { get; set; }
    [Networked] private TickTimer BurnTimer { get; set; }
    [Networked] private TickTimer DizzyTimer { get; set; }
    [Networked] private TickTimer BurnNextTickTimer { get; set; }
    [Networked] private int BurnDamagePerTick { get; set; }

    public EnemyStats Stats;
    public EnemyCore Core;
    public DebuffIconDisplay IconDisplay;

    private const float BurnTickInterval = 0.5f;

    /// <summary>Local preview cho client (proxy): hiển thị icon ngay khi nhận RPC trước khi [Networked] replicate.</summary>
    float _localStunEnd;
    float _localBurnEnd;
    float _localDizzyEnd;

    public override void Spawned()
    {
        Stats = GetComponent<EnemyStats>();
        Core = GetComponent<EnemyCore>();
        if (IconDisplay == null)
            IconDisplay = GetComponentInChildren<DebuffIconDisplay>(true);
    }

    public override void FixedUpdateNetwork()
    {
        if (HasStateAuthority)
        {
            if (Core != null && Stats != null && Stats.HP > 0)
                TickBurn();
        }

        RefreshIconDisplay();
    }

    public override void Render()
    {
        RefreshIconDisplay();
    }

    void Update()
    {
        RefreshIconDisplay();
    }

    void TickBurn()
    {
        if (BurnTimer.ExpiredOrNotRunning(Runner)) return;

        if (!BurnNextTickTimer.ExpiredOrNotRunning(Runner)) return;

        int dmg = BurnDamagePerTick;
        if (dmg > 0)
        {
            Stats.TakeDamage(dmg);
            if (Core != null)
            {
                Core.RPC_ShowDamage(dmg);
                if (Stats.HP <= 0)
                    Core.Die();
            }
        }

        BurnNextTickTimer = TickTimer.CreateFromSeconds(Runner, BurnTickInterval);
    }

    void RefreshIconDisplay()
    {
        if (IconDisplay == null)
        {
            IconDisplay = GetComponentInChildren<DebuffIconDisplay>(true);
            if (IconDisplay == null) return;
        }

        bool stun;
        bool burn;
        bool dizzy;
        float stunRem, burnRem, dizzyRem;

        float t = Time.time;
        bool stunNet = false, burnNet = false, dizzyNet = false;
        float stunRemNet = 0f, burnRemNet = 0f, dizzyRemNet = 0f;

        if (Runner != null && Runner.IsRunning)
        {
            stunNet = !StunTimer.ExpiredOrNotRunning(Runner);
            burnNet = !BurnTimer.ExpiredOrNotRunning(Runner);
            dizzyNet = !DizzyTimer.ExpiredOrNotRunning(Runner);
            stunRemNet = stunNet ? StunTimer.RemainingTime(Runner).Value : 0f;
            burnRemNet = burnNet ? BurnTimer.RemainingTime(Runner).Value : 0f;
            dizzyRemNet = dizzyNet ? DizzyTimer.RemainingTime(Runner).Value : 0f;
        }

        if (Object.HasStateAuthority)
        {
            stun = stunNet;
            burn = burnNet;
            dizzy = dizzyNet;
            stunRem = stunRemNet;
            burnRem = burnRemNet;
            dizzyRem = dizzyRemNet;
        }
        else
        {
            // Proxy: dùng [Networked] timers (replicate tới late joiner) HOẶC _local* (preview ngay khi nhận RPC)
            bool stunLocal = t < _localStunEnd;
            bool burnLocal = t < _localBurnEnd;
            bool dizzyLocal = t < _localDizzyEnd;
            stun = stunNet || stunLocal;
            burn = burnNet || burnLocal;
            dizzy = dizzyNet || dizzyLocal;
            stunRem = stun ? (stunNet ? stunRemNet : Mathf.Max(0, _localStunEnd - t)) : 0f;
            burnRem = burn ? (burnNet ? burnRemNet : Mathf.Max(0, _localBurnEnd - t)) : 0f;
            dizzyRem = dizzy ? (dizzyNet ? dizzyRemNet : Mathf.Max(0, _localDizzyEnd - t)) : 0f;
        }

        IconDisplay.SetStunActive(stun);
        IconDisplay.SetBurnActive(burn);
        IconDisplay.SetDizzyActive(dizzy);
        IconDisplay.SetRemainingTimes(stunRem, burnRem, dizzyRem);
    }

    [Rpc(RpcSources.All, RpcTargets.StateAuthority)]
    public void RPC_RequestApplyDebuff(DebuffEffect type, float duration, int burnDamagePerTick)
    {
        // Chỉ StateAuthority mới vào đây
        ApplyDebuffInternal(type, duration, burnDamagePerTick);
    }
    private void ApplyDebuffInternal(DebuffEffect type, float duration, int burnDamagePerTick)
    {
        if (duration <= 0f || Runner == null || !Runner.IsRunning) return;

        var timer = TickTimer.CreateFromSeconds(Runner, duration);

        switch (type)
        {
            case DebuffEffect.Stun:
                StunTimer = timer;
                break;

            case DebuffEffect.Burn:
                BurnTimer = timer;
                BurnDamagePerTick = Mathf.Max(1, burnDamagePerTick);
                BurnNextTickTimer = TickTimer.CreateFromSeconds(Runner, BurnTickInterval);
                break;

            case DebuffEffect.Dizzy:
                DizzyTimer = timer;
                break;
        }
    }
    public bool IsStunned => !StunTimer.ExpiredOrNotRunning(Runner);
    public bool IsBurning => !BurnTimer.ExpiredOrNotRunning(Runner);
    public bool IsDizzy => !DizzyTimer.ExpiredOrNotRunning(Runner);
    public bool CannotAct => IsStunned || IsDizzy;
}
