using Newtonsoft.Json;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MarketShopUI : MonoBehaviour
{
    public static MarketShopUI Instance;

    public Transform Content;
    public GameObject MarketItemRowPrefab;

    private float reloadInterval = 15f;
    private float reloadTimer;

    private void Awake()
    {
        Instance = this;
    }

    void Update()
    {
        reloadTimer += Time.deltaTime;
        if (reloadTimer >= reloadInterval)
        {
            LoadMarketItems();
            reloadTimer = 0;
        }
    }

    private void OnEnable()
    {
        LoadMarketItems();
    }

    public void LoadMarketItems()
    {
        StartCoroutine(CoLoadMarketItems());
    }

    private IEnumerator CoLoadMarketItems()
    {
        yield return ApiClientBase.Instance.Get<MarketItemDto[]>(
            "Account/market/all",
            items =>
            {
                // Xóa UI cũ
                foreach (Transform child in Content)
                    Destroy(child.gameObject);

                foreach (var item in items)
                {
                    var row = Instantiate(MarketItemRowPrefab, Content);
                    var rowUI = row.GetComponent<MarketItemRowUI>();

                    if (rowUI == null)
                    {
                        Debug.LogError("MarketItemRowUI component not found on prefab!");
                        continue;
                    }

                    var stats = ItemStatDatabase.Instance.GetStatsdtb(item.item_ID);
                    if (stats == null)
                    {
                        Debug.LogWarning($"Không tìm thấy stats cho market item ID: {item.item_ID}");
                        continue;
                    }

                    rowUI.SetData(item, stats);
                }

                Debug.Log($"[MarketShopUI] Load xong {items.Length} items từ market");
            },
            error => Debug.LogError("Lỗi load market: " + error)
        );
    }
}