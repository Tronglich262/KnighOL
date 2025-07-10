using Assets.HeroEditor.FantasyInventory.Scripts.Interface.Elements;
using Newtonsoft.Json;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Networking;

public class InventoryManager : MonoBehaviour
{
    public static InventoryManager Instance;

    public InventoryUIManager uiManager; // gán từ scene
    public List<InventoryItem1> playerInventory = new List<InventoryItem1>();
    public List<InventoryItem1> equippedItemsList = new List<InventoryItem1>();

    [HideInInspector] public ClientSession session = new ClientSession();

    void Awake() => Instance = this;
    // --- KHI LOGIN THÀNH CÔNG, GỌI HÀM NÀY ---
    public void OnLoginSuccess(int accountId, string token)
    {
        session.AccountId = accountId;
        session.Token = token;
        playerInventory.Clear();
        equippedItemsList.Clear();
        LoadInventory(null);
    }
    public void AddItem(string itemId, int quantity)
    {
        var item = playerInventory.Find(i => i.itemId == itemId);
        if (item != null)
        {
            item.quantity += quantity;
            // --- LUÔN gọi cập nhật lên server ---
            SaveSingleItemToServer(itemId, item.quantity);   // <-- ADD dòng này!
        }
        else
        {
            var stats = Resources.LoadAll<ItemStats>("ItemStats");
            var data = System.Array.Find(stats, s => s.itemId == itemId);
            if (data == null) return;

            item = new InventoryItem1
            {
                itemId = itemId,
                quantity = quantity,
                stats = data
            };
            playerInventory.Add(item);
            SaveSingleItemToServer(itemId, quantity);    // Đã có rồi
        }

        uiManager.DisplayInventory(playerInventory); // cập nhật UI
    }

    public List<ItemStats> GetEquippedItems()
    {
        List<ItemStats> equipped = new List<ItemStats>();

        if (string.IsNullOrEmpty(PlayerDataHolder1.CharacterJson)) return equipped;

        var dict = JsonConvert.DeserializeObject<Dictionary<string, string>>(PlayerDataHolder1.CharacterJson);

        string[] keys = { "Helmet", "Armor", "Boots", "Gloves", "Pauldrons", "Vest", "Belt", "Shield", "Cape", "Back", "Glasses", "Hair", "Bow", "PrimaryMeleeWeapon", "SecondaryMeleeWeapon" };

        foreach (var key in keys)
        {
            if (dict.TryGetValue(key, out string itemId))
            {
                var stats = ItemDatabase.Instance.GetItemStatsById(itemId, key);
                if (stats != null)
                {
                    equipped.Add(stats);
                }
            }
        }
        foreach (var item in equipped)
        {
            //   Debug.Log($"[InventoryManager] 🧪 Trang bị: {item.Name}, Vitality: {item.Vitality}");
        }
        return equipped;

    }
    public void RemoveItem(string itemId, int quantity)
    {
        var item = playerInventory.Find(i => i.itemId == itemId);
        if (item != null)
        {
            item.quantity -= quantity;
            int newQuantity = item.quantity;
            if (item.quantity <= 0)
            {
                playerInventory.Remove(item);
                newQuantity = 0;
            }
            // LUÔN gọi cập nhật lên server với số lượng mới (0 nếu xóa)
            SaveSingleItemToServer(itemId, newQuantity);
        }
        else
        {
            Debug.LogWarning($"Không tìm thấy item {itemId} để remove.");
            // Không cần gửi lên server trong trường hợp này!
        }
        uiManager.DisplayInventory(playerInventory); // cập nhật lại UI sau khi remove
    }




    //load dữ liệu từ dtb xuống inventory 
    public void LoadInventory(System.Action<InventoryItemDto[]> onLoaded)
    {
        StartCoroutine(CoLoadInventory(onLoaded));
    }

    IEnumerator CoLoadInventory(System.Action<InventoryItemDto[]> onLoaded)
    {
        int accountId = session.AccountId;
        string token = session.Token;
        string url = $"https://localhost:7124/api/Account/inventory/{accountId}";

        UnityWebRequest req = UnityWebRequest.Get(url);
        req.SetRequestHeader("Authorization", "Bearer " + token);
        yield return req.SendWebRequest();

        if (req.result == UnityWebRequest.Result.Success)
        {
            InventoryItemDto[] items = JsonHelper.FromJson<InventoryItemDto>(req.downloadHandler.text);

            // === UPDATE LẠI playerInventory ===
            playerInventory.Clear();
            foreach (var item in items)
            {
                // item.itemId ở đây là int (DB trả về)
                // Lookup string itemId tương ứng trong ItemStatDatabase (hoặc 1 mapping riêng)
                string stringId = ItemStatDatabase.Instance.GetStringIdFromInt(item.itemId); // cần viết hàm này
                var so = ItemStatDatabase.Instance.GetStats(stringId);

                if (so != null)
                {
                    playerInventory.Add(new InventoryItem1
                    {
                        itemId = so.itemId,
                        quantity = item.quantity,
                        stats = so
                    });
                }
                else
                {
                    Debug.LogWarning($"Không tìm thấy ItemStats cho DB itemId int: {item.itemId}");
                }
            }


            // Cập nhật UI inventory
            uiManager.DisplayInventory(playerInventory);

            // Callback nếu có
            onLoaded?.Invoke(items);
        }
        else
        {
            Debug.LogError("Lỗi tải inventory: " + req.error);
            onLoaded?.Invoke(null);
        }
    }
    public void OnInventoryLoaded(InventoryItemDto[] items)
    {
        playerInventory.Clear();
        foreach (var dto in items)
        {
            // Lookup lại ScriptableObject theo itemId
            var itemStatsSO = ItemStatDatabase.Instance.GetStatsdtb(dto.itemId);
            if (itemStatsSO != null)
            {
                var invItem = new InventoryItem1
                {
                    itemId = itemStatsSO.itemId, // hoặc itemStatsSO.Item_ID nếu dùng int
                    quantity = dto.quantity,
                    stats = itemStatsSO
                };
                playerInventory.Add(invItem);
            }
            else
            {
                Debug.LogWarning($"Không tìm thấy ItemStats cho itemId: {dto.itemId}");
            }
        }
        // Cập nhật UI
        uiManager.DisplayInventory(playerInventory);
    }
    public void SaveSingleItemToServer(string itemId, int quantity)
    {
        StartCoroutine(CoSaveSingleItem(itemId, quantity));
    }

    IEnumerator CoSaveSingleItem(string itemId, int quantity)
    {
        int accountId = session.AccountId;
        string token = session.Token;
        string url = $"https://localhost:7124/api/Account/add-item";

        int parsedId = 0;
        if (!int.TryParse(itemId, out parsedId))
        {
            var stat = ItemStatDatabase.Instance.GetStats(itemId);
            if (stat != null) parsedId = stat.Item_ID;
            else
            {
                Debug.LogWarning($"Không chuyển được itemId string sang int: {itemId}");
                yield break;
            }
        }

        var dto = new AddItemDto
        {
            AccountId = accountId,
            ItemId = parsedId,
            Quantity = quantity
        };

        string json = JsonUtility.ToJson(dto);

        UnityWebRequest req = new UnityWebRequest(url, "POST");
        byte[] bodyRaw = System.Text.Encoding.UTF8.GetBytes(json);
        req.uploadHandler = new UploadHandlerRaw(bodyRaw);
        req.downloadHandler = new DownloadHandlerBuffer();
        req.SetRequestHeader("Content-Type", "application/json");
        req.SetRequestHeader("Authorization", "Bearer " + token);

        yield return req.SendWebRequest();

        if (req.result == UnityWebRequest.Result.Success)
        {
            Debug.Log($"Đã lưu item {dto.ItemId} với số lượng {dto.Quantity} lên server");
        }
        else
        {
            Debug.LogError($"Lỗi lưu item {dto.ItemId} lên server: " + req.error);
        }
    }

    [System.Serializable]
    public class AddItemDto
    {
        public int AccountId;
        public int ItemId;
        public int Quantity;
    }
    // DTO class
    [System.Serializable]
    public class SaveInventoryDto
    {
        public int AccountId;
        public InventoryItemDto[] Items;
    }

    [System.Serializable]
    public class InventoryItemDto
    {
        public int itemId;
        public int quantity;
    }


}
