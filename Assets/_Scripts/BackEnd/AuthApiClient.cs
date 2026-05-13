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
        yield return ApiClientBase.GetOrCreate().Post<LoginResponse>("Account/register", dto,
            response => { Debug.Log("Register success."); onSuccess?.Invoke(); },
            error => { Debug.LogError("Register failed: " + error); onError?.Invoke(error); });
    }

    // ====================== LOGIN ======================
    public static IEnumerator Login(LoginDto dto, Action<LoginResponse> onSuccess, Action<string> onError)
    {
        Debug.Log("=== AuthApiClient.Login CALLED ===");

        if (ApiClientBase.GetOrCreate() == null)
        {
            Debug.LogError("ApiClientBase.GetOrCreate returned null.");
            onError?.Invoke("ApiClientBase is not initialized");
            yield break;
        }

        yield return ApiClientBase.GetOrCreate().Post<LoginResponse>("Account/login", dto,
            response =>
            {
                Debug.Log("Login request succeeded.");

                if (response != null && !string.IsNullOrEmpty(response.accessToken))
                {
                    PlayerSessionService.GetOrCreate().SetSession(
                        response.accountId,
                        response.accessToken,
                        response.name,
                        response.refreshToken
                    );

                    Debug.Log($"[AuthApiClient] Login success | AccountId: {response.accountId} | RefreshToken: {(string.IsNullOrEmpty(response.refreshToken) ? "NULL" : "YES")}");
                    onSuccess?.Invoke(response);
                }
                else
                {
                    Debug.LogWarning("Login response has no accessToken.");
                    onError?.Invoke("Invalid login response");
                }
            },
            error =>
            {
                Debug.LogError("Login failed: " + error);
                onError?.Invoke(error);
            });
    }

    // ====================== REFRESH TOKEN (ĐÃ TỐI ƯU + LOG CHI TIẾT) ======================
    public static IEnumerator RefreshToken(Action<LoginResponse> onSuccess, Action<string> onError)
    {
        Debug.Log("=== AuthApiClient.RefreshToken CALLED ===");

        if (ApiClientBase.GetOrCreate() == null)
        {
            onError?.Invoke("ApiClientBase chưa khởi tạo");
            yield break;
        }

        var session = PlayerSessionService.GetOrCreate();
        string refreshToken = session.RefreshToken;

        Debug.Log($"[RefreshToken] Current refresh token: {(string.IsNullOrEmpty(refreshToken) ? "NULL" : "YES - Length = " + refreshToken.Length)}");

        if (string.IsNullOrEmpty(refreshToken))
        {
            Debug.LogWarning("[RefreshToken] Missing refresh token.");
            onError?.Invoke("Missing refresh token");
            yield break;
        }

        RefreshTokenDto dto = new RefreshTokenDto { RefreshToken = refreshToken };

        Debug.Log($"[RefreshToken] Sending POST /Account/refresh with token length = {refreshToken.Length}");

        yield return ApiClientBase.GetOrCreate().Post<LoginResponse>("Account/refresh", dto,
            response =>
            {
                if (response != null && !string.IsNullOrEmpty(response.accessToken))
                {
                    session.SetSession(
                        response.accountId,
                        response.accessToken,
                        response.name,
                        response.refreshToken
                    );

                    Debug.Log("[RefreshToken] Success. New token saved.");
                    onSuccess?.Invoke(response);
                }
                else
                {
                    Debug.LogWarning("[RefreshToken] Invalid response.");
                    onError?.Invoke("Refresh token failed: invalid response");
                }
            },
            error =>
            {
                Debug.LogError("[RefreshToken] Server error: " + error);
                onError?.Invoke(error);
            });
    }

    [System.Serializable]
    public class RefreshTokenDto
    {
        public string RefreshToken;
    }
}
