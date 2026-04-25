// Assets/Scripts/Login/LoginManager.cs
using System;
using UnityEngine;
using UnityEngine.SceneManagement;

public class LoginManager : MonoBehaviour
{
    public static LoginManager Instance { get; private set; }

    [Header("UI References")]
    public GameObject loginPanel;
    public GameObject loadingPanel;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);

        Debug.Log("🔥 [LoginManager] Awake - Đã khởi tạo thành công!");
    }

    private void Start()
    {
        Debug.Log("🔥 [LoginManager] Start - Bắt đầu TryAutoLogin...");
        TryAutoLogin();
    }

    private void TryAutoLogin()
    {
        if (PlayerSessionService.Instance.HasValidSession())
        {
            Debug.Log("[LoginManager] Có session cũ → Auto login");
            ShowLoading(true);
            GoToGameScene();
        }
        else
        {
            Debug.Log("[LoginManager] Không có session cũ → Hiện form login");
        }
    }

    public void Login(string email, string password)
    {
        ShowLoading(true);

        LoginDto dto = new LoginDto { Email = email, Password = password };

        StartCoroutine(AuthApiClient.Login(dto,
            onSuccess: response =>
            {
                if (response != null && !string.IsNullOrEmpty(response.accessToken))
                {
                    PlayerSessionService.Instance.SetSession(
                        response.accountId,
                        response.accessToken,
                        response.name,
                        response.refreshToken
                    );

                    Debug.Log($"[LoginManager] Đăng nhập THÀNH CÔNG! AccountId: {response.accountId}");
                    GoToGameScene();
                }
                else
                {
                    Debug.LogError("[LoginManager] Đăng nhập thất bại - Không có token");
                    ShowLoading(false);
                }
            },
            onError: error =>
            {
                Debug.LogError("[LoginManager] Lỗi đăng nhập: " + error);
                ShowLoading(false);
            }));
    }

    private void GoToGameScene()
    {
        Debug.Log("[LoginManager] Chuyển sang Game scene...");
        SceneManager.LoadScene("MenuGame");   // Đổi thành tên scene chính của bạn nếu khác
    }

    private void ShowLoading(bool show)
    {
        if (loadingPanel != null) loadingPanel.SetActive(show);
        if (loginPanel != null) loginPanel.SetActive(!show);
    }

    public void Logout()
    {
        PlayerSessionService.Instance.ClearSession();
        SceneManager.LoadScene("Login");
    }
}