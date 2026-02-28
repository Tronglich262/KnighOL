using UnityEngine;

[System.Serializable]
public class AOESkillData
{
    public float radius = 5f;
    public int maxTargets = 3;      // -1 = không giới hạn
    public float coneAngle = 0f;    // 0 = hình tròn, >0 = hình quạt
    public int minDamage = 100;
    public int maxDamage = 200;

    [Header("Debuff (hiệu ứng xấu)")]
    public DebuffEffect debuffType = DebuffEffect.None;
    [Range(0f, 1f)]
    [Tooltip("Xác suất xuất hiện debuff (0-1). VD: 0.5 = 50%")]
    public float debuffChance = 0.5f;
    public float debuffDuration = 2f;
    [Tooltip("Chỉ dùng cho Burn: damage mỗi 0.5s")]
    public int burnDamagePerTick = 15;
}