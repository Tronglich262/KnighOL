using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InventoryManager : MonoBehaviour
{
    public static InventoryManager Instance;

    public InventoryUIManager uiManager;

    public List<InventoryItem1> playerInventory = new List<InventoryItem1>();
    public List<InventoryItem1> equippedItemsList = new List<InventoryItem1>();

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    // Gọi sau khi Login thành công
    public void OnLoginSuccess()
    {
        playerInventory.Clear();
        equippedItemsList.Clear();
        LoadInventory(null);
    }

    // ====================== LOAD INVENTORY ======================
    public void LoadInventory(Action<InventoryItemDto[]> onLoaded)
    {
        StartCoroutine(CoLoadInventory(onLoaded));
    }

    private IEnumerator CoLoadInventory(Action<InventoryItemDto[]> onLoaded)
    {
        yield return ApiClientBase.Instance.Get<InventoryItemDto[]>(
            $"Account/inventory/{SessionManager.AccountId}",
            items =>
            {
                playerInventory.Clear();
                foreach (var item in items)
                {
                    string stringId = ItemStatDatabase.Instance.GetStringIdFromInt(item.itemId);
                    var stats = ItemStatDatabase.Instance.GetStats(stringId);

                    if (stats != null)
                    {
                        playerInventory.Add(new InventoryItem1
                        {
                            itemId = stats.itemId,
                            quantity = item.quantity,
                            stats = stats
                        });
                    }
                    else
                    {
                        Debug.LogWarning($"Không tìm thấy ItemStats cho itemId: {item.itemId}");
                    }
                }

                uiManager?.DisplayInventory(playerInventory);
                onLoaded?.Invoke(items);
                Debug.Log($"[Inventory] Load xong {playerInventory.Count} items");
            },
            error => Debug.LogError("Lỗi load inventory: " + error)
        );
    }

    // ====================== ADD / REMOVE ITEM ======================
    public void AddItem(string itemId, int quantity)
    {
        var item = playerInventory.Find(i => i.itemId == itemId);
        if (item != null)
        {
            item.quantity += quantity;
            SaveSingleItemToServer(item.itemId, item.quantity);
        }
        else
        {
            var stats = ItemStatDatabase.Instance.GetStats(itemId);
            if (stats == null) return;

            item = new InventoryItem1 { itemId = itemId, quantity = quantity, stats = stats };
            playerInventory.Add(item);
            SaveSingleItemToServer(itemId, quantity);
        }

        uiManager?.DisplayInventory(playerInventory);
    }

    public void RemoveItem(string itemId, int quantity)
    {
        var item = playerInventory.Find(i => i.itemId == itemId);
        if (item == null) return;

        item.quantity -= quantity;
        if (item.quantity <= 0)
        {
            playerInventory.Remove(item);
            quantity = 0;
        }

        SaveSingleItemToServer(itemId, item.quantity);
        uiManager?.DisplayInventory(playerInventory);
    }

    // ====================== SAVE TO SERVER ======================
    public void SaveSingleItemToServer(string itemId, int quantity)
    {
        StartCoroutine(CoSaveSingleItem(itemId, quantity));
    }

    private IEnumerator CoSaveSingleItem(string itemId, int quantity)
    {
        int parsedId = 0;
        if (!int.TryParse(itemId, out parsedId))
        {
            var stat = ItemStatDatabase.Instance.GetStats(itemId);
            if (stat != null) parsedId = stat.Item_ID;
            else
            {
                Debug.LogWarning($"Không convert được itemId: {itemId}");
                yield break;
            }
        }

        var dto = new AddItemDto
        {
            AccountId = SessionManager.AccountId,
            ItemId = parsedId,
            Quantity = quantity
        };

        yield return ApiClientBase.Instance.Post<object>(
            "Account/add-item",
            dto,
            _ => Debug.Log($"Đã lưu item {itemId} x{quantity} lên server"),
            error => Debug.LogError("Lỗi save item: " + error)
        );
    }

    // ====================== DTOs ======================
    [System.Serializable]
    public class AddItemDto
    {
        public int AccountId;
        public int ItemId;
        public int Quantity;
    }

    [System.Serializable]
    public class InventoryItemDto
    {
        public int itemId;
        public int quantity;
    }
}