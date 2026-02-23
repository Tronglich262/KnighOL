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

        melee1HActions[0] = () => attacker.UseSkill(0);
        melee2HActions[0] = () => attacker.UseSkill(0);
        bowActions[0] = () => attacker.UseSkill(0);

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
            skillButtons[i].image.sprite = icons != null && i < icons.Length ? icons[i] : null;
            skillButtons[i].onClick.RemoveAllListeners();

            if (actions != null && i < actions.Length && actions[i] != null)
            {
                int idx = i;
                skillButtons[i].onClick.AddListener(() => actions[idx]());
            }
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

}
