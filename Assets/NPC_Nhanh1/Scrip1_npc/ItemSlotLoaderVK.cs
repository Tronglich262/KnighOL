using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class ItemSlotLoaderVK : MonoBehaviour
{
    public Transform contentParent1; // Content chứa các slot (item, item(1), ...)


    public string itemTypeToShow;   // Loại item cần hiển thị: "Áo", "Giáp vai", v.v.

    private List<Transform> itemSlots = new List<Transform>();

    void Awake()
    {
        // Cache các slot con trong Content
       
        foreach (Transform child in contentParent1)
        {
            itemSlots.Add(child);
        }

    }

    /// <summary>
    /// Gọi hàm này khi nhấn vào button như "Áo", "Giáp vai"
    /// </summary>
    /// <param name="type">Loại item: "Áo", "Giày", "Giáp vai", ...</param>
    public void ShowItemsByType(string type)
    {
        itemTypeToShow = type;

        // Lấy toàn bộ item có type phù hợp từ database
        List<ItemStats> filteredItems = new List<ItemStats>();
        foreach (var stat in ItemStatDatabase.Instance.GetAll())
        {
            if (stat.Type == itemTypeToShow)
                filteredItems.Add(stat);
        }

        // Gán icon vào từng ô
        for (int i = 0; i < itemSlots.Count; i++)
        {
            var image = itemSlots[i].GetComponentInChildren<Image>();
            if (image == null) continue;

            if (i < filteredItems.Count)
            {
                image.sprite = filteredItems[i].Icon;
                image.color = Color.white;

                // Gán thêm thông tin nếu cần
                var equipUI = itemSlots[i].GetComponent<EquipmentSlotUI>();
                if (equipUI != null)
                {
                    equipUI.SetItem(filteredItems[i].itemId, filteredItems[i].Icon, filteredItems[i].Type);
                }
            }
            else
            {
                // Reset ô nếu không có item
                image.sprite = null;
                image.color = Color.clear;

                var equipUI = itemSlots[i].GetComponent<EquipmentSlotUI>();
                if (equipUI != null)
                {
                    equipUI.SetItem(null, null);
                }
            }
        }
    }
}