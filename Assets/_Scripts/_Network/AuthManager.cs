using Assets.HeroEditor.FantasyInventory.Scripts.Interface.Elements;
using System.Collections;
using System.Text;
using System.Text.RegularExpressions;
using TMPro;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using static MenuManager;

public class AuthManager : MonoBehaviour
{
    [Header("UI Panels")]
    public GameObject registerPanel;
    public GameObject loginPanel;

    [Header("Register UI")]
    public TMP_InputField registerUsername;
    public TMP_InputField registerEmail;
    public TMP_InputField registerPassword;

    [Header("Login UI")]
    public TMP_InputField loginEmail;
    public TMP_InputField loginPassword;

    [Header("Thông báo UI")]
    public TMP_Text loginMessageText;
    public TMP_Text registerMessageText;

    [Header("Toggle mật khẩu")]
    public Button loginPasswordToggleBtn;
    public Image loginPasswordEyeIcon;
    public Sprite eyeOpen;
    public Sprite eyeClosed;

    public static AuthManager Instance;

    private Coroutine tokenCheckCoroutine;
    private Coroutine loginMessageCoroutine;
    private Coroutine registerMessageCoroutine;

    private bool isLoginPasswordShown = false;
    private bool isLoggingIn = false;
    private bool isRegistering = false;
    private bool isSavingCharacter = false;
    private string pendingCharacterJson = null;

    private const float TokenCheckInterval = 60f; // Silent refresh mỗi 60 giây

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

    private void Start()
    {
        if (loginPasswordToggleBtn != null)
        {
            loginPasswordToggleBtn.onClick.RemoveAllListeners();
            loginPasswordToggleBtn.onClick.AddListener(ToggleLoginPasswordVisibility);
        }
    }

    private void OnDestroy()
    {
        if (Instance == this) Instance = null;
    }

    // ========================= MESSAGE =========================
    public void ShowLoginMessage(string msg, float duration = 3.5f)
    {
        if (loginMessageCoroutine != null) StopCoroutine(loginMessageCoroutine);
        loginMessageCoroutine = StartCoroutine(ClearLoginMessageAfterDelay(msg, duration));
    }

    private IEnumerator ClearLoginMessageAfterDelay(string msg, float delay)
    {
        if (loginMessageText == null) yield break;
        loginMessageText.text = msg;
        yield return new WaitForSeconds(delay);
        if (loginMessageText != null) loginMessageText.text = "";
    }

    public void ShowRegisterMessage(string msg, float duration = 3.5f)
    {
        if (registerMessageCoroutine != null) StopCoroutine(registerMessageCoroutine);
        registerMessageCoroutine = StartCoroutine(ClearRegisterMessageAfterDelay(msg, duration));
    }

    private IEnumerator ClearRegisterMessageAfterDelay(string msg, float delay)
    {
        if (registerMessageText == null) yield break;
        registerMessageText.text = msg;
        yield return new WaitForSeconds(delay);
        if (registerMessageText != null) registerMessageText.text = "";
    }

    // ========================= REGISTER =========================
    public void OnRegisterClick()
    {
        if (isRegistering) return;
        StartCoroutine(Register());
    }

    private IEnumerator Register()
    {
        isRegistering = true;
        RegisterDto registerDto = new RegisterDto
        {
            Name = registerUsername?.text.Trim() ?? "",
            Email = registerEmail?.text.Trim() ?? "",
            Password = registerPassword?.text.Trim() ?? ""
        };

        bool done = false;
        bool success = false;
        string errorMsg = "";

        yield return StartCoroutine(AuthApiClient.Register(
            registerDto,
            () => { success = true; done = true; },
            error => { errorMsg = NormalizeError(error, "Đăng ký thất bại!"); done = true; }));

        yield return new WaitUntil(() => done);

        if (success)
        {
            ShowRegisterMessage("Đăng ký thành công!\nVui lòng đăng nhập.");
            yield return new WaitForSeconds(1f);
            if (registerPanel != null) registerPanel.SetActive(false);
            if (loginPanel != null) loginPanel.SetActive(true);
        }
        else
        {
            ShowRegisterMessage(errorMsg);
        }

        isRegistering = false;
    }

    // ========================= LOGIN =========================
    public void OnLoginClick()
    {
        if (isLoggingIn) return;
        StartCoroutine(Login());
    }

    private IEnumerator Login()
    {
        isLoggingIn = true;

        LoginDto loginDto = new LoginDto
        {
            Email = loginEmail?.text.Trim() ?? "",
            Password = loginPassword?.text ?? ""
        };

        LoginResponse loginResponse = null;
        string errorMsg = "";

        yield return StartCoroutine(AuthApiClient.Login(
            loginDto,
            response => loginResponse = response,
            error => errorMsg = NormalizeError(error, "Đăng nhập thất bại!")));

        if (loginResponse != null && loginResponse.accountId > 0 && !string.IsNullOrEmpty(loginResponse.accessToken))
        {
            ApplySession(loginResponse);
            ShowLoginMessage("Đăng nhập thành công!");
            yield return new WaitForSeconds(0.5f);
            SceneManager.LoadScene("MenuGame");
            RestartTokenChecker();
        }
        else
        {
            ShowLoginMessage(string.IsNullOrEmpty(errorMsg) ? "Đăng nhập thất bại!" : errorMsg);
        }

        isLoggingIn = false;
    }

    private void ApplySession(LoginResponse loginResponse)
    {
        PlayerSessionService.Instance.SetSession(
            loginResponse.accountId,
            loginResponse.accessToken,
            loginResponse.name,
            loginResponse.refreshToken);

        SessionManager.SetSession(loginResponse.accountId, loginResponse.accessToken,
            loginResponse.name, loginResponse.refreshToken);
    }

    private void RestartTokenChecker()
    {
        if (tokenCheckCoroutine != null) StopCoroutine(tokenCheckCoroutine);
        tokenCheckCoroutine = StartCoroutine(TokenChecker());
    }

    // ========================= TOKEN CHECKER - SILENT REFRESH =========================
    private IEnumerator TokenChecker()
    {
        yield return new WaitForSeconds(8f);

        while (true)
        {
            if (!PlayerSessionService.Instance.HasValidSession())
                yield break;

            yield return new WaitForSeconds(TokenCheckInterval);

            Debug.Log("[TokenChecker] Bắt đầu refresh token...");

            bool refreshSuccess = false;
            yield return AuthApiClient.RefreshToken(
                _ => { refreshSuccess = true; Debug.Log("[TokenChecker] Refresh token THÀNH CÔNG"); },
                error => Debug.LogWarning("[TokenChecker] Refresh thất bại: " + error));

            if (!refreshSuccess)
            {
                Debug.LogWarning("[TokenChecker] Refresh thất bại → Logout");
                ClearSession();
                ForceBackToLogin();
                yield break;
            }
        }
    }

    // ========================= GET PROFILE =========================
    public IEnumerator GetUserProfile()
    {
        if (!PlayerSessionService.Instance.HasValidSession())
            yield break;

        yield return ApiClientBase.Instance.Get<object>("Account/profile",
            response => Debug.Log("[PROFILE] Thành công! Dữ liệu user: " + JsonUtility.ToJson(response)),
            error =>
            {
                Debug.LogError("Lỗi lấy dữ liệu user: " + error);
                if (error.Contains("401"))
                {
                    Debug.LogWarning("Token không hợp lệ hoặc đã đăng nhập ở nơi khác.");
                    ClearSession();
                    ForceBackToLogin();
                }
            });
    }

    // ========================= SAVE CHARACTER =========================
    public IEnumerator SaveCharacterToServer(string characterJson)
    {
        if (string.IsNullOrEmpty(characterJson) || !PlayerSessionService.Instance.HasValidSession())
            yield break;

        if (isSavingCharacter)
        {
            pendingCharacterJson = characterJson;
            yield break;
        }

        isSavingCharacter = true;
        do
        {
            pendingCharacterJson = null;
            yield return StartCoroutine(SaveCharacterInternal(characterJson));
            characterJson = pendingCharacterJson;
        }
        while (!string.IsNullOrEmpty(characterJson));

        isSavingCharacter = false;
    }

    private IEnumerator SaveCharacterInternal(string characterJson)
    {
        SaveCharacterDto dto = new SaveCharacterDto
        {
            AccountId = SessionManager.AccountId,
            CharacterJson = characterJson
        };

        yield return ApiClientBase.Instance.Post<object>("Account/save-character", dto,
            _ => Debug.Log("Lưu nhân vật lên server thành công."),
            error => Debug.LogError("Lỗi khi lưu nhân vật: " + error));
    }

    // ========================= PLAYER STATE =========================
    public IEnumerator GetPlayerState(System.Action<PlayerState> onDone)
    {
        if (!PlayerSessionService.Instance.HasValidSession())
        {
            onDone?.Invoke(null);
            yield break;
        }

        yield return ApiClientBase.Instance.Get<PlayerState>($"Account/playerstate/{SessionManager.AccountId}",
            state => onDone?.Invoke(state),
            error => { Debug.LogError("Lỗi GetPlayerState: " + error); onDone?.Invoke(null); });
    }

    public IEnumerator UpdatePlayerState(UpdatePlayerStateDto dto, System.Action<bool> onDone)
    {
        if (!PlayerSessionService.Instance.HasValidSession())
        {
            onDone?.Invoke(false);
            yield break;
        }

        yield return ApiClientBase.Instance.Post<object>("Account/playerstate/update", dto,
            _ => onDone?.Invoke(true),
            error => { Debug.LogError("Update PlayerState thất bại: " + error); onDone?.Invoke(false); });
    }

    // ========================= STATS =========================
    public IEnumerator GetPlayerStats(System.Action<PlayerStats> onDone)
    {
        if (!PlayerSessionService.Instance.HasValidSession())
        {
            onDone?.Invoke(null);
            yield break;
        }

        yield return ApiClientBase.Instance.Get<PlayerStats>($"Account/stats/{SessionManager.AccountId}",
            stats => onDone?.Invoke(stats),
            error => { Debug.LogError("Lỗi GetPlayerStats: " + error); onDone?.Invoke(null); });
    }

    public IEnumerator AllocateStats(int addHp, int addStrength, int addSpeed, int addAgility, int addSpirit, int addDefense, System.Action<bool> onDone)
    {
        if (!PlayerSessionService.Instance.HasValidSession())
        {
            onDone?.Invoke(false);
            yield break;
        }

        AllocateStatsDto dto = new AllocateStatsDto
        {
            HP = addHp,
            Strength = addStrength,
            Speed = addSpeed,
            Agility = addAgility,
            Spirit = addSpirit,
            Defense = addDefense
        };

        yield return ApiClientBase.Instance.Post<object>("Account/stats/allocate", dto,
            _ => onDone?.Invoke(true),
            error => { Debug.LogError("Lỗi AllocateStats: " + error); onDone?.Invoke(false); });
    }

    // ========================= QUEST =========================
    public IEnumerator GetUserQuests(System.Action<QuestResponse[]> onDone)
    {
        if (!PlayerSessionService.Instance.HasValidSession())
        {
            onDone?.Invoke(null);
            yield break;
        }

        yield return ApiClientBase.Instance.Get<QuestResponse[]>("Account/quests",
            onDone,
            error => { Debug.LogError("Lỗi GetUserQuests: " + error); onDone?.Invoke(null); });
    }

    public void UpdateQuestProgress(string targetType, int targetId, int amount)
    {
        if (!PlayerSessionService.Instance.HasValidSession()) return;
        StartCoroutine(UpdateQuestProgressCoroutine(targetType, targetId, amount));
    }

    private IEnumerator UpdateQuestProgressCoroutine(string targetType, int targetId, int amount)
    {
        QuestProgressDto dto = new QuestProgressDto
        {
            targetType = targetType,
            targetId = targetId,
            amount = amount
        };

        yield return ApiClientBase.Instance.Post<object>("Account/quests/progress", dto,
            _ =>
            {
                QuestDisplay questDisplay = FindAnyObjectByType<QuestDisplay>();
                if (questDisplay != null) questDisplay.ReloadQuests();
            },
            error => Debug.LogError("Update quest progress FAIL: " + error));
    }

    // ========================= CLEAR SESSION =========================
    private void ClearSession()
    {
        SessionManager.Clear();
        PlayerSessionService.Instance.ClearSession();
        isSavingCharacter = false;
        pendingCharacterJson = null;

        if (tokenCheckCoroutine != null)
        {
            StopCoroutine(tokenCheckCoroutine);
            tokenCheckCoroutine = null;
        }
    }

    private void ForceBackToLogin()
    {
        if (SceneManager.GetActiveScene().name != "Login")
            SceneManager.LoadScene("Login");
    }

    // ========================= PASSWORD TOGGLE =========================
    public void ToggleLoginPasswordVisibility()
    {
        if (loginPassword == null) return;
        isLoginPasswordShown = !isLoginPasswordShown;
        loginPassword.contentType = isLoginPasswordShown ? TMP_InputField.ContentType.Standard : TMP_InputField.ContentType.Password;
        loginPassword.ForceLabelUpdate();
        if (loginPasswordEyeIcon != null)
            loginPasswordEyeIcon.sprite = isLoginPasswordShown ? eyeOpen : eyeClosed;
    }

    private string NormalizeError(string raw, string fallback)
    {
        if (string.IsNullOrEmpty(raw)) return fallback;
        try
        {
            LoginResponse resp = JsonUtility.FromJson<LoginResponse>(raw);
            if (resp != null && !string.IsNullOrEmpty(resp.message))
                return resp.message;
        }
        catch { }
        return raw;
    }
}

// ====================== DTOs ======================
[System.Serializable] public class QuestProgressRewardResponse { public string message; public QuestReward reward; }
[System.Serializable] public class QuestReward { public int gold; public int exp; public ItemReward[] items; }
[System.Serializable] public class ItemReward { public int itemId; public int amount; }
[System.Serializable] public class SaveCharacterDto { public int AccountId; public string CharacterJson; }
[System.Serializable] public class LoginDto { public string Email; public string Password; }
[System.Serializable] public class RegisterDto { public string Name; public string Email; public string Password; }
