[System.Serializable]
public class AOESkillData
{
    public float radius = 5f;
    public int maxTargets = 3;      // -1 = không giới hạn
    public float coneAngle = 0f;    // 0 = hình tròn, >0 = hình quạt
    public int minDamage = 100;
    public int maxDamage = 200;
}