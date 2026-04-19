using UnityEngine;

public enum SkillWeaponType
{
    Melee1H,
    Melee2H,
    Bow
}

[CreateAssetMenu(fileName = "NewSkill", menuName = "Skill System/Skill Data")]
public class SkillSO : ScriptableObject
{
    public string skillID;
    public string skillName;
    [TextArea] public string description;
    public Sprite icon;
    public SkillWeaponType weaponType;
}
