// Scripts/BackEnd/AuthApiClient.cs
using System;
using System.Collections;
using System.Text;
using UnityEngine;
using UnityEngine.Networking;

using System;
using System.Collections;
using UnityEngine;

public static class AuthApiClient
{
    // ====================== REGISTER ======================
    public static IEnumerator Register(RegisterDto dto, Action onSuccess, Action<string> onError)
    {
        yield return ApiClientBase.Instance.Post<LoginResponse>("Account/register", dto,
            response =>
            {
                Debug.Log("Đăng ký thành công!");
                onSuccess?.Invoke();
            },
            error =>
            {
                Debug.LogError("Đăng ký thất bại: " + error);
                onError?.Invoke(error);
            });
    }

    // ====================== LOGIN ======================
    public static IEnumerator Login(LoginDto dto, Action<LoginResponse> onSuccess, Action<string> onError)
    {
        yield return ApiClientBase.Instance.Post<LoginResponse>("Account/login", dto,
            response =>
            {
                if (!string.IsNullOrEmpty(response.accessToken))
                {
                    SessionManager.SetSession(
                        response.accountId,
                        response.accessToken,
                        response.name,
                        response.refreshToken
                    );

                    if (ApiService.Instance != null)
                        ApiService.Instance.SetTokens(response.accessToken, response.refreshToken);

                    Debug.Log($"Đăng nhập thành công: {response.name}");
                    onSuccess?.Invoke(response);
                }
                else
                {
                    onError?.Invoke("Response không hợp lệ");
                }
            },
            error =>
            {
                Debug.LogError("Đăng nhập thất bại: " + error);
                onError?.Invoke(error);
            });
    }
}