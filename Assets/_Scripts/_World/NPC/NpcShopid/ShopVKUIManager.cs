using UnityEngine;

public class ShopVKUIManager : BaseShopUIManager
{
    public static ShopVKUIManager Instance { get; private set; }

    protected override void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
    }

    protected override ShopType CurrentShopType => ShopType.Weapon;
    protected override EquipmentSlotUI.ShopPanelType GetShopPanelType() => EquipmentSlotUI.ShopPanelType.ShopVK;
}