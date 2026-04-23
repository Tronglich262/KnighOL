using UnityEngine;

public class ShopPKUIManager : BaseShopUIManager
{
    public static ShopPKUIManager Instance { get; private set; }

    protected override void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
    }

    protected override ShopType CurrentShopType => ShopType.Consumable;
    protected override EquipmentSlotUI.ShopPanelType GetShopPanelType() => EquipmentSlotUI.ShopPanelType.ShopPK;
}