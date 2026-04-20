using System;
using System.Collections;
using System.Text;
using UnityEngine;
using UnityEngine.Networking;

public class ApiService : MonoBehaviour
{
    public static ApiService Instance { get; private set; }

    public string BaseUrl = "https://localhost:7124";   // Sau này đổi thành config

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
    }

    public void SetTokens(string accessToken, string refreshToken)
    {
        _accessToken = accessToken;
        _refreshToken = refreshToken;
        SessionManager.SetSession(SessionManager.AccountId, accessToken); // đồng bộ
    }

    public IEnumerator Post<T>(string endpoint, object data, Action<T> onSuccess, Action<string> onError)
    {
        string url = BaseUrl + endpoint;
        string json = JsonUtility.ToJson(data);

        using var request = new UnityWebRequest(url, "POST");
        byte[] bodyRaw = Encoding.UTF8.GetBytes(json);
        request.uploadHandler = new UploadHandlerRaw(bodyRaw);
        request.downloadHandler = new DownloadHandlerBuffer();
        request.SetRequestHeader("Content-Type", "application/json");

        if (!string.IsNullOrEmpty(_accessToken))
            request.SetRequestHeader("Authorization", "Bearer " + _accessToken);

        // Bypass cert cho localhost
        if (url.Contains("localhost"))
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
                // Gọi refresh token ở đây (sẽ bổ sung sau)
            }
            onError?.Invoke(request.error + " | " + request.downloadHandler.text);
        }
    }
}