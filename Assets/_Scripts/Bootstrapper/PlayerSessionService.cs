using UnityEngine;

public class PlayerSessionService : MonoBehaviour
{
    public static PlayerSessionService Instance { get; private set; }

    // Data
    public int AccountId { get; private set; }
    public string Token { get; private set; }
    public string RefreshToken { get; private set; }
    public string PlayerName { get; private set; }

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    public void Initialize()
    {
        Debug.Log("[PlayerSessionService] Initialized");
    }

    public void SetSession(int accountId, string token, string playerName = null, string refreshToken = null)
    {
        AccountId = accountId;
        Token = token;
        RefreshToken = refreshToken;
        PlayerName = playerName;

        // Đồng bộ với PlayerDataHolder1 (giữ tương thích)
        PlayerDataHolder1.AccountId = accountId;
        PlayerDataHolder1.Token = token;
        if (!string.IsNullOrEmpty(playerName))
            PlayerDataHolder1.PlayerName = playerName;
    }

    public void ClearSession()
    {
        AccountId = 0;
        Token = null;
        RefreshToken = null;
        PlayerName = null;

        // Clear PlayerDataHolder1 an toàn (không gọi Clear() vì nó không tồn tại)
        PlayerDataHolder1.AccountId = 0;
        PlayerDataHolder1.Token = null;
        PlayerDataHolder1.PlayerName = null;

        Debug.Log("[PlayerSessionService] Session đã được clear");
    }

    public bool HasValidSession() => AccountId > 0 && !string.IsNullOrEmpty(Token);
}