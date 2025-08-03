using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class ItemDaily : MonoBehaviour
{
    public Transform contentParent;

    private List<Transform> itemSlots = new List<Transform>();

    void Awake()
    {
        
        foreach (Transform child in contentParent)
        {
            itemSlots.Add(child);
        }
    }

    private void Start()
    {
        ShowAllItems();
    }
    
    public void ShowAllItems()
    {
        List<ItemStats> allItems = ItemStatDatabase.Instance.GetAll();
        Debug.Log("Total items loaded: " + allItems.Count);

        for (int i = 0; i < itemSlots.Count; i++)
        {
            var image = itemSlots[i].GetComponentInChildren<Image>();
            if (image == null)
            {
                Debug.LogWarning("No Image in slot: " + itemSlots[i].name);
                continue;
            }

            if (i < allItems.Count)
            {
                if (allItems[i].Icon == null)
                {
                    Debug.LogWarning("Missing icon for item: " + allItems[i].itemId);
                }

                image.sprite = allItems[i].Icon;
                image.color = Color.white;

                var equipUI = itemSlots[i].GetComponent<EquipmentSlotUI>();
                if (equipUI != null)
                {
                    equipUI.SetItem(allItems[i].itemId, allItems[i].Icon, allItems[i].Type);
                }
            }
            else
            {
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