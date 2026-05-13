using Newtonsoft.Json;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MarketShopUI : MonoBehaviour
{
    public static MarketShopUI Instance;

    [Header("UI References")]
    public Transform Content;                    // Parent ch?a các row
    public GameObject MarketItemRowPrefab;       // Prefab c?a m?t hàng item

    private bool isLoading = false;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    private void OnEnable()
    {
        // Khi panel Market du?c b?t ? load d? li?u m?t l?n
        LoadMarketItems();
    }

    private void OnDisable()
    {
        // Khi t?t panel ? d?n d?p UI d? tránh rò r? memory
        ClearAllItems();
    }

    /// <summary>
    /// Load danh sách item t? server
    /// </summary>
    public void LoadMarketItems()
    {
        if (isLoading) return;
        isLoading = true;

        StartCoroutine(CoLoadMarketItems());
    }

    /// <summary>
    /// G?i t? nút "Làm m?i" trên UI
    /// </summary>
    public void RefreshMarket()
    {
        ClearAllItems();        // Xóa UI cu tru?c
        LoadMarketItems();
    }

    private IEnumerator CoLoadMarketItems()
    {
        yield return ApiClientBase.GetOrCreate().Get<MarketItemDto[]>(
            "Account/market/all",
            items =>
            {
                ClearAllItems();   

                foreach (var item in items)
                {
                    CreateMarketItemRow(item);
                }

                Debug.Log($"[MarketShopUI] Loaded {items.Length} items from market.");
                isLoading = false;
            },
            error =>
            {
                Debug.LogError("Load market failed: " + error);
                isLoading = false;
            }
        );
    }

    private void CreateMarketItemRow(MarketItemDto item)
    {
        if (MarketItemRowPrefab == null || Content == null) return;

        GameObject rowObj = Instantiate(MarketItemRowPrefab, Content);
        MarketItemRowUI rowUI = rowObj.GetComponent<MarketItemRowUI>();

        if (rowUI != null)
        {
            var stats = ItemStatDatabase.GetOrCreate().GetStatsdtb(item.item_ID);
            rowUI.SetData(item, stats);
        }
        else
        {
            Debug.LogWarning("MarketItemRowUI component missing on prefab!");
        }
    }

    private void ClearAllItems()
    {
        foreach (Transform child in Content)
        {
            Destroy(child.gameObject);
        }
    }
}
