//using ApiLogin.modelAccount;
using System.Collections.Generic;
using Unity.VisualScripting.Antlr3.Runtime.Misc;
using UnityEngine;
using UnityEngine.Networking;
//using ApiLogin.modelAccount;

public class MarketShopUI : MonoBehaviour
{
    public Transform Content; // Gán Content của Scroll View ở Inspector
    public GameObject MarketItemRowPrefab;
    public static MarketShopUI Instance; // Singleton instance

    private float reloadInterval = 15f;
    private float reloadTimer;
    public void Awake()
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
        StartCoroutine(GetMarketItems());
    }

    IEnumerator<UnityWebRequestAsyncOperation> GetMarketItems()
    {
        string url = "https://localhost:7124/api/Account/market/all";
        UnityWebRequest req = UnityWebRequest.Get(url);
        // Nếu cần token thì add header
        // req.SetRequestHeader("Authorization", "Bearer ...");

        yield return req.SendWebRequest();

        if (req.result == UnityWebRequest.Result.Success)
        {
            // Parse về mảng object
            Debug.Log("Market JSON: " + req.downloadHandler.text);
          var items = JsonArrayHelper.FromJson<MarketItemDto>(req.downloadHandler.text);

            // Xoá cũ
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
                    continue;
                }

                rowUI.SetData(item, stats);
            }





        }
        else
        {
            Debug.LogError("Lỗi tải market: " + req.error);
        }
    }
}
