// Assets/_Scripts/BackEnd/ApiClientBase.cs
using Newtonsoft.Json;
using System;
using System.Collections;
using System.Text;
using UnityEngine;
using UnityEngine.Networking;

public class ApiClientBase : MonoBehaviour
{
    public static ApiClientBase Instance { get; private set; }

    private static bool _isRefreshing = false;
    private const int MAX_REFRESH_RETRY = 2;   // Giới hạn retry
    private int refreshRetryCount = 0;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);
        Debug.Log("[ApiClientBase] Đã khởi tạo thành công!");
    }

    public IEnumerator Post<T>(string endpoint, object data, Action<T> onSuccess, Action<string> onError)
    {
        yield return SendRequest("POST", endpoint, data, onSuccess, onError, endpoint.Contains("refresh"));
    }

    public IEnumerator Get<T>(string endpoint, Action<T> onSuccess, Action<string> onError)
    {
        yield return SendRequest("GET", endpoint, null, onSuccess, onError);
    }

    private IEnumerator SendRequest<T>(string method, string endpoint, object data, Action<T> onSuccess, Action<string> onError, bool isRefreshRequest = false)
    {
        string fullUrl = ApiConfigManager.Instance.GetFullUrl(endpoint);
        Debug.Log($"[ApiClientBase] 🚀 Gọi API: {method} {fullUrl}");

        UnityWebRequest request = method switch
        {
            "GET" => UnityWebRequest.Get(fullUrl),
            _ => CreatePostRequest(fullUrl, data)
        };

        // Thêm token nếu có
        if (PlayerSessionService.Instance != null
    && !string.IsNullOrEmpty(PlayerSessionService.Instance.Token)
    && !isRefreshRequest)   // ← Quan trọng: bỏ qua khi refresh
        {
            request.SetRequestHeader("Authorization", "Bearer " + PlayerSessionService.Instance.Token);
        }

        // Bypass certificate cho ngrok (cả Editor + Build)
        if (fullUrl.Contains("localhost") || fullUrl.Contains("ngrok"))
            request.certificateHandler = new AcceptAllCertificates();

        yield return request.SendWebRequest();

        string rawResponse = request.downloadHandler?.text ?? "[NULL]";

        if (request.result == UnityWebRequest.Result.Success)
        {
            Debug.Log($"[ApiClientBase] ✅ 200 OK - {endpoint} | Raw: {rawResponse}");

            try
            {
                T result = JsonConvert.DeserializeObject<T>(rawResponse);
                onSuccess?.Invoke(result);
                refreshRetryCount = 0; // Reset retry khi thành công
            }
            catch (Exception ex)
            {
                Debug.LogError($"[ApiClientBase] Parse JSON thất bại: {ex.Message}\nRaw: {rawResponse}");
                onError?.Invoke("Lỗi parse JSON");
            }
        }
        else if (request.responseCode == 401 && !isRefreshRequest)
        {
            Debug.LogWarning($"[ApiClientBase] 401 tại {endpoint} → Thử refresh token...");

            if (_isRefreshing || refreshRetryCount >= MAX_REFRESH_RETRY)
            {
                Debug.LogError("[ApiClientBase] Refresh thất bại nhiều lần → Đăng xuất");
                PlayerSessionService.Instance.ClearSession();
                onError?.Invoke("Token hết hạn. Vui lòng đăng nhập lại.");
                yield break;
            }

            refreshRetryCount++;
            _isRefreshing = true;

            bool refreshSuccess = false;
            yield return PerformRefresh(() => refreshSuccess = true);

            _isRefreshing = false;

            if (refreshSuccess)
            {
                // Retry request gốc sau khi refresh thành công
                if (method == "GET")
                    yield return Get<T>(endpoint, onSuccess, onError);
                else
                    yield return Post<T>(endpoint, data, onSuccess, onError);
            }
            else
            {
                PlayerSessionService.Instance.ClearSession();
                onError?.Invoke("Refresh token thất bại. Đăng nhập lại.");
            }
        }
        else
        {
            Debug.LogError($"[ApiClientBase] ❌ Lỗi {request.responseCode} tại {endpoint}\nRaw: {rawResponse}");
            onError?.Invoke(rawResponse);
        }
    }

    private UnityWebRequest CreatePostRequest(string url, object data)
    {
        string json = data != null ? JsonConvert.SerializeObject(data) : "{}";
        byte[] bodyRaw = Encoding.UTF8.GetBytes(json);

        var request = new UnityWebRequest(url, "POST");
        request.uploadHandler = new UploadHandlerRaw(bodyRaw);
        request.downloadHandler = new DownloadHandlerBuffer();
        request.SetRequestHeader("Content-Type", "application/json");
        return request;
    }

    private IEnumerator PerformRefresh(Action onSuccess)
    {
        Debug.Log("[ApiClientBase] Đang refresh token...");

        yield return AuthApiClient.RefreshToken(
            response =>
            {
                Debug.Log("[ApiClientBase] Refresh token THÀNH CÔNG!");
                onSuccess?.Invoke();
            },
            error =>
            {
                Debug.LogError("[ApiClientBase] Refresh token THẤT BẠI: " + error);
            });
    }
}