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

        GameObject cloneObj = GameObject.Find("ClonePreview");
        if (cloneObj != null)
            characterPreview = cloneObj.GetComponent<Character>();

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

    public void LoadCharacterFromJson(string json)
    {
        presenter.LoadFromJson(json, applyVisual: true);
    }

    public void ClearPreviewData()
    {
        presenter.ClearAllSlots();
        equippedItems.Clear();
    }
}