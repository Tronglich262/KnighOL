
//new gỡ trang bị swap
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
// hiển thị bảng panel thông tin trang bị nhân vật
public class ItemDetailsPanel : MonoBehaviour
{
    public static ItemDetailsPanel Instance;

    public GameObject panel;
    public Image icon;
    public TMP_Text description;
    public TMP_Text Type;
    public TMP_Text Name;
    public Button unequipButton; // Gán trong Inspector
    private string currentItemId;
    private string currentType;

    //text
    public TextMeshProUGUI equipMessageText;
    private Coroutine equipMessageCoroutine;
    private Vector3 equipMsgOriginPos;
    public bool ischeckgodo = false;
    private void Awake()
    {
        Instance = this;
        panel.SetActive(false);
    }

    public void Show(string id, Sprite iconSprite, string type = null)
    {
        if (!panel.activeSelf) panel.SetActive(true);
        Debug.Log($"da goi");
        icon.sprite = iconSprite;

        currentItemId = id;
        currentType = type;

        string name = id.Split('.').Length > 0 ? id.Split('.').Last() : id;
        string displayType = type ?? "Không rõ loại";

        description.text = $"{displayType}\n\n{GetStatsFromId(id)}";
        Type.text = $"Loại: {displayType}";

        Name.text = $"Tên: {name}";
    }


    public void Hide()
    {
        panel.SetActive(false);
    }
   

    private string GetStatsFromId(string id)
    {
        var stats = ItemStatDatabase.Instance.GetStats(id);
        if (stats == null)
            return "Không có thông tin.";

        return
            $"Sức mạnh: {stats.Strength}\n" +
            $"Phòng thủ: {stats.Defense}\n" +
            $"Nhanh nhẹn: {stats.Agility}\n" +
            $"Trí tuệ: {stats.Intelligence}\n" +
            $"Thể lực: {stats.Vitality}";
    }
    //gỡ ttrang bị 
    public void OnUnequipButtonClick()
    {
        if (string.IsNullOrEmpty(currentType)) return;
        if (!System.Enum.TryParse(currentType, out HeroEditor.Common.Enums.EquipmentPart part)) return;
        var character = CharacterUIManager1.Instance?.character;
        if (character == null) return;

        character.UnEquip(part);

        var json = PlayerDataHolder1.CharacterJson;
        if (!string.IsNullOrEmpty(json))
        {
            var dict = Newtonsoft.Json.JsonConvert.DeserializeObject<Dictionary<string, string>>(json);

            if (part == HeroEditor.Common.Enums.EquipmentPart.Bow)
            {
                // Lấy bow đang mặc
                if (dict.TryGetValue("Bow", out var oldBowId) && !string.IsNullOrEmpty(oldBowId))
                    InventoryManager.Instance.AddItem(oldBowId, 1);

                if (dict.TryGetValue("PrimaryMeleeWeapon", out var old1HId) && !string.IsNullOrEmpty(old1HId))
                    InventoryManager.Instance.AddItem(old1HId, 1);

                if (dict.TryGetValue("SecondaryMeleeWeapon", out var old2HId) && !string.IsNullOrEmpty(old2HId))
                    InventoryManager.Instance.AddItem(old2HId, 1);

                dict["Bow"] = "";
                dict["PrimaryMeleeWeapon"] = "";
                dict["SecondaryMeleeWeapon"] = "";
                dict["WeaponType"] = "Melee1H";
                CharacterUIManager1.Instance?.ClearSlot(CharacterUIManager1.Instance.Bowslot);
                CharacterUIManager1.Instance?.ClearSlot(CharacterUIManager1.Instance.MeleeWeapon1Hslot);
                CharacterUIManager1.Instance?.ClearSlot(CharacterUIManager1.Instance.MeleeWeapon2Hslot);
            }
            else if (part == HeroEditor.Common.Enums.EquipmentPart.MeleeWeapon1H ||
                     part == HeroEditor.Common.Enums.EquipmentPart.MeleeWeapon2H ||
                     part == HeroEditor.Common.Enums.EquipmentPart.MeleeWeaponPaired)
            {
                if (dict.TryGetValue("PrimaryMeleeWeapon", out var old1HId) && !string.IsNullOrEmpty(old1HId))
                    InventoryManager.Instance.AddItem(old1HId, 1);

                if (dict.TryGetValue("SecondaryMeleeWeapon", out var old2HId) && !string.IsNullOrEmpty(old2HId))
                    InventoryManager.Instance.AddItem(old2HId, 1);

                dict["PrimaryMeleeWeapon"] = "";
                dict["SecondaryMeleeWeapon"] = "";
                dict["WeaponType"] = "Melee1H";
                CharacterUIManager1.Instance?.ClearSlot(CharacterUIManager1.Instance.MeleeWeapon1Hslot);
                CharacterUIManager1.Instance?.ClearSlot(CharacterUIManager1.Instance.MeleeWeapon2Hslot);
            }
            else if (part == HeroEditor.Common.Enums.EquipmentPart.Glasses)
            {
                if (dict.TryGetValue("Glasses", out var oldId) && !string.IsNullOrEmpty(oldId))
                    InventoryManager.Instance.AddItem(oldId, 1);

                character.Glasses = null;
                dict["Glasses"] = "";
                CharacterUIManager1.Instance?.ClearSlot(CharacterUIManager1.Instance.Glassesslot);
            }
            else if (part == HeroEditor.Common.Enums.EquipmentPart.Helmet)
            {
                if (dict.TryGetValue("Helmet", out var oldId) && !string.IsNullOrEmpty(oldId))
                    InventoryManager.Instance.AddItem(oldId, 1);

                character.Helmet = null;
                dict["Helmet"] = "";
                CharacterUIManager1.Instance?.ClearSlot(CharacterUIManager1.Instance.Helmetslot);
            }
            else if (part == HeroEditor.Common.Enums.EquipmentPart.Mask)
            {
                if (dict.TryGetValue("Mask", out var oldId) && !string.IsNullOrEmpty(oldId))
                    InventoryManager.Instance.AddItem(oldId, 1);

                character.Mask = null;
                dict["Mask"] = "";
                CharacterUIManager1.Instance?.ClearSlot(CharacterUIManager1.Instance.Maskslot);
            }
            else if (part == HeroEditor.Common.Enums.EquipmentPart.Shield)
            {
                if (dict.TryGetValue("Shield", out var oldId) && !string.IsNullOrEmpty(oldId))
                    InventoryManager.Instance.AddItem(oldId, 1);

                character.Shield = null;
                dict["Shield"] = "";
                CharacterUIManager1.Instance?.ClearSlot(CharacterUIManager1.Instance.Shieldslot);
            }
            else if (part == HeroEditor.Common.Enums.EquipmentPart.Cape)
            {
                if (dict.TryGetValue("Cape", out var oldId) && !string.IsNullOrEmpty(oldId))
                    InventoryManager.Instance.AddItem(oldId, 1);

                character.Cape = null;
                dict["Cape"] = "";
                CharacterUIManager1.Instance?.ClearSlot(CharacterUIManager1.Instance.Capeslot);
            }
            else if (part == HeroEditor.Common.Enums.EquipmentPart.Back)
            {
                if (dict.TryGetValue("Back", out var oldId) && !string.IsNullOrEmpty(oldId))
                    InventoryManager.Instance.AddItem(oldId, 1);

                character.Back = null;
                dict["Back"] = "";
                CharacterUIManager1.Instance?.ClearSlot(CharacterUIManager1.Instance.Backslot);
            }
            else
            {
                // Gỡ đúng loại Armor
                string[] armorTypes = { "Armor", "Boots", "Gloves", "Pauldrons", "Vest", "Belt" };
                int idx = System.Array.IndexOf(armorTypes, currentType);
                if (idx >= 0 && idx < character.Armor.Count)
                {
                    // Lấy itemId đang mặc ở slot này
                    if (dict.TryGetValue(armorTypes[idx], out var oldArmorId) && !string.IsNullOrEmpty(oldArmorId))
                        InventoryManager.Instance.AddItem(oldArmorId, 1);

                    character.Armor[idx] = null;
                    character.EquipArmor(character.Armor);
                    dict[armorTypes[idx]] = "";
                    dict["Armor"] = ""; // Bắt buộc phải clear Armor tổng
                }
            }


            // Serialize lại JSON
            string updatedJson = Newtonsoft.Json.JsonConvert.SerializeObject(dict);
            PlayerDataHolder1.CharacterJson = updatedJson;

            // Gọi lại LoadJson đúng sau khi update

            // Gửi về player thật nếu có
            var playerClone = ItemDetailsUI.Instance?.playerClone;
            if (playerClone != null)
            {
                var cloneCtrl = playerClone.GetComponent<PlayerCloneController>();
                if (cloneCtrl != null)
                {
                    cloneCtrl.SendCharacterJsonToTarget(updatedJson);
                    cloneCtrl.LoadJson(PlayerDataHolder1.CharacterJson); 

                }
            }

            // Cập nhật lại UI slot
            CharacterUIManager1.Instance?.LoadCharacterToUI();

            // Đồng bộ lên server
            if (AuthManager.Instance != null)
            {
                AuthManager.Instance.StartCoroutine(AuthManager.Instance.SaveCharacterToServer(updatedJson));
            }
        }
        CharacterUIManager1.Instance.UpdateCharacterStatsAndUI();
        if (currentType == "Hair")
        {
            ShowEquipMessage("    Không thể gỡ bỏ", 2.5f);
        }
        ShowEquipMessage("Đã gỡ trang bị thành công!", 2.5f);
        Hide();
    }
  
    // ✅ THÊM TỪ CODE B  tuấn anh
    public bool IsVisible()
    {
        return panel != null && panel.activeSelf;
    }

    public bool IsShowingItem(string id)
    {
        return currentItemId == id;
    }

    //hiệu ứng text 
    public void ShowEquipMessage(string msg, float duration = 2.5f)
    {
        if (equipMessageCoroutine != null) StopCoroutine(equipMessageCoroutine);
        equipMessageCoroutine = StartCoroutine(FlyUpEquipMessage(msg, duration));
    }
    IEnumerator FlyUpEquipMessage(string msg, float duration)
    {
        // Set lại vị trí, scale, alpha ban đầu mỗi lần gọi
        equipMessageText.text = msg;
        var rect = equipMessageText.rectTransform;
        rect.anchoredPosition = equipMsgOriginPos;
        equipMessageText.color = new Color(1, 1, 1, 0);
        equipMessageText.transform.localScale = Vector3.one * 1.15f;

        // Fade in + scale in (0.15s)
        float t = 0f;
        while (t < 0.15f)
        {
            equipMessageText.color = new Color(1, 1, 1, Mathf.Lerp(0, 1, t / 0.15f));
            equipMessageText.transform.localScale = Vector3.Lerp(Vector3.one * 1.15f, Vector3.one, t / 0.15f);
            t += Time.deltaTime;
            yield return null;
        }
        equipMessageText.color = new Color(1, 1, 1, 1);
        equipMessageText.transform.localScale = Vector3.one;

        // Bay lên (move y lên), giữ trong duration-0.3s
        float moveTime = duration - 0.3f;
        float yStart = equipMsgOriginPos.y;
        float yEnd = yStart + 60f; // Bay lên 60 đơn vị pixel, tuỳ UI chỉnh lại số này
        t = 0f;
        while (t < moveTime)
        {
            float percent = t / moveTime;
            float y = Mathf.Lerp(yStart, yEnd, percent);
            rect.anchoredPosition = new Vector2(rect.anchoredPosition.x, y);
            t += Time.deltaTime;
            yield return null;
        }
        rect.anchoredPosition = new Vector2(rect.anchoredPosition.x, yEnd);

        // Fade out + scale out (0.15s)
        t = 0f;
        while (t < 0.15f)
        {
            equipMessageText.color = new Color(1, 1, 1, Mathf.Lerp(1, 0, t / 0.15f));
            equipMessageText.transform.localScale = Vector3.Lerp(Vector3.one, Vector3.one * 0.95f, t / 0.15f);
            t += Time.deltaTime;
            yield return null;
        }
        equipMessageText.text = "";
        equipMessageText.color = new Color(1, 1, 1, 0);
        rect.anchoredPosition = equipMsgOriginPos; // Reset lại vị trí cho lần sau
    }
}