public static class SessionManager
{
    public static int AccountId { get; private set; }
    public static string Token { get; private set; }
    public static string PlayerName { get; private set; }

    public static void SetSession(int accountId, string token, string playerName = null)
    {
        AccountId = accountId;
        Token = token;
        PlayerName = playerName ?? PlayerName;

        PlayerDataHolder1.AccountId = accountId;
        PlayerDataHolder1.Token = token;

        if (!string.IsNullOrEmpty(playerName))
            PlayerDataHolder1.PlayerName = playerName;
    }

    public static void Clear()
    {
        AccountId = 0;
        Token = null;
        PlayerName = null;

        PlayerDataHolder1.AccountId = 0;
        PlayerDataHolder1.Token = null;
        PlayerDataHolder1.PlayerName = null;
    }

    public static bool HasValidSession()
    {
        return AccountId > 0 && !string.IsNullOrEmpty(Token);
    }
}