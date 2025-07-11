using UnityEngine;
using UnityEngine.UI;
using System;
using System.Collections;
using HeroEditor.Common;
using HeroEditor.Common.Enums;
using Assets.HeroEditor.Common.CharacterScripts;
using Fusion; // THÊM Fusion nếu chưa có

public class SkillButtonManager : MonoBehaviour
{
    [Header("Tham chiếu Player")]
    public Character character;

    [Header("Các button skill trên UI")]
    public Button[] skillButtons;

    [Header("Icon skill của từng loại vũ khí")]
    public Sprite[] melee1HIcons;
    public Sprite[] melee2HIcons;
    public Sprite[] bowIcons;

    public Action[] melee1HActions = new Action[5];
    public Action[] melee2HActions = new Action[5];
    public Action[] bowActions = new Action[5];

    private WeaponType lastWeaponType;
    private bool isReady = false;
    public static SkillButtonManager Instance; 
    
    // KHÔNG dùng static Singleton nữa!

    void Start()
    {
        StartCoroutine(DelayFindLocalPlayer());
    }

    IEnumerator DelayFindLocalPlayer()
    {
        float timeout = 2f;
        float t = 0f;

        while (character == null && t < timeout)
        {
            // Tìm mọi GameObject có tag "Player"
            foreach (var playerObj in GameObject.FindGameObjectsWithTag("Player"))
            {
                var netObj = playerObj.GetComponent<NetworkObject>();
                if (netObj != null && netObj.HasInputAuthority) // Chỉ lấy player local!
                {
                    character = playerObj.GetComponent<Character>();
                    break;
                }
            }
            if (character == null)
            {
                yield return null;
                t += Time.deltaTime;
            }
        }

        if (character == null)
        {
            Debug.LogError("Không tìm thấy player local (HasInputAuthority)!");
            enabled = false;
            yield break;
        }

        // Gán function test (giữ nguyên code mẫu hoặc custom theo từng loại vũ khí)
        melee1HActions[0] = () => Debug.Log("Kiếm 1 tay - Chém thường");
        melee1HActions[1] = () => Debug.Log("Kiếm 1 tay - Skill 2");
        melee1HActions[2] = () => Debug.Log("Kiếm 1 tay - Skill 3");
        melee1HActions[3] = () => Debug.Log("Kiếm 1 tay - Skill 4");
        melee1HActions[4] = () => Debug.Log("Kiếm 1 tay - Skill 5");

        melee2HActions[0] = () => Debug.Log("Vũ khí 2 tay - Đập thường");
        melee2HActions[1] = () => Debug.Log("Vũ khí 2 tay - Skill 2");
        melee2HActions[2] = () => Debug.Log("Vũ khí 2 tay - Skill 3");
        melee2HActions[3] = () => Debug.Log("Vũ khí 2 tay - Skill 4");
        melee2HActions[4] = () => Debug.Log("Vũ khí 2 tay - Skill 5");

        bowActions[0] = () => Debug.Log("Cung - Bắn thường");
        bowActions[1] = () => Debug.Log("Cung - Skill 2");
        bowActions[2] = () => Debug.Log("Cung - Skill 3");
        bowActions[3] = () => Debug.Log("Cung - Skill 4");
        bowActions[4] = () => Debug.Log("Cung - Skill 5");

        lastWeaponType = character.WeaponType;
        UpdateSkillButtons(lastWeaponType);
        isReady = true;
    }

    void Update()
    {
        if (!isReady || character == null) return;

        if (character.WeaponType != lastWeaponType)
        {
            UpdateSkillButtons(character.WeaponType);
            lastWeaponType = character.WeaponType;
        }
    }

    public void UpdateSkillButtons(WeaponType weaponType)
    {
        Sprite[] icons = null;
        Action[] actions = null;

        switch (weaponType)
        {
            case WeaponType.Melee1H:
                icons = melee1HIcons;
                actions = melee1HActions;
                break;
            case WeaponType.Melee2H:
                icons = melee2HIcons;
                actions = melee2HActions;
                break;
            case WeaponType.Bow:
                icons = bowIcons;
                actions = bowActions;
                break;
            default:
                icons = new Sprite[skillButtons.Length];
                actions = new Action[skillButtons.Length];
                break;
        }

        for (int i = 0; i < skillButtons.Length; i++)
        {
            skillButtons[i].image.sprite = (icons != null && i < icons.Length) ? icons[i] : null;
            skillButtons[i].onClick.RemoveAllListeners();
            if (actions != null && i < actions.Length && actions[i] != null)
            {
                int idx = i;
                skillButtons[i].onClick.AddListener(() => actions[idx]?.Invoke());
            }
        }
    }
}
