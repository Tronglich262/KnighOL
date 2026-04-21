using UnityEngine;

[CreateAssetMenu(fileName = "ApiConfigManager", menuName = "Config/Api Config Manager")]
public class ApiConfigManager : ScriptableObject
{
    public static ApiConfigManager Instance { get; private set; }

    [Header("=== API CONFIG ===")]
    [Tooltip("Đổi thành domain thật khi deploy")]
    public string BaseUrl = "https://localhost:7124";

    [Tooltip("Thường là 'api'")]
    public string ApiVersion = "api";

    [Header("Editor Only")]
    public bool UseLocalhostInEditor = true;

    private void OnEnable()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(this);
            return;
        }
        Instance = this;
    }

    /// <summary>
    /// Tạo URL đầy đủ: https://localhost:7124/api/...
    /// </summary>
    public string GetFullUrl(string endpoint)
    {
        string url = BaseUrl.TrimEnd('/');

        if (!string.IsNullOrEmpty(ApiVersion))
            url += $"/{ApiVersion.Trim('/')}";

        endpoint = endpoint.TrimStart('/');
        return $"{url}/{endpoint}";
    }
}