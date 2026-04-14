using System.Collections.Generic;
using UnityEngine;
using Assets.HeroEditor.Common.CharacterScripts;
using Newtonsoft.Json;

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
            characterPreview, equippedItems,
            Helmetslot, ArmorSlots, Vestslot, Pauldronsslot, Glovesslot, Bootslot,
            Bowslot, Hairslot, Beltslot, Capeslot, Backslot, Maskslot, Glassesslot,
            Shieldslot, ArmorGeneralSlot, MeleeWeapon1Hslot, MeleeWeapon2Hslot
        );

        gameObject.SetActive(false);
    }

    // ====================== HÀM CHÍNH - XEM THÔNG TIN NGƯỜI KHÁC ======================
    public void ShowPreviewOfOtherPlayer(string json)
    {
        if (string.IsNullOrEmpty(json) || characterPreview == null)
        {
            Debug.LogWarning("JSON rỗng hoặc characterPreview null!");
            return;
        }

        // BƯỚC 1: XÓA SẠCH HẾT (rất quan trọng)
        ClearAllPreviewData();

        // BƯỚC 2: Load visual nhân vật
        presenter.LoadFromJson(json, applyVisual: true);

        // BƯỚC 3: Load icon vào slot (xử lý cả tháo đồ)
        LoadAllItemIconsFromJson(json);
    }

    // Xóa sạch hết dữ liệu và icon
    private void ClearAllPreviewData()
    {
        presenter.ClearAllSlots();
        equippedItems.Clear();
        ClearAllUISlots();
    }

    // Clear tất cả icon trên UI
    private void ClearAllUISlots()
    {
        ClearSlot(Helmetslot);
        ClearSlot(Hairslot);
        ClearSlot(Backslot);
        ClearSlot(Capeslot);
        ClearSlot(Maskslot);
        ClearSlot(Glassesslot);
        ClearSlot(Shieldslot);
        ClearSlot(Beltslot);
        ClearSlot(Bootslot);
        ClearSlot(Vestslot);
        ClearSlot(Pauldronsslot);
        ClearSlot(Glovesslot);
        ClearSlot(ArmorGeneralSlot);

        foreach (var slot in ArmorSlots) ClearSlot(slot);

        ClearSlot(MeleeWeapon1Hslot);
        ClearSlot(MeleeWeapon2Hslot);
        ClearSlot(Bowslot);
    }

    private void ClearSlot(GameObject slot)
    {
        if (slot != null)
            CharacterEquipmentHelper.ClearSlotUI(slot);
    }

    // Load icon - Phần quan trọng nhất để unequip hoạt động
    private void LoadAllItemIconsFromJson(string json)
    {
        if (string.IsNullOrEmpty(json)) return;

        // Luôn clear icon trước khi load → khi tháo đồ sẽ thành trống
        ClearAllUISlots();

        // Load các món đang mặc
        LoadSingleItem(Helmetslot, GetItemIdFromJson(json, "Helmet"), "Helmet");
        LoadSingleItem(Hairslot, GetItemIdFromJson(json, "Hair"), "Hair");
        LoadSingleItem(Backslot, GetItemIdFromJson(json, "Back"), "Back");
        LoadSingleItem(Capeslot, GetItemIdFromJson(json, "Cape"), "Cape");
        LoadSingleItem(Maskslot, GetItemIdFromJson(json, "Mask"), "Mask");
        LoadSingleItem(Glassesslot, GetItemIdFromJson(json, "Glasses"), "Glasses");
        LoadSingleItem(Shieldslot, GetItemIdFromJson(json, "Shield"), "Shield");
        LoadSingleItem(Beltslot, GetItemIdFromJson(json, "Belt"), "Belt");
        LoadSingleItem(Bootslot, GetItemIdFromJson(json, "Boots"), "Boots");
        LoadSingleItem(Vestslot, GetItemIdFromJson(json, "Vest"), "Vest");
        LoadSingleItem(Pauldronsslot, GetItemIdFromJson(json, "Pauldrons"), "Pauldrons");
        LoadSingleItem(Glovesslot, GetItemIdFromJson(json, "Gloves"), "Gloves");

        LoadSingleItem(ArmorGeneralSlot, GetItemIdFromJson(json, "Armor"), "Armor");
        foreach (var slot in ArmorSlots)
            LoadSingleItem(slot, GetItemIdFromJson(json, "Armor"), "Armor");

        // Weapons
        LoadSingleItem(MeleeWeapon1Hslot, GetItemIdFromJson(json, "MeleeWeapon1H"), "MeleeWeapon1H");
        LoadSingleItem(MeleeWeapon2Hslot, GetItemIdFromJson(json, "MeleeWeapon2H"), "MeleeWeapon2H");
        LoadSingleItem(Bowslot, GetItemIdFromJson(json, "Bow"), "Bow");
    }

    private void LoadSingleItem(GameObject slot, string itemId, string expectedType)
    {
        if (string.IsNullOrEmpty(itemId) || slot == null) return;

        string path = GetItemPathByType(expectedType, itemId);
        var stats = CharacterSlotRenderer.DisplaySlot(slot, path, expectedType);
        if (stats != null)
            equippedItems.Add(stats);
    }

    private string GetItemPathByType(string type, string itemId)
    {
        return $"Items/{type}/{itemId}";
    }

    public string GetItemIdFromJson(string json, string key)
    {
        if (string.IsNullOrEmpty(json) || string.IsNullOrEmpty(key))
            return null;

        try
        {
            var dict = JsonConvert.DeserializeObject<Dictionary<string, string>>(json);
            return dict != null && dict.TryGetValue(key, out string value) ? value : null;
        }
        catch
        {
            return null;
        }
    }

    // Giữ lại để tương thích
    public void LoadCharacterFromJson(string json)
    {
        presenter.LoadFromJson(json, applyVisual: true);
    }

    public void ClearPreviewData()
    {
        ClearAllPreviewData();
    }
}