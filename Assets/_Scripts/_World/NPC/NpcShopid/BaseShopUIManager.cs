using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public abstract class BaseShopUIManager : MonoBehaviour
{
    [Header("Shop UI References")]
    public Transform contentParent;
    public GameObject shopItemPrefab;

    protected List<NpcShopItem> allShopItems = new List<NpcShopItem>();
    protected List<GameObject> currentUIs = new List<GameObject>();

    protected abstract ShopType CurrentShopType { get; }
    protected abstract EquipmentSlotUI.ShopPanelType GetShopPanelType();

    protected virtual void Awake() { }

    // ====================== Má»ž SHOP â†’ AUTO TAB Äáº¦U TIÃŠN ======================
    public virtual IEnumerator ShowShop(List<NpcShopItem> items)
    {
        allShopItems = items ?? new List<NpcShopItem>();

        string defaultType = GetDefaultFilterType();
        Debug.Log($"[Shop {CurrentShopType}] Mo shop -> AUTO filter tab dau tien: {defaultType} | Tong {allShopItems.Count} items");

        yield return StartCoroutine(FilterShopByTypeCoroutine(defaultType));
    }

    protected virtual string GetDefaultFilterType()
    {
        return CurrentShopType switch
        {
            ShopType.Weapon => "Weapon",
            ShopType.Consumable => "Consumable",
            ShopType.Accessory => "Accessory",
            _ => "Weapon"
        };
    }

    public void StartFilterShopByType(string type)
    {
        StopAllCoroutines();
        StartCoroutine(FilterShopByTypeCoroutine(type));
    }

    private IEnumerator FilterShopByTypeCoroutine(string type)
    {
        var filtered = allShopItems
            .Where(x => x.type.Equals(type, System.StringComparison.OrdinalIgnoreCase))
            .ToList();

        Debug.Log($"[Filter] Type '{type}' -> Tim thay {filtered.Count} items");
        yield return StartCoroutine(DisplayFilteredItems(filtered));
    }

    protected virtual IEnumerator DisplayFilteredItems(List<NpcShopItem> items)
    {
        ClearUI();
        foreach (var item in items)
        {
            CreateShopItemUI(item);
            yield return null;
        }
    }

    protected virtual void CreateShopItemUI(NpcShopItem item)
    {
        if (item == null) return;

        ItemStats stats = ItemStatDatabase.GetOrCreate().GetStats(item.itemId.ToString());
        if (stats == null)
            stats = ItemStatDatabase.GetOrCreate().GetStatsdtb(item.itemId);

        if (stats == null)
        {
            Debug.LogWarning($"Khong tim thay stats cho itemId: {item.itemId}");
            return;
        }

        GameObject obj = Instantiate(shopItemPrefab, contentParent);
        var slotUI = obj.GetComponent<EquipmentSlotUI>();

        slotUI.SetItem(stats.itemId, stats.Icon, item.type, item.price);
        slotUI.npcShopItemData = item;
        slotUI.shopPanelType = GetShopPanelType();

        currentUIs.Add(obj);
    }

    public void ClearUI()
    {
        foreach (var ui in currentUIs)
            if (ui != null) Destroy(ui);
        currentUIs.Clear();
    }
}