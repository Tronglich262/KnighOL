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
            response => { Debug.Log("Đăng ký thành công!"); onSuccess?.Invoke(); },
            error => { Debug.LogError("Đăng ký thất bại: " + error); onError?.Invoke(error); });
    }

    // ====================== LOGIN ======================
    public static IEnumerator Login(LoginDto dto, Action<LoginResponse> onSuccess, Action<string> onError)
    {
        Debug.Log("=== AuthApiClient.Login CALLED ===");

        if (ApiClientBase.Instance == null)
        {
            Debug.LogError("❌ ApiClientBase.Instance = NULL !");
            onError?.Invoke("ApiClientBase chưa khởi tạo");
            yield break;
        }

        yield return ApiClientBase.Instance.Post<LoginResponse>("Account/login", dto,
            response =>
            {
                Debug.Log("→ Post Login thành công, nhận response");

                if (response != null && !string.IsNullOrEmpty(response.accessToken))
                {
                    PlayerSessionService.Instance.SetSession(
                        response.accountId,
                        response.accessToken,
                        response.name,
                        response.refreshToken
                    );

                    Debug.Log($"[AuthApiClient] ✅ Đăng nhập thành công | AccountId: {response.accountId} | RefreshToken: {(string.IsNullOrEmpty(response.refreshToken) ? "NULL" : "CÓ")}");
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

    // ====================== REFRESH TOKEN (ĐÃ TỐI ƯU + LOG CHI TIẾT) ======================
    public static IEnumerator RefreshToken(Action<LoginResponse> onSuccess, Action<string> onError)
    {
        Debug.Log("=== AuthApiClient.RefreshToken CALLED ===");

        if (ApiClientBase.Instance == null)
        {
            onError?.Invoke("ApiClientBase chưa khởi tạo");
            yield break;
        }

        string refreshToken = PlayerSessionService.Instance.RefreshToken;

        Debug.Log($"[RefreshToken] RefreshToken từ PlayerSessionService: {(string.IsNullOrEmpty(refreshToken) ? "NULL" : "CÓ - Length = " + refreshToken.Length)}");

        if (string.IsNullOrEmpty(refreshToken))
        {
            Debug.LogWarning("[RefreshToken] ❌ Không có RefreshToken");
            onError?.Invoke("Không có refresh token");
            yield break;
        }

        RefreshTokenDto dto = new RefreshTokenDto { RefreshToken = refreshToken };

        Debug.Log($"[RefreshToken] Đang gửi POST /Account/refresh với RefreshToken length = {refreshToken.Length}");

        yield return ApiClientBase.Instance.Post<LoginResponse>("Account/refresh", dto,
            response =>
            {
                if (response != null && !string.IsNullOrEmpty(response.accessToken))
                {
                    PlayerSessionService.Instance.SetSession(
                        response.accountId,
                        response.accessToken,
                        response.name,
                        response.refreshToken
                    );

                    Debug.Log("[RefreshToken] ✅ THÀNH CÔNG - Token mới đã được lưu");
                    onSuccess?.Invoke(response);
                }
                else
                {
                    Debug.LogWarning("[RefreshToken] Response không hợp lệ");
                    onError?.Invoke("Refresh token thất bại - response không hợp lệ");
                }
            },
            error =>
            {
                Debug.LogError("[RefreshToken] ❌ Lỗi từ server: " + error);
                onError?.Invoke(error);
            });
    }

    [System.Serializable]
    public class RefreshTokenDto
    {
        public string RefreshToken;
    }
}