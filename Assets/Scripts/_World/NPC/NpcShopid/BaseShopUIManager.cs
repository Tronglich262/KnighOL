using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public abstract class BaseShopUIManager : MonoBehaviour
{
    // Mỗi Shop Manager sẽ có Instance riêng
    protected static BaseShopUIManager _instance;

    [Header("Shop UI References")]
    public Transform contentParent;
    public GameObject shopItemPrefab;

    protected List<GameObject> currentShopItemUIs = new List<GameObject>();
    protected List<NpcShopItem> allShopItems = new List<NpcShopItem>();

    protected virtual void Awake()
    {
        // Không destroy panel vì đây là UI riêng biệt
        Debug.Log($"[{GetType().Name}] Awake() - Panel đã active.");
    }

    public virtual IEnumerator ShowShop(List<NpcShopItem> items)
    {
        Debug.Log($"[{GetType().Name}] Nhận {items?.Count ?? 0} items");
        allShopItems = items ?? new List<NpcShopItem>();

        var filtered = FilterItemsByCurrentType();
        yield return StartCoroutine(DisplayFilteredItems(filtered));
    }

    protected abstract List<NpcShopItem> FilterItemsByCurrentType();

    protected virtual IEnumerator DisplayFilteredItems(List<NpcShopItem> items)
    {
        ClearShopUI();

        foreach (var item in items)
        {
            CreateShopItemUI(item);
            yield return null;
        }
    }

    public void StartFilterShopByType(string type)
    {
        StopAllCoroutines();
        StartCoroutine(FilterShopByTypeCoroutine(type));
    }

    private IEnumerator FilterShopByTypeCoroutine(string type)
    {
        var filtered = allShopItems.Where(x => x.type == type).ToList();
        Debug.Log($"[{GetType().Name}] Filter loại: {type} → {filtered.Count} items");

        yield return StartCoroutine(DisplayFilteredItems(filtered));
    }

    protected virtual void ClearShopUI()
    {
        foreach (var obj in currentShopItemUIs)
            Destroy(obj);
        currentShopItemUIs.Clear();
    }

    protected virtual void CreateShopItemUI(NpcShopItem item)
    {
        if (item == null) return;

        var stats = Resources.LoadAll<ItemStats>("ItemStats")
                             .FirstOrDefault(x => x.Item_ID == item.itemId);

        if (stats == null)
        {
            Debug.LogWarning($"Không tìm thấy stats cho itemId: {item.itemId}");
            return;
        }

        var obj = Instantiate(shopItemPrefab, contentParent);
        var slotUI = obj.GetComponent<EquipmentSlotUI>();

        slotUI.SetItem(stats.itemId, stats.Icon, item.type, item.price);
        slotUI.npcShopItemData = item;
        slotUI.shopPanelType = GetShopPanelType();

        var iconImg = obj.transform.Find("Icon")?.GetComponent<Image>();
        if (iconImg != null && stats.Icon != null)
            iconImg.sprite = stats.Icon;

        currentShopItemUIs.Add(obj);
    }

    protected abstract EquipmentSlotUI.ShopPanelType GetShopPanelType();
}