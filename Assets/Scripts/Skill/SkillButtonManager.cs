using UnityEngine;
using UnityEngine.UI;
using System;
using System.Collections;
using HeroEditor.Common;
using HeroEditor.Common.Enums;
using Assets.HeroEditor.Common.CharacterScripts;

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

    public Action[] melee1HActions = new Action[4];
    public Action[] melee2HActions = new Action[4];
    public Action[] bowActions = new Action[4];

    private WeaponType lastWeaponType;
    private bool isReady = false;
    public GameObject Skillbutton;
    public static SkillButtonManager Instance;
    public void Awake()
    {
        Instance = this;
    }

    void Start()
    {
        StartCoroutine(DelayFindPlayer());
    }

    IEnumerator DelayFindPlayer()
    {
        // Đợi tới khi tìm được Player hoặc sau 2 giây (tránh lặp vô hạn)
        float timeout = 2f;
        float t = 0f;
        while (character == null && t < timeout)
        {
            GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
            if (playerObj != null)
            {
                character = playerObj.GetComponent<Character>();
            }
            else
            {
                yield return null; // Chờ frame kế tiếp
                t += Time.deltaTime;
            }
        }

        if (character == null)
        {
            Debug.LogError("Không tìm thấy player (Character) gắn tag 'Player' sau khi chờ!");
            enabled = false;
            yield break;
        }

        // Gán function test (nếu bạn chưa gán từ script khác)
        melee1HActions[0] = () => Debug.Log("Kiếm 1 tay - Chém thường");
        melee1HActions[1] = () => Debug.Log("Kiếm 1 tay - Skill 2");
        melee1HActions[2] = () => Debug.Log("Kiếm 1 tay - Skill 3");
        melee1HActions[3] = () => Debug.Log("Kiếm 1 tay - Skill 4");

        melee2HActions[0] = () => Debug.Log("Vũ khí 2 tay - Đập thường");
        melee2HActions[1] = () => Debug.Log("Vũ khí 2 tay - Skill 2");
        melee2HActions[2] = () => Debug.Log("Vũ khí 2 tay - Skill 3");
        melee2HActions[3] = () => Debug.Log("Vũ khí 2 tay - Skill 4");

        bowActions[0] = () => Debug.Log("Cung - Bắn thường");
        bowActions[1] = () => Debug.Log("Cung - Skill 2");
        bowActions[2] = () => Debug.Log("Cung - Skill 3");
        bowActions[3] = () => Debug.Log("Cung - Skill 4");

        lastWeaponType = character.WeaponType;
        UpdateSkillButtons(lastWeaponType); // Set UI đúng lúc đã sẵn sàng
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
                icons = new Sprite[4];
                actions = new Action[4];
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
