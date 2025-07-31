using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using TMPro;
using HeroEditor.Common.Enums;

public class CharacterSkillUIManager : MonoBehaviour
{
    [Header("UI Elements")]
    public Transform contentParent;           // SkillContent trong Scroll View
    public GameObject skillItemPrefab;        // Prefab nút kỹ năng
    public TextMeshProUGUI descriptionText;   // Text hiển thị mô tả skill

    [Header("Hiển thị kỹ năng theo vũ khí")]
    public WeaponType currentWeaponType = WeaponType.Melee1H;

    private List<GameObject> spawnedButtons = new List<GameObject>();

    void Start()
    {
        LoadAndShowSkills();
    }

    public void LoadAndShowSkills()
    {
        SkillSO[] skills = Resources.LoadAll<SkillSO>("Skills");

        // Xoá cũ
        foreach (var btn in spawnedButtons)
            Destroy(btn);
        spawnedButtons.Clear();

        foreach (var skill in skills)
        {
            GameObject btnObj = Instantiate(skillItemPrefab, contentParent);
            var btn = btnObj.GetComponent<Button>();
            var icon = btnObj.GetComponentInChildren<Image>();
            var uiItem = btnObj.GetComponent<SkillUIItem>();

            uiItem.skillSO = skill;
            icon.sprite = skill.icon;

            bool match = IsWeaponMatch(skill.weaponType, currentWeaponType);

            // Mờ nếu không khớp
           // icon.color = match ? Color.white : new Color(1, 1, 1, 0.3f);

            // Click luôn được để hiện mô tả
            btn.onClick.AddListener(() => ShowSkillDescription(skill));

            spawnedButtons.Add(btnObj);
        }

        descriptionText.text = "";
    }

    void ShowSkillDescription(SkillSO skill)
    {
        descriptionText.text = $"<b>{skill.skillName}</b>\n{skill.description}";
    }

    bool IsWeaponMatch(SkillWeaponType skillType, WeaponType playerWeapon)
    {
        return skillType switch
        {
            SkillWeaponType.Melee1H => playerWeapon == WeaponType.Melee1H,
            SkillWeaponType.Melee2H => playerWeapon == WeaponType.Melee2H,
            SkillWeaponType.Bow => playerWeapon == WeaponType.Bow,
            _ => false
        };
    }
}
