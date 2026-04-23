using UnityEngine;

public class ShopTPUIManager : BaseShopUIManager
{
    public static ShopTPUIManager Instance { get; private set; }

    protected override void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
    }

    protected override ShopType CurrentShopType => ShopType.Accessory;
    protected override EquipmentSlotUI.ShopPanelType GetShopPanelType() => EquipmentSlotUI.ShopPanelType.ShopTP;
}