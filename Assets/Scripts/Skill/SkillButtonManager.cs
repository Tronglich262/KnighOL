using UnityEngine;
using UnityEngine.UI;
using System;
using System.Collections;
using HeroEditor.Common.Enums;
using Assets.HeroEditor.Common.CharacterScripts;
using Fusion;

public class SkillButtonManager : MonoBehaviour
{
    public Character character;
    public Button[] skillButtons;

    public Sprite[] melee1HIcons;
    public Sprite[] melee2HIcons;
    public Sprite[] bowIcons;
    public GameObject[] skill;

    public Action[] melee1HActions = new Action[5];
    public Action[] melee2HActions = new Action[5];
    public Action[] bowActions = new Action[5];
    public SkillCooldownUI cooldownManager;
    private WeaponType lastWeaponType;
    private bool isReady;
    public static SkillButtonManager Instance { get; private set; }
    private void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);
    }
    void Start()
    {
        StartCoroutine(FindLocalPlayer());
    }

    IEnumerator FindLocalPlayer()
    {
        while (character == null)
        {
            foreach (var p in GameObject.FindGameObjectsWithTag("Player"))
            {
                var net = p.GetComponent<NetworkObject>();
                if (net != null && net.HasInputAuthority)
                {
                    character = p.GetComponent<Character>();
                    break;
                }
            }
            yield return null;
        }

        var attacker = character.GetComponent<Assets.HeroEditor.Common.ExampleScripts.AttackingExample>();
        var buff = character.GetComponent<BuffSkillNetwork>();   

        // Base Skill
        melee1HActions[0] = () => buff.TryUseBaseSkill();
        melee2HActions[0] = () => buff.TryUseBaseSkill();
        bowActions[0] = () => buff.TryUseBaseSkill();

        // Melee1H
        melee1HActions[1] = () => buff.TryUseBuff(0);
        melee1HActions[2] = () => buff.TryUseBuff(1);
        melee1HActions[3] = () => buff.TryUseAttack(6);
        melee1HActions[4] = () => buff.TryUseAttack(7);

        // Melee2H
        melee2HActions[1] = () => buff.TryUseBuff(2);
        melee2HActions[2] = () => buff.TryUseBuff(3);
        melee2HActions[3] = () => buff.TryUseAttack(10);
        melee2HActions[4] = () => buff.TryUseAttack(11);

        // Bow
        bowActions[1] = () => buff.TryUseBuff(4);
        bowActions[2] = () => buff.TryUseBuff(5);
        bowActions[3] = () => buff.TryUseAttack(8);
        bowActions[4] = () => buff.TryUseAttack(9);

        lastWeaponType = character.WeaponType;
        UpdateSkillButtons(lastWeaponType);
        isReady = true;
    }

    void Update()
    {
        if (!isReady) return;

        if (character.WeaponType != lastWeaponType)
        {
            UpdateSkillButtons(character.WeaponType);
            lastWeaponType = character.WeaponType;
        }
    }

    void UpdateSkillButtons(WeaponType weaponType)
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
        }

        for (int i = 0; i < skillButtons.Length; i++)
        {
            skillButtons[i].image.sprite =
                icons != null && i < icons.Length ? icons[i] : null;

            skillButtons[i].onClick.RemoveAllListeners();

            if (actions != null && i < actions.Length && actions[i] != null)
            {
                int idx = i;
                skillButtons[i].onClick.AddListener(() => actions[idx]());
            }
        }

        // ===== FIX COOLDOWN INDEX =====

        switch (weaponType)
        {
            case WeaponType.Melee1H:
                cooldownManager.SetSkillIndex(0, 12);
                cooldownManager.SetSkillIndex(1, 0); // Buff 0
                cooldownManager.SetSkillIndex(2, 1); // Buff 1
                cooldownManager.SetSkillIndex(3, 6); // Attack 1
                cooldownManager.SetSkillIndex(4, 7); // Attack 2
                break;

            case WeaponType.Melee2H:
                cooldownManager.SetSkillIndex(0, 12);
                cooldownManager.SetSkillIndex(1, 2); // Buff 2
                cooldownManager.SetSkillIndex(2, 3); // Buff 3
                cooldownManager.SetSkillIndex(3, 10);
                cooldownManager.SetSkillIndex(4, 11);
                break;

            case WeaponType.Bow:
                cooldownManager.SetSkillIndex(0, 12);
                cooldownManager.SetSkillIndex(1, 4); // Buff 4
                cooldownManager.SetSkillIndex(2, 5); // Buff 5
                cooldownManager.SetSkillIndex(3, 8);
                cooldownManager.SetSkillIndex(4, 9);
                break;
        }
    }
    //tat bat skill
    public void ToggleSkills(bool isActive)
    {
        foreach (var button in skill)
        {
            button.gameObject.SetActive(isActive);
        }
    }
    int GetWeaponBaseIndex(WeaponType type)
    {
        switch (type)
        {
            case WeaponType.Melee1H: return 0;
            case WeaponType.Melee2H: return 2;
            case WeaponType.Bow: return 4;
        }
        return -1;
    }

}
