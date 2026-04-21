using System;
using System.Collections;
using System.Text;
using UnityEngine;
using UnityEngine.Networking;

public class ApiService : MonoBehaviour
{
    public static ApiService Instance { get; private set; }

    [SerializeField] private ApiConfigManager apiConfig;   // Kéo asset vào đây

    private string _accessToken = "";
    private string _refreshToken = "";

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);

        // Tự load config nếu chưa gán
        if (apiConfig == null)
            apiConfig = Resources.Load<ApiConfigManager>("ApiConfigManager");
    }

    public string BaseUrl => apiConfig != null ? apiConfig.BaseUrl : "https://localhost:7124";

    public void SetTokens(string accessToken, string refreshToken)
    {
        _accessToken = accessToken;
        _refreshToken = refreshToken;
        SessionManager.SetSession(SessionManager.AccountId, accessToken);
    }

    // ========== POST với config mới ==========
    public IEnumerator Post<T>(string endpoint, object data, Action<T> onSuccess, Action<string> onError)
    {
        string fullUrl = apiConfig != null
            ? apiConfig.GetFullUrl(endpoint)
            : BaseUrl + endpoint;

        string json = JsonUtility.ToJson(data);

        using var request = new UnityWebRequest(fullUrl, "POST");
        byte[] bodyRaw = Encoding.UTF8.GetBytes(json);
        request.uploadHandler = new UploadHandlerRaw(bodyRaw);
        request.downloadHandler = new DownloadHandlerBuffer();
        request.SetRequestHeader("Content-Type", "application/json");

        if (!string.IsNullOrEmpty(_accessToken))
            request.SetRequestHeader("Authorization", "Bearer " + _accessToken);

        // Bypass cert cho localhost
        if (fullUrl.Contains("localhost"))
            request.certificateHandler = new AcceptAllCertificates();

        yield return request.SendWebRequest();

        if (request.result == UnityWebRequest.Result.Success)
        {
            T result = JsonUtility.FromJson<T>(request.downloadHandler.text);
            onSuccess?.Invoke(result);
        }
        else
        {
            if (request.responseCode == 401)
            {
                Debug.LogWarning("Token hết hạn → đang refresh...");
            }
            onError?.Invoke(request.error + " | " + request.downloadHandler.text);
        }
    }
}