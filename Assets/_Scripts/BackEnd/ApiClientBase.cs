using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.Networking;

public class ApiClientBase : MonoBehaviour
{
    public static ApiClientBase Instance { get; private set; }

    private static bool _isRefreshing = false;
    private static readonly Queue<PendingRequest> _pendingRequests = new();
    private const int MAX_REFRESH_RETRY = 2;
    private int refreshRetryCount = 0;

    private class PendingRequest
    {
        public string Method;
        public string Endpoint;
        public object Data;
        public Action<object> OnSuccess;
        public Action<string> OnError;
        public Type ResponseType;
    }

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

    public static ApiClientBase GetOrCreate()
    {
        if (Instance != null)
            return Instance;

        var go = new GameObject("ApiClientBase");
        return go.AddComponent<ApiClientBase>();
    }

    public IEnumerator Post<T>(string endpoint, object data, Action<T> onSuccess, Action<string> onError)
    {
        yield return SendRequest("POST", endpoint, data, onSuccess, onError);
    }

    public IEnumerator Get<T>(string endpoint, Action<T> onSuccess, Action<string> onError)
    {
        yield return SendRequest("GET", endpoint, null, onSuccess, onError);
    }

    private IEnumerator SendRequest<T>(string method, string endpoint, object data, Action<T> onSuccess, Action<string> onError)
    {
        var apiConfig = ApiConfigManager.GetInstance();
        if (apiConfig == null)
        {
            onError?.Invoke("ApiConfigManager chua khoi tao");
            yield break;
        }

        string fullUrl = apiConfig.GetFullUrl(endpoint);
        Debug.Log($"[ApiClientBase] 🚀 Gọi API: {method} {fullUrl}");

        if (_isRefreshing && !IsRefreshEndpoint(endpoint))
        {
            Debug.Log("[ApiClientBase] Đang refresh → Đẩy request vào queue");
            var pending = new PendingRequest
            {
                Method = method,
                Endpoint = endpoint,
                Data = data,
                OnSuccess = result => onSuccess?.Invoke((T)result),
                OnError = onError,
                ResponseType = typeof(T)
            };
            _pendingRequests.Enqueue(pending);
            yield break;
        }

        UnityWebRequest request = method switch
        {
            "GET" => UnityWebRequest.Get(fullUrl),
            _ => CreatePostRequest(fullUrl, data)
        };

        AddAuthHeader(request);

        if (fullUrl.Contains("localhost") || fullUrl.Contains("ngrok"))
            request.certificateHandler = new AcceptAllCertificates();

        yield return request.SendWebRequest();

        if (request.result == UnityWebRequest.Result.Success)
        {
            Debug.Log($"[ApiClientBase] ✅ 200 OK - {endpoint}");
            HandleSuccess<T>(request, onSuccess, onError);
        }
        else if (request.responseCode == 401)
        {
            Debug.LogWarning($"[ApiClientBase] 401 tại {endpoint} → Refresh token...");
            yield return HandleTokenRefresh(method, endpoint, data, onSuccess, onError);
        }
        else
        {
            Debug.LogError($"[ApiClientBase] ❌ Lỗi {request.responseCode} tại {endpoint}");
            onError?.Invoke(request.downloadHandler?.text ?? "Unknown error");
        }
    }

    private void AddAuthHeader(UnityWebRequest request)
    {
        string url = request.url.ToLower();
        if (url.Contains("/login") || url.Contains("/refresh")) return;

        if (PlayerSessionService.Instance != null && !string.IsNullOrEmpty(PlayerSessionService.Instance.Token))
        {
            request.SetRequestHeader("Authorization", "Bearer " + PlayerSessionService.Instance.Token);
            Debug.Log($"[ApiClientBase] 📌 Gửi token length = {PlayerSessionService.Instance.Token.Length}");
        }
    }

    private bool IsRefreshEndpoint(string endpoint)
    {
        return !string.IsNullOrEmpty(endpoint) &&
               endpoint.ToLowerInvariant().Contains("refresh");
    }

    private void HandleSuccess<T>(UnityWebRequest request, Action<T> onSuccess, Action<string> onError)
    {
        try
        {
            T result = JsonConvert.DeserializeObject<T>(request.downloadHandler.text);
            onSuccess?.Invoke(result);
            refreshRetryCount = 0;
        }
        catch (Exception ex)
        {
            Debug.LogError($"Parse JSON thất bại: {ex.Message}");
            onError?.Invoke("Lỗi parse JSON");
        }
    }

    private IEnumerator HandleTokenRefresh<T>(string method, string endpoint, object data, Action<T> onSuccess, Action<string> onError)
    {
        if (_isRefreshing || refreshRetryCount >= MAX_REFRESH_RETRY)
        {
            PlayerSessionService.GetOrCreate().ClearSession();
            FailPendingRequests("Token het han. Vui long dang nhap lai.");
            onError?.Invoke("Token hết hạn. Vui lòng đăng nhập lại.");
            yield break;
        }

        _isRefreshing = true;
        refreshRetryCount++;

        bool refreshSuccess = false;
        yield return AuthApiClient.RefreshToken(
            _ => refreshSuccess = true,
            _ => { });

        _isRefreshing = false;

        if (refreshSuccess)
        {
            Debug.Log("[ApiClientBase] Refresh thành công → Xử lý queue + retry");
            // Retry request gốc
            if (method == "GET")
                yield return Get<T>(endpoint, onSuccess, onError);
            else
                yield return Post<T>(endpoint, data, onSuccess, onError);

            yield return ProcessPendingRequests();

        }
        else
        {
            PlayerSessionService.GetOrCreate().ClearSession();
            FailPendingRequests("Refresh token that bai.");
            onError?.Invoke("Refresh token thất bại.");
        }
    }

    private void FailPendingRequests(string error)
    {
        while (_pendingRequests.Count > 0)
        {
            var pending = _pendingRequests.Dequeue();
            pending.OnError?.Invoke(error);
        }
    }

    private IEnumerator ProcessPendingRequests()
    {
        while (_pendingRequests.Count > 0)
        {
            var pending = _pendingRequests.Dequeue();
            Debug.Log($"[ApiClientBase] Retry pending request: {pending.Endpoint}");

            var apiConfig = ApiConfigManager.GetInstance();
            if (apiConfig == null)
            {
                pending.OnError?.Invoke("ApiConfigManager chua khoi tao");
                continue;
            }

            string fullUrl = apiConfig.GetFullUrl(pending.Endpoint);
            UnityWebRequest request = pending.Method == "GET"
                ? UnityWebRequest.Get(fullUrl)
                : CreatePostRequest(fullUrl, pending.Data);

            AddAuthHeader(request);

            if (fullUrl.Contains("localhost") || fullUrl.Contains("ngrok"))
                request.certificateHandler = new AcceptAllCertificates();

            yield return request.SendWebRequest();

            if (request.result == UnityWebRequest.Result.Success)
            {
                try
                {
                    object result = JsonConvert.DeserializeObject(request.downloadHandler.text, pending.ResponseType);
                    pending.OnSuccess?.Invoke(result);
                }
                catch (Exception ex)
                {
                    Debug.LogError($"Parse JSON pending request failed: {ex.Message}");
                    pending.OnError?.Invoke("Loi parse JSON");
                }
            }
            else
            {
                pending.OnError?.Invoke(request.downloadHandler?.text ?? "Unknown error");
            }
        }
    }

    private UnityWebRequest CreatePostRequest(string url, object data)
    {
        string json = data != null ? JsonConvert.SerializeObject(data) : "{}";
        byte[] body = Encoding.UTF8.GetBytes(json);

        var request = new UnityWebRequest(url, "POST");
        request.uploadHandler = new UploadHandlerRaw(body);
        request.downloadHandler = new DownloadHandlerBuffer();
        request.SetRequestHeader("Content-Type", "application/json");
        return request;
    }
}
