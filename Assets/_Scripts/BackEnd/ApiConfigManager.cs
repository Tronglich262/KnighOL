using UnityEngine;

[CreateAssetMenu(fileName = "ApiConfigManager", menuName = "Config/Api Config Manager")]
public class ApiConfigManager : ScriptableObject
{
    public static ApiConfigManager Instance { get; private set; }

    [Header("=== API CONFIG ===")]
    [Tooltip("URL dùng trong Editor (localhost)")]
    public string EditorBaseUrl = "https://emergency-vivacious-unusable.ngrok-free.dev";

    [Tooltip("URL PUBLIC NGROK hiện tại")]
    public string ProductionBaseUrl = "https://emergency-vivacious-unusable.ngrok-free.dev";

    [Tooltip("Thường là 'api'")]
    public string ApiVersion = "api";

    [Header("Editor Only")]
    public bool ForceUseLocalhostInEditor = false;   // ← Quan trọng: false

    private void OnEnable()
    {
        if (Instance == null)
        {
            Instance = this;
            Debug.Log("[ApiConfigManager] Instance đã load thành công.");
        }
    }

    public static ApiConfigManager GetInstance()
    {
        if (Instance == null)
        {
            Instance = Resources.Load<ApiConfigManager>("ApiConfig/ApiConfigManager");
            if (Instance == null)
                Debug.LogError("❌ Không tìm thấy ApiConfigManager.asset trong Resources/ApiConfig/");
        }
        return Instance;
    }

    /// <summary>
    /// Tự động trả về URL phù hợp (Editor hay Production)
    /// </summary>
    public string GetFullUrl(string endpoint)
    {
        if (Instance == null) GetInstance();

        string baseUrl = Application.isEditor && ForceUseLocalhostInEditor
            ? EditorBaseUrl.TrimEnd('/')
            : ProductionBaseUrl.TrimEnd('/');

        if (!string.IsNullOrEmpty(ApiVersion))
            baseUrl += $"/{ApiVersion.Trim('/')}";

        endpoint = endpoint.TrimStart('/');
        return $"{baseUrl}/{endpoint}";
    }
}