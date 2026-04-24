public class ShopTriggerTP : BaseShopTrigger
{
    public static ShopTriggerTP Instance { get; private set; }

    protected override int NpcId => 2;
    protected override BaseShopUIManager ShopUIManager => ShopTPUIManager.Instance;

    private void Awake()
    {
        Instance = this;
        base.Awake();
    }

    public void OnClickVestTab() => ShopTPUIManager.Instance.StartFilterShopByType("Vest");
    public void OnClickPauldronsTab() => ShopTPUIManager.Instance.StartFilterShopByType("Pauldrons");
    public void OnClickGlovesTab() => ShopTPUIManager.Instance.StartFilterShopByType("Gloves");
    public void OnClickBeltTab() => ShopTPUIManager.Instance.StartFilterShopByType("Belt");
    public void OnClickBootsTab() => ShopTPUIManager.Instance.StartFilterShopByType("Boots");
}