using System;
using System.Collections;
using System.Text;
using UnityEngine;
using UnityEngine.Networking;

public static class AuthApiClient
{
    public static IEnumerator Login(string baseUrl, LoginDto dto, Action<LoginResponse> onSuccess, Action<string> onError)
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
        request.timeout = 10;

        yield return request.SendWebRequest();

        Debug.Log("[AuthApiClient.Login] responseCode = " + request.responseCode);
        Debug.Log("[AuthApiClient.Login] responseText = " + request.downloadHandler.text);

        if (request.result == UnityWebRequest.Result.Success)
        {
            LoginResponse response = JsonUtility.FromJson<LoginResponse>(request.downloadHandler.text);
            onSuccess?.Invoke(response);
        }
        else
        {
            string error = string.IsNullOrEmpty(request.downloadHandler.text)
                ? request.error
                : request.downloadHandler.text;

            onError?.Invoke(error);
        }
    }

    public static IEnumerator Register(string baseUrl, RegisterDto dto, Action onSuccess, Action<string> onError)
    {
        string fullUrl = baseUrl + "/register";
        string json = JsonUtility.ToJson(dto);

        Debug.Log("[AuthApiClient.Register] URL = " + fullUrl);
        Debug.Log("[AuthApiClient.Register] BODY = " + json);

        using UnityWebRequest request = new UnityWebRequest(fullUrl, "POST");
        byte[] bodyRaw = Encoding.UTF8.GetBytes(json);
        request.uploadHandler = new UploadHandlerRaw(bodyRaw);
        request.downloadHandler = new DownloadHandlerBuffer();
        request.SetRequestHeader("Content-Type", "application/json");
        request.timeout = 10;

        yield return request.SendWebRequest();

        Debug.Log("[AuthApiClient.Register] responseCode = " + request.responseCode);
        Debug.Log("[AuthApiClient.Register] responseText = " + request.downloadHandler.text);

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
}