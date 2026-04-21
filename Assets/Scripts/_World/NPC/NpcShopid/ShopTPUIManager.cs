using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class ShopTPUIManager : BaseShopUIManager
{
    public static ShopTPUIManager Instance { get; private set; }

    protected override void Awake()
    {
        base.Awake();
        Instance = this;
    }

    protected override List<NpcShopItem> FilterItemsByCurrentType()
    {
        return allShopItems.Where(x => x.type == "Cape").ToList();
    }

    protected override EquipmentSlotUI.ShopPanelType GetShopPanelType()
    {
        return EquipmentSlotUI.ShopPanelType.ShopTP;
    }
}