using UnityEngine;

[System.Serializable]
public class InventoryItem1
{
    public string itemId;
    public int quantity;

    [System.NonSerialized] public ItemStats stats;

    public string itemName => stats != null ? stats.Name : "";
    public Sprite icon => stats != null ? stats.Icon : null;
}
