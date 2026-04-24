public class ShopTriggerVK : BaseShopTrigger
{
    public static ShopTriggerVK Instance { get; private set; }

    protected override int NpcId => 3;
    protected override BaseShopUIManager ShopUIManager => ShopVKUIManager.Instance;

    private void Awake()
    {
        Instance = this;
        base.Awake();
    }

    public void OnClickMeleeWeapon1HTab() => ShopVKUIManager.Instance.StartFilterShopByType("MeleeWeapon1H");
    public void OnClickMeleeWeapon2HTab() => ShopVKUIManager.Instance.StartFilterShopByType("MeleeWeapon2H");
    public void OnClickBowTab() => ShopVKUIManager.Instance.StartFilterShopByType("Bow");
    public void OnClickShieldTab() => ShopVKUIManager.Instance.StartFilterShopByType("Shield");
    public void OnClickHelmetTab() => ShopVKUIManager.Instance.StartFilterShopByType("Helmet");
}