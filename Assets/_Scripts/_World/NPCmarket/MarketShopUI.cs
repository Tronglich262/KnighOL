using Newtonsoft.Json;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MarketShopUI : MonoBehaviour
{
    public static MarketShopUI Instance;

    [Header("UI References")]
    public Transform Content;                    // Parent chứa các row
    public GameObject MarketItemRowPrefab;       // Prefab của một hàng item

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
        // Khi panel Market được bật → load dữ liệu một lần
        LoadMarketItems();
    }

    private void OnDisable()
    {
        // Khi tắt panel → dọn dẹp UI để tránh rò rỉ memory
        ClearAllItems();
    }

    /// <summary>
    /// Load danh sách item từ server
    /// </summary>
    public void LoadMarketItems()
    {
        if (isLoading) return;
        isLoading = true;

        StartCoroutine(CoLoadMarketItems());
    }

    /// <summary>
    /// Gọi từ nút "Làm mới" trên UI
    /// </summary>
    public void RefreshMarket()
    {
        ClearAllItems();        // Xóa UI cũ trước
        LoadMarketItems();
    }

    private IEnumerator CoLoadMarketItems()
    {
        yield return ApiClientBase.Instance.Get<MarketItemDto[]>(
            "Account/market/all",
            items =>
            {
                ClearAllItems();   

                foreach (var item in items)
                {
                    CreateMarketItemRow(item);
                }

                Debug.Log($"[MarketShopUI] Load thành công {items.Length} items từ market.");
                isLoading = false;
            },
            error =>
            {
                Debug.LogError("Lỗi load market: " + error);
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
            var stats = ItemStatDatabase.Instance.GetStatsdtb(item.item_ID);
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