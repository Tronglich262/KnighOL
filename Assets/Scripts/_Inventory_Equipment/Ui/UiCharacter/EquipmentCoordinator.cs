using UnityEngine;

public static class EquipmentCoordinator
{
    public static bool Equip(InventoryItem1 item, out string message)
    {
        message = null;

        if (item == null || item.stats == null)
        {
            message = "Item không hợp lệ.";
            return false;
        }

        var ui = CharacterUIManager1.Instance;
        if (ui == null || ui.character == null)
        {
            message = "Character UI chưa sẵn sàng.";
            return false;
        }

        var state = EquipmentState.FromCurrent();
        state.Equip(item.stats.Type, item.itemId);

        string finalJson = state.ToJson();
        Commit(finalJson);

        message = "Trang bị thành công.";
        return true;
    }

    public static bool Unequip(string type, out string message)
    {
        message = null;

        var ui = CharacterUIManager1.Instance;
        if (ui == null || ui.character == null)
        {
            message = "Character UI chưa sẵn sàng.";
            return false;
        }

        var state = EquipmentState.FromCurrent();

        if (!state.CanUnequip(type))
        {
            if (type == EquipKeys.Hair)
                message = "Không thể gỡ bỏ tóc.";
            else
                message = "Vũ khí chỉ có thể thay thế, không thể gỡ bỏ.";

            return false;
        }

        string equippedItemId = state.Get(type);
        if (string.IsNullOrEmpty(equippedItemId))
        {
            message = "Không có trang bị để gỡ.";
            return false;
        }

        state.Unequip(type);
        string finalJson = state.ToJson();

        Commit(finalJson);

        if (InventoryManager.Instance != null)
            InventoryManager.Instance.AddItem(equippedItemId, 1);

        message = "Đã gỡ trang bị thành công.";
        return true;
    }

    private static void Commit(string finalJson)
    {
        EquipmentSyncService.ApplyFullJson(
            finalJson,
            ItemDetailsUI.Instance != null ? ItemDetailsUI.Instance.playerClone : null
        );

        var ui = CharacterUIManager1.Instance;
        if (ui != null)
        {
            ui.RefreshFromLatestJson();
            ui.UpdateCharacterStatsAndUI();
        }

        if (PlayerSpawner.LocalPlayerObject != null)
        {
            var equipStat = PlayerSpawner.LocalPlayerObject.GetComponent<EquipmentStatManager>();
            if (equipStat != null)
                equipStat.LoadFromCharacterJson(finalJson);
        }

        if (InventoryManager.Instance != null && InventoryUIManager.instance != null)
        {
            InventoryUIManager.instance.DisplayInventory(InventoryManager.Instance.playerInventory);
        }
    }
}