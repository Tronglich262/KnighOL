using System.Collections.Generic;
using UnityEngine;
using Assets.HeroEditor.Common.CharacterScripts;

public class CharacterPreviewPanel : MonoBehaviour
{
    public static CharacterPreviewPanel Instance;

    [Header("Preview Character")]
    public Character characterPreview;

    [Header("Slots")]
    public GameObject Helmetslot;
    public GameObject[] ArmorSlots;
    public GameObject Vestslot;
    public GameObject Pauldronsslot;
    public GameObject Glovesslot;
    public GameObject Bootslot;
    public GameObject Bowslot;
    public GameObject Hairslot;
    public GameObject Beltslot;
    public GameObject Capeslot;
    public GameObject Backslot;
    public GameObject Maskslot;
    public GameObject Glassesslot;
    public GameObject Shieldslot;
    public GameObject ArmorGeneralSlot;
    public GameObject MeleeWeapon1Hslot;
    public GameObject MeleeWeapon2Hslot;

    private readonly List<ItemStats> equippedItems = new(16);
    private CharacterEquipmentPresenter presenter;

    private void Awake()
    {
        Instance = this;

        if (characterPreview == null)
        {
            GameObject cloneObj = GameObject.Find("ClonePreview");
            if (cloneObj != null)
            {
                characterPreview = cloneObj.GetComponent<Character>();
                if (characterPreview == null)
                    characterPreview = cloneObj.GetComponentInChildren<Character>(true);
            }
        }

        presenter = new CharacterEquipmentPresenter(
            characterPreview,
            equippedItems,
            Helmetslot,
            ArmorSlots,
            Vestslot,
            Pauldronsslot,
            Glovesslot,
            Bootslot,
            Bowslot,
            Hairslot,
            Beltslot,
            Capeslot,
            Backslot,
            Maskslot,
            Glassesslot,
            Shieldslot,
            ArmorGeneralSlot,
            MeleeWeapon1Hslot,
            MeleeWeapon2Hslot
        );

        gameObject.SetActive(false);
    }

    public void ShowPreviewOfOtherPlayer(string json)
    {
        if (string.IsNullOrEmpty(json))
        {
            Debug.LogWarning("[CharacterPreviewPanel] json rỗng.");
            return;
        }

        if (characterPreview == null)
        {
            Debug.LogWarning("[CharacterPreviewPanel] characterPreview null.");
            return;
        }

        ClearAllPreviewData();

        // 1) Render icon slot UI
        presenter.LoadFromJson(json, applyVisual: false);

        // 2) Render visual thật của preview character
        var dict = CharacterJsonService.LoadDict(json);
        CharacterVisualCompositeBuilder.ApplyAll(characterPreview, dict);

        gameObject.SetActive(true);
    }

    public void LoadCharacterFromJson(string json)
    {
        ShowPreviewOfOtherPlayer(json);
    }

    public void ClearPreviewData()
    {
        ClearAllPreviewData();
    }

    private void ClearAllPreviewData()
    {
        equippedItems.Clear();

        if (presenter != null)
            presenter.ClearAllSlots();

        ClearPreviewCharacterVisual();
    }

    private void ClearPreviewCharacterVisual()
    {
        if (characterPreview == null)
            return;

        var emptyDict = CharacterJsonService.CreateEmptyDict();
        CharacterVisualCompositeBuilder.ApplyAll(characterPreview, emptyDict);
    }
}