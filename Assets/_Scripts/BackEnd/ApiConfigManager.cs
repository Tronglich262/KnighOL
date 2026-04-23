using UnityEngine;

[CreateAssetMenu(fileName = "ApiConfigManager", menuName = "Config/Api Config Manager")]
public class ApiConfigManager : ScriptableObject
{
    public static ApiConfigManager Instance { get; private set; }

    [Header("=== API CONFIG ===")]
    [Tooltip("Đổi thành domain thật khi deploy")]
    public string BaseUrl = "http://localhost:5072";

    [Tooltip("Thường là 'api'")]
    public string ApiVersion = "api";

    [Header("Editor Only")]
    public bool UseLocalhostInEditor = true;

    private void OnEnable()
    {
        if (Instance == null)
        {
            Instance = this;
            Debug.Log("[ApiConfigManager] Instance đã được load thành công từ Resources.");
        }
    }

    /// <summary>
    /// Load instance từ Resources (đảm bảo hoạt động trong Build)
    /// </summary>
    public static ApiConfigManager GetInstance()
    {
        if (Instance == null)
        {
            Instance = Resources.Load<ApiConfigManager>("ApiConfig/ApiConfigManager");
            if (Instance == null)
                Debug.LogError("❌ Không tìm thấy ApiConfigManager.asset trong Resources/ApiConfig/");
            else
                Debug.Log("[ApiConfigManager] Load từ Resources thành công.");
        }
        return Instance;
    }

    public string GetFullUrl(string endpoint)
    {
        if (Instance == null) GetInstance(); // tự động load nếu chưa có

        string url = BaseUrl.TrimEnd('/');

        if (!string.IsNullOrEmpty(ApiVersion))
            url += $"/{ApiVersion.Trim('/')}";

        endpoint = endpoint.TrimStart('/');
        return $"{url}/{endpoint}";
    }
}