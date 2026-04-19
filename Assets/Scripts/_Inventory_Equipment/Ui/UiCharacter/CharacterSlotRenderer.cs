using UnityEngine;

public static class CharacterSlotRenderer
{
    public static ItemStats DisplaySlot(
        GameObject slot,
        string itemPath,
        string expectedType)
    {
        if (slot == null || string.IsNullOrEmpty(itemPath))
        {
            CharacterEquipmentHelper.ClearSlotUI(slot);
            return null;
        }

        string cleanId = CharacterEquipmentHelper.GetCleanId(itemPath);
        string itemName = CharacterEquipmentHelper.GetLastToken(cleanId);

        var icon = CharacterEquipmentHelper.FindIcon(cleanId, expectedType);
        if (icon == null)
        {
            CharacterEquipmentHelper.ClearSlotUI(slot);
            return null;
        }

        CharacterEquipmentHelper.SetSlotUI(slot, itemName, icon.Sprite, icon.Id, icon.Type);

        return ItemDatabase.Instance.GetItemStatsById(
            CharacterEquipmentHelper.GetLastToken(icon.Id),
            icon.Type
        );
    }
}