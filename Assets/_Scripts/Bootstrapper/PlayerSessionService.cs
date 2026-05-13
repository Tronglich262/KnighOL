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

    public static PlayerSessionService GetOrCreate()
    {
        if (Instance != null)
            return Instance;

        var go = new GameObject("PlayerSessionService");
        return go.AddComponent<PlayerSessionService>();
    }

    public void Initialize()
    {
        Debug.Log("[PlayerSessionService] Initialized");
    }

    public void SetSession(int accountId, string token, string playerName = null, string refreshToken = null)
    {
        AccountId = accountId;
        Token = token;
        RefreshToken = refreshToken ?? RefreshToken;
        PlayerName = playerName ?? PlayerName;

        // Đồng bộ với PlayerDataHolder1 (giữ tương thích)
        SessionManager.SetSession(accountId, token, PlayerName, RefreshToken);
    }

    public void ClearSession()
    {
        AccountId = 0;
        Token = null;
        RefreshToken = null;
        PlayerName = null;

        // Clear PlayerDataHolder1 an toàn (không gọi Clear() vì nó không tồn tại)
        SessionManager.Clear();

        Debug.Log("[PlayerSessionService] Session đã được clear");
    }

    public bool HasValidSession() => AccountId > 0 && !string.IsNullOrEmpty(Token);
}
