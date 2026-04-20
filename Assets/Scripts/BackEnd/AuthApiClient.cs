// Scripts/BackEnd/AuthApiClient.cs
using System;
using System.Collections;
using System.Text;
using UnityEngine;
using UnityEngine.Networking;

public static class AuthApiClient
{
    public static IEnumerator Register(string baseUrl, RegisterDto dto,
        Action onSuccess, Action<string> onError)
    {
        string fullUrl = baseUrl + "/register";   // ← endpoint đúng

        string json = JsonUtility.ToJson(dto);

        Debug.Log("[AuthApiClient.Register] URL = " + fullUrl);
        Debug.Log("[AuthApiClient.Register] BODY = " + json);

        using UnityWebRequest request = new UnityWebRequest(fullUrl, "POST");
        byte[] bodyRaw = Encoding.UTF8.GetBytes(json);
        request.uploadHandler = new UploadHandlerRaw(bodyRaw);
        request.downloadHandler = new DownloadHandlerBuffer();
        request.SetRequestHeader("Content-Type", "application/json");
        request.timeout = 12;

        // Bypass cert cho localhost
        if (fullUrl.Contains("localhost"))
            request.certificateHandler = new AcceptAllCertificates();

        yield return request.SendWebRequest();

        Debug.Log($"[REGISTER] ResponseCode = {request.responseCode} | Body = {request.downloadHandler.text}");

        if (request.result == UnityWebRequest.Result.Success)
        {
            onSuccess?.Invoke();
        }
        else
        {
            string error = string.IsNullOrEmpty(request.downloadHandler.text)
                ? request.error
                : request.downloadHandler.text;

            onError?.Invoke(error);
        }
    }

    public static IEnumerator Login(string baseUrl, LoginDto dto,
        Action<LoginResponse> onSuccess, Action<string> onError)
    {
        string fullUrl = baseUrl + "/login";

        string json = JsonUtility.ToJson(dto);

        Debug.Log("[AuthApiClient.Login] URL = " + fullUrl);
        Debug.Log("[AuthApiClient.Login] BODY = " + json);

        using UnityWebRequest request = new UnityWebRequest(fullUrl, "POST");
        byte[] bodyRaw = Encoding.UTF8.GetBytes(json);
        request.uploadHandler = new UploadHandlerRaw(bodyRaw);
        request.downloadHandler = new DownloadHandlerBuffer();
        request.SetRequestHeader("Content-Type", "application/json");
        request.timeout = 12;

        if (fullUrl.Contains("localhost"))
            request.certificateHandler = new AcceptAllCertificates();

        yield return request.SendWebRequest();

        Debug.Log($"[LOGIN] ResponseCode = {request.responseCode} | Body = {request.downloadHandler.text}");

        if (request.result == UnityWebRequest.Result.Success)
        {
            string rawJson = request.downloadHandler.text;
            LoginResponse response = JsonUtility.FromJson<LoginResponse>(rawJson);

            if (response != null && !string.IsNullOrEmpty(response.accessToken))
            {
                // Lưu token
                if (ApiService.Instance != null)
                    ApiService.Instance.SetTokens(response.accessToken, response.refreshToken);

                SessionManager.SetSession(response.accountId, response.accessToken, response.name);

                onSuccess?.Invoke(response);
            }
            else
            {
                onError?.Invoke("Response không hợp lệ");
            }
        }
        else
        {
            string error = string.IsNullOrEmpty(request.downloadHandler.text)
                ? request.error
                : request.downloadHandler.text;

            onError?.Invoke(error);
        }
    }
}