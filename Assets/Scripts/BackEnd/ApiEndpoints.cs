public static class ApiEndpoints
{
    public static class Account
    {
        public const string Register = "Account/register";
        public const string Login = "Account/login";
        public const string Refresh = "Account/refresh";

        public static string Inventory(int accountId) => $"Account/inventory/{accountId}";
        public const string AddItem = "Account/add-item";

        public const string GetQuests = "Account/quests";
        public const string ClaimQuest = "Account/quests/claim";

        public const string MarketAll = "Account/market/all";
        public const string MarketBuy = "Account/market/buy";

        public static string NpcShop(int npcId) => $"Account/npc-shop/{npcId}";
    }
}