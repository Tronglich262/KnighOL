using System;
using System.Collections;
using System.Text;
using UnityEngine;
using UnityEngine.Networking;

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
        Debug.Log("=== AuthApiClient.Login CALLED ===");

        if (ApiClientBase.Instance == null)
        {
            Debug.LogError("❌ ApiClientBase.Instance = NULL ! Kiểm tra GameObject ApiClientBase có trong scene không?");
            onError?.Invoke("ApiClientBase chưa khởi tạo");
            yield break;
        }

        Debug.Log("→ Bắt đầu gọi ApiClientBase.Post...");

        yield return ApiClientBase.Instance.Post<LoginResponse>("Account/login", dto,
            response =>
            {
                Debug.Log("→ Post thành công, nhận response");
                if (!string.IsNullOrEmpty(response.accessToken))
                {
                    SessionManager.SetSession(
                        response.accountId,
                        response.accessToken,
                        response.name,
                        response.refreshToken
                    );

                    Debug.Log($"Đăng nhập thành công: {response.name}");
                    onSuccess?.Invoke(response);
                }
                else
                {
                    Debug.LogWarning("Response không có accessToken");
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
