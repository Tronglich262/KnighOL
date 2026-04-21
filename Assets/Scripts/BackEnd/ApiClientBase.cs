// Scripts/BackEnd/ApiClientBase.cs
using Newtonsoft.Json;
using System;
using System.Collections;
using System.Text;
using UnityEngine;
using UnityEngine.Networking;

public class ApiClientBase : MonoBehaviour
{
    public static ApiClientBase Instance { get; private set; }

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

    // ====================== POST ======================
    public IEnumerator Post<T>(string endpoint, object data, Action<T> onSuccess, Action<string> onError)
    {
        yield return SendRequest("POST", endpoint, data, onSuccess, onError);
    }

    // ====================== GET ======================
    public IEnumerator Get<T>(string endpoint, Action<T> onSuccess, Action<string> onError)
    {
        yield return SendRequest("GET", endpoint, null, onSuccess, onError);
    }

    // ====================== CORE REQUEST ======================
    private IEnumerator SendRequest<T>(string method, string endpoint, object data, Action<T> onSuccess, Action<string> onError)
    {
        string fullUrl = ApiConfigManager.Instance.GetFullUrl(endpoint);

        UnityWebRequest request = method switch
        {
            "GET" => UnityWebRequest.Get(fullUrl),
            _ => CreatePostRequest(fullUrl, data)
        };

        // Thêm Token tự động
        if (!string.IsNullOrEmpty(SessionManager.Token))
            request.SetRequestHeader("Authorization", "Bearer " + SessionManager.Token);

        // Bypass certificate cho localhost (Editor only)
        if (fullUrl.Contains("localhost"))
            request.certificateHandler = new AcceptAllCertificates();

        yield return request.SendWebRequest();

        if (request.result == UnityWebRequest.Result.Success)
        {
            try
            {
                string json = request.downloadHandler.text;
                T result = JsonConvert.DeserializeObject<T>(json);
                onSuccess?.Invoke(result);
            }
            catch (Exception ex)
            {
                onError?.Invoke("Lỗi parse JSON: " + ex.Message);
            }
        }
        else
        {
            // Xử lý 401 → tự động refresh token
            if (request.responseCode == 401)
            {
                Debug.LogWarning("Token hết hạn → đang refresh...");
                yield return RefreshTokenRoutine(onSuccess, onError, endpoint, method, data);
                yield break;
            }

            string errorMsg = request.downloadHandler.text;
            if (string.IsNullOrEmpty(errorMsg))
                errorMsg = request.error;

            onError?.Invoke(errorMsg);
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

    // ====================== TỰ ĐỘNG REFRESH TOKEN ======================
    private IEnumerator RefreshTokenRoutine<T>(Action<T> onSuccess, Action<string> onError, string originalEndpoint, string originalMethod, object originalData)
    {
        if (string.IsNullOrEmpty(SessionManager.RefreshToken)) // bạn cần lưu RefreshToken vào SessionManager
        {
            onError?.Invoke("Refresh token không tồn tại. Vui lòng đăng nhập lại.");
            yield break;
        }

        var refreshData = new { refreshToken = SessionManager.RefreshToken };
        bool refreshSuccess = false;

        yield return Post<LoginResponse>("Account/refresh", refreshData,
            response =>
            {
                refreshSuccess = true;
                SessionManager.SetSession(response.accountId, response.accessToken, response.name);
                ApiService.Instance?.SetTokens(response.accessToken, response.refreshToken); // nếu bạn vẫn dùng ApiService
            },
            err => onError?.Invoke("Refresh token thất bại: " + err));

        if (refreshSuccess)
        {
            // Gọi lại request ban đầu
            if (originalMethod == "GET")
                yield return Get<T>(originalEndpoint, onSuccess, onError);
            else
                yield return Post<T>(originalEndpoint, originalData, onSuccess, onError);
        }
    }
}