using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class ShopPKUIManager : MonoBehaviour
{
    public static ShopPKUIManager Instance;
    public Transform contentParent;
    public GameObject shopItemPrefab;
    private List<GameObject> currentShopItemUIs = new List<GameObject>();
    private List<NpcShopItem> allShopItems = new List<NpcShopItem>();

    private void Awake()
    {
        Instance = this;
    }

    public IEnumerator ShowShop(List<NpcShopItem> items)
    {
        Debug.Log("ShowShop nhận " + (items != null ? items.Count : 0) + " items");
        foreach (var i in items)
            Debug.Log($"Item: {i.itemId}, Name: {i.name}, Type: {i.type}, Price: {i.price}");

        allShopItems = items;

        var filtered = allShopItems.Where(x => x.type == "Cape").ToList();
        ClearShopUI();

        foreach (var item in filtered)
        {
            CreateShopItemUI(item);
            yield return null; // 💡 tránh lag
        }
    }

    public void StartFilterShopByType(string type)
    {
        StopAllCoroutines(); // Dừng nếu người chơi đổi tab nhanh
        StartCoroutine(FilterShopByTypeCoroutine(type));
    }

    private IEnumerator FilterShopByTypeCoroutine(string type)
    {
        var filtered = allShopItems.Where(x => x.type == type).ToList();
        Debug.Log("Filter loại: " + type + ", có " + filtered.Count + " item");

        ClearShopUI();

        foreach (var item in filtered)
        {
            CreateShopItemUI(item);
            yield return null; // 💡 Cho Unity nghỉ 1 frame giữa các item
        }
    }


    private void ClearShopUI()
    {
        foreach (var obj in currentShopItemUIs)
            Destroy(obj);
        currentShopItemUIs.Clear();
    }

    private void CreateShopItemUI(NpcShopItem item)
    {
        Debug.Log("Tạo UI cho item: " + item.itemId + " - Type: " + item.type);

        var statsList = Resources.LoadAll<ItemStats>("ItemStats");
        var stats = statsList.FirstOrDefault(x => x.Item_ID == item.itemId);

        var obj = Instantiate(shopItemPrefab, contentParent);
        if (obj == null) { Debug.LogError("shopItemPrefab NULL hoặc chưa gán!"); return; }

        var slotUI = obj.GetComponent<EquipmentSlotUI>();
        slotUI.SetItem(stats.itemId, stats.Icon, item.type, item.price);
        slotUI.npcShopItemData = item;
        slotUI.shopPanelType = EquipmentSlotUI.ShopPanelType.ShopPK;

        var iconTrans = obj.transform.Find("Icon");
        var iconImg = iconTrans?.GetComponent<Image>();
        if (iconImg != null && stats?.Icon != null)
            iconImg.sprite = stats.Icon;
        else
            Debug.LogWarning($"Không tìm thấy ItemStats hoặc Icon cho itemId {item.itemId}");

        currentShopItemUIs.Add(obj);
    }
}