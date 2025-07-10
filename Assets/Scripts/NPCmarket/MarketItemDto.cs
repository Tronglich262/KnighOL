[System.Serializable]
public class MarketItemDto
{
    public int marketItem_ID;
    public int seller_Account_ID;
    public int SellerAccountId;  // nếu backend cần, hoặc có thể bỏ
    public int item_ID;
    public int quantity;
    public int price;
    public string createdAt;
    public string itemName;
    public string itemType;
    public string itemDescription;
    public string itemRarity;
    // Nếu backend trả về các trường này thì thêm:
    public int? strength;
    public int? defense;
    public int? agility;
    public int? intelligence;
    public int? vitality;
}