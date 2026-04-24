public class ShopTriggerPK : BaseShopTrigger
{
    public static ShopTriggerPK Instance { get; private set; }

    protected override int NpcId => 1;
    protected override BaseShopUIManager ShopUIManager => ShopPKUIManager.Instance;

    private void Awake()
    {
        Instance = this;
        base.Awake();
    }

    public void OnClickCapeTab() => ShopPKUIManager.Instance.StartFilterShopByType("Cape");
    public void OnClickMaskTab() => ShopPKUIManager.Instance.StartFilterShopByType("Mask");
    public void OnClickGlassesTab() => ShopPKUIManager.Instance.StartFilterShopByType("Glasses");
    public void OnClickHairTab() => ShopPKUIManager.Instance.StartFilterShopByType("Hair");
    public void OnClickBackTab() => ShopPKUIManager.Instance.StartFilterShopByType("Back");
}