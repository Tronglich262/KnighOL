using UnityEngine;

public class ItemDatabase1 : MonoBehaviour
{
    public static ItemDatabase1 Instance;

    private ItemStats[] allItems;

    void Awake()
    {
        Instance = this;
        allItems = Resources.LoadAll<ItemStats>("ItemStats");
    }

    public ItemStats GetItemById(string id)
    {
        foreach (var item in allItems)
        {
            if (item.itemId == id)
                return item;
        }

        Debug.LogWarning($"❌ Không tìm thấy ItemId: {id}");
        return null;
    }
    public ItemStats[] GetAll()
    {
        return allItems;
    }

}
