using Assets.HeroEditor.FantasyInventory.Scripts.Interface.Elements;
using System.Collections;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
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

    [Header("API")]
    public string apiUrl = "https://localhost:7124/api/Account";

    [Header("Thông báo UI")]
    public TMP_Text loginMessageText;
    public TMP_Text registerMessageText;

    [Header("Toggle mật khẩu đăng nhập")]
    public Button loginPasswordToggleBtn;
    public Image loginPasswordEyeIcon;
    public Sprite eyeOpen;
    public Sprite eyeClosed;

    public static AuthManager Instance;

    public ClientSession UserSession = new ClientSession();

    private Coroutine tokenCheckCoroutine;
    private Coroutine loginMessageCoroutine;
    private Coroutine registerMessageCoroutine;

    private bool isLoginPasswordShown = false;
    private bool isLoggingIn = false;
    private bool isRegistering = false;
    private bool isSavingCharacter = false;
    private string pendingCharacterJson = null;

    private const float TokenCheckInterval = 2f;

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
            loginPasswordToggleBtn.onClick.RemoveListener(ToggleLoginPasswordVisibility);
            loginPasswordToggleBtn.onClick.AddListener(ToggleLoginPasswordVisibility);
        }
    }

    private void OnDisable()
    {
        StopAllCoroutines();
        tokenCheckCoroutine = null;
        loginMessageCoroutine = null;
        registerMessageCoroutine = null;
    }

    private void OnDestroy()
    {
        if (Instance == this)
            Instance = null;
    }

    // =========================
    // Message UI
    // =========================
    public void ShowLoginMessage(string msg, float duration = 3.5f)
    {
        if (loginMessageCoroutine != null)
            StopCoroutine(loginMessageCoroutine);

        loginMessageCoroutine = StartCoroutine(ClearLoginMessageAfterDelay(msg, duration));
    }

    private IEnumerator ClearLoginMessageAfterDelay(string msg, float delay)
    {
        if (loginMessageText == null)
            yield break;

        loginMessageText.text = msg;
        yield return new WaitForSeconds(delay);

        if (loginMessageText != null)
            loginMessageText.text = "";
    }

    public void ShowRegisterMessage(string msg, float duration = 3.5f)
    {
        if (registerMessageCoroutine != null)
            StopCoroutine(registerMessageCoroutine);

        registerMessageCoroutine = StartCoroutine(ClearRegisterMessageAfterDelay(msg, duration));
    }

    private IEnumerator ClearRegisterMessageAfterDelay(string msg, float delay)
    {
        if (registerMessageText == null)
            yield break;

        registerMessageText.text = msg;
        yield return new WaitForSeconds(delay);

        if (registerMessageText != null)
            registerMessageText.text = "";
    }

    // =========================
    // Register
    // =========================
    public void OnRegisterClick()
    {
        if (isRegistering)
            return;

        string username = registerUsername != null ? registerUsername.text.Trim() : "";
        string email = registerEmail != null ? registerEmail.text.Trim() : "";
        string password = registerPassword != null ? registerPassword.text.Trim() : "";

        if (string.IsNullOrEmpty(username))
        {
            ShowRegisterMessage("Vui lòng nhập tên tài khoản!");
            return;
        }

        if (username.Length < 4)
        {
            ShowRegisterMessage("Tên tài khoản phải có ít nhất 4 ký tự.");
            return;
        }

        if (Regex.IsMatch(username, "[A-Z]"))
        {
            ShowRegisterMessage("Tên tài khoản không được chứa chữ in hoa.");
            return;
        }

        if (!Regex.IsMatch(username, @"^[a-z0-9_]+$"))
        {
            ShowRegisterMessage("Tên tài khoản chỉ chứa chữ thường, số và dấu _.");
            return;
        }

        if (string.IsNullOrEmpty(email))
        {
            ShowRegisterMessage("Vui lòng nhập email!");
            return;
        }

        if (string.IsNullOrEmpty(password))
        {
            ShowRegisterMessage("Vui lòng nhập mật khẩu!");
            return;
        }

        if (password.Length < 6)
        {
            ShowRegisterMessage("Mật khẩu phải có ít nhất 6 ký tự.");
            return;
        }

        if (Regex.IsMatch(password, "[A-Z]"))
        {
            ShowRegisterMessage("Mật khẩu không được chứa chữ in hoa.");
            return;
        }

        if (!Regex.IsMatch(password, @"^[a-z0-9_]+$"))
        {
            ShowRegisterMessage("Mật khẩu chỉ chứa chữ thường, số và dấu _.");
            return;
        }

        StartCoroutine(Register());
    }

    private IEnumerator Register()
    {
        isRegistering = true;

        RegisterDto registerDto = new RegisterDto
        {
            Name = registerUsername != null ? registerUsername.text.Trim() : "",
            Email = registerEmail != null ? registerEmail.text.Trim() : "",
            Password = registerPassword != null ? registerPassword.text.Trim() : ""
        };

        string json = JsonUtility.ToJson(registerDto);

        using UnityWebRequest request = BuildPostRequest(apiUrl + "/register", json, false);

        yield return request.SendWebRequest();

        if (request.result == UnityWebRequest.Result.Success)
        {
            ShowRegisterMessage("Đăng ký thành công!\nVui lòng đăng nhập.");
            yield return new WaitForSeconds(1f);

            if (registerPanel != null) registerPanel.SetActive(false);
            if (loginPanel != null) loginPanel.SetActive(true);
        }
        else
        {
            string errorMsg = ExtractResponseMessage(request, "Đăng ký thất bại!");
            ShowRegisterMessage(errorMsg);
            Debug.LogError(errorMsg);
        }

        isRegistering = false;
    }

    // =========================
    // Login
    // =========================
    public void OnLoginClick()
    {
        if (isLoggingIn)
            return;

        string email = loginEmail != null ? loginEmail.text.Trim() : "";
        string password = loginPassword != null ? loginPassword.text : "";

        if (string.IsNullOrEmpty(email))
        {
            ShowLoginMessage("Vui lòng nhập email!");
            return;
        }

        if (string.IsNullOrEmpty(password))
        {
            ShowLoginMessage("Vui lòng nhập mật khẩu!");
            return;
        }

        StartCoroutine(Login());
    }

    private IEnumerator Login()
    {
        isLoggingIn = true;

        LoginDto loginDto = new LoginDto
        {
            Email = loginEmail != null ? loginEmail.text.Trim() : "",
            Password = loginPassword != null ? loginPassword.text : ""
        };

        string json = JsonUtility.ToJson(loginDto);

        using UnityWebRequest request = BuildPostRequest(apiUrl + "/login", json, false);

        yield return request.SendWebRequest();

        if (request.result == UnityWebRequest.Result.Success)
        {
            string responseJson = request.downloadHandler.text;
            LoginResponse loginResponse = JsonUtility.FromJson<LoginResponse>(responseJson);

            if (loginResponse == null || loginResponse.accountId <= 0 || string.IsNullOrEmpty(loginResponse.token))
            {
                ShowLoginMessage("Dữ liệu đăng nhập trả về không hợp lệ.");
                isLoggingIn = false;
                yield break;
            }

            ApplySession(loginResponse);
            RestartTokenChecker();

            ShowLoginMessage("Đăng nhập thành công!");
            yield return new WaitForSeconds(0.5f);

            SceneManager.LoadScene("MenuGame");
        }
        else
        {
            string errorMsg = ExtractResponseMessage(request, "Đăng nhập thất bại!");
            ShowLoginMessage(errorMsg);
            Debug.LogError(errorMsg);
        }

        isLoggingIn = false;
    }

    private void ApplySession(LoginResponse loginResponse)
    {
        UserSession.AccountId = loginResponse.accountId;
        UserSession.Token = loginResponse.token;

        PlayerDataHolder1.AccountId = loginResponse.accountId;
        PlayerDataHolder1.Token = loginResponse.token;

        Debug.Log($"[LOGIN OK] accountId={UserSession.AccountId}, token={UserSession.Token}");
    }

    private void RestartTokenChecker()
    {
        if (tokenCheckCoroutine != null)
            StopCoroutine(tokenCheckCoroutine);

        tokenCheckCoroutine = StartCoroutine(TokenChecker());
    }

    private IEnumerator TokenChecker()
    {
        while (true)
        {
            if (!gameObject.activeInHierarchy)
                yield break;

            if (!HasValidSession())
                yield break;

            yield return new WaitForSeconds(TokenCheckInterval);
            yield return StartCoroutine(GetUserProfile());
        }
    }

    public async Task<UnityWebRequest> SendAuthRequest(string url)
    {
        UnityWebRequest req = UnityWebRequest.Get(url);
        req.SetRequestHeader("Authorization", "Bearer " + UserSession.Token);
        req.timeout = 10;

        var op = req.SendWebRequest();
        while (!op.isDone)
            await Task.Yield();

        if (req.responseCode == 401)
        {
            Debug.LogWarning("Bị kick về login do đăng nhập trùng hoặc token hết hạn.");
            ForceBackToLogin();
        }

        return req;
    }

    public IEnumerator GetUserProfile()
    {
        if (!HasValidSession())
            yield break;

        using UnityWebRequest request = UnityWebRequest.Get(apiUrl + "/profile");
        request.SetRequestHeader("Authorization", "Bearer " + UserSession.Token);
        request.timeout = 5;

        yield return request.SendWebRequest();

        if (request.result == UnityWebRequest.Result.Success)
            yield break;

        Debug.LogError($"Lỗi lấy dữ liệu user. Code={request.responseCode}, Error={request.error}, Body={request.downloadHandler.text}");

        if (request.responseCode == 401)
        {
            Debug.LogWarning("Token không hợp lệ hoặc đã đăng nhập ở nơi khác.");
        }
        else
        {
            Debug.LogWarning("Không kết nối được đến API. Về màn hình Login!");
        }

        ClearSession();
        ForceBackToLogin();
    }

    // =========================
    // Save Character
    // =========================
    public IEnumerator SaveCharacterToServer(string characterJson)
    {
        if (string.IsNullOrEmpty(characterJson))
            yield break;

        if (!HasValidSession())
        {
            Debug.LogWarning("Chưa có session hợp lệ, bỏ qua SaveCharacterToServer.");
            yield break;
        }

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

            if (!string.IsNullOrEmpty(pendingCharacterJson) && pendingCharacterJson != characterJson)
            {
                characterJson = pendingCharacterJson;
            }
            else
            {
                characterJson = null;
            }
        }
        while (!string.IsNullOrEmpty(characterJson));

        isSavingCharacter = false;
    }

    private IEnumerator SaveCharacterInternal(string characterJson)
    {
        int accountId = UserSession.AccountId;
        if (accountId == 0)
        {
            Debug.LogError("AccountId chưa được lưu, không thể lưu nhân vật.");
            yield break;
        }

        SaveCharacterDto dto = new SaveCharacterDto
        {
            AccountId = accountId,
            CharacterJson = characterJson
        };

        string json = JsonUtility.ToJson(dto);

        using UnityWebRequest request = BuildPostRequest(apiUrl + "/save-character", json, true);
        yield return request.SendWebRequest();

        if (request.result == UnityWebRequest.Result.Success)
        {
            Debug.Log("Lưu nhân vật lên server thành công.");
        }
        else
        {
            Debug.LogError("Lỗi khi lưu nhân vật: " + request.downloadHandler.text);
        }
    }

    // =========================
    // PlayerState
    // =========================
    public IEnumerator GetPlayerState(System.Action<PlayerState> onDone)
    {
        if (!HasValidSession())
        {
            onDone?.Invoke(null);
            yield break;
        }

        string url = apiUrl + $"/playerstate/{UserSession.AccountId}";
        using UnityWebRequest req = UnityWebRequest.Get(url);
        req.SetRequestHeader("Authorization", "Bearer " + UserSession.Token);

        yield return req.SendWebRequest();

        if (req.result == UnityWebRequest.Result.Success)
        {
            PlayerState state = JsonUtility.FromJson<PlayerState>(req.downloadHandler.text);
            onDone?.Invoke(state);
        }
        else
        {
            Debug.LogError("Lỗi GetPlayerState: " + req.downloadHandler.text);
            onDone?.Invoke(null);
        }
    }

    public IEnumerator UpdatePlayerState(UpdatePlayerStateDto dto, System.Action<bool> onDone)
    {
        if (!HasValidSession())
        {
            onDone?.Invoke(false);
            yield break;
        }

        string url = apiUrl + "/playerstate/update";
        string json = JsonUtility.ToJson(dto);

        using UnityWebRequest req = BuildPostRequest(url, json, true);
        yield return req.SendWebRequest();

        Debug.Log("UpdatePlayerState JSON: " + json);

        if (req.result == UnityWebRequest.Result.Success)
        {
            Debug.Log("Update PlayerState thành công!");
            onDone?.Invoke(true);
        }
        else
        {
            Debug.LogError("Update PlayerState thất bại: " + req.downloadHandler.text);
            onDone?.Invoke(false);
        }
    }

    // =========================
    // Quests
    // =========================
    public IEnumerator GetUserQuests(System.Action<QuestResponse[]> onDone)
    {
        if (!HasValidSession())
        {
            onDone?.Invoke(null);
            yield break;
        }

        string url = apiUrl + "/quests";
        using UnityWebRequest req = UnityWebRequest.Get(url);
        req.SetRequestHeader("Authorization", "Bearer " + UserSession.Token);

        yield return req.SendWebRequest();

        if (req.result == UnityWebRequest.Result.Success)
        {
            QuestResponse[] quests = JsonHelper.FromJson<QuestResponse>(req.downloadHandler.text);
            onDone?.Invoke(quests);
        }
        else
        {
            Debug.LogError("Lỗi GetUserQuests: " + req.downloadHandler.text);
            onDone?.Invoke(null);
        }
    }

    public void UpdateQuestProgress(string targetType, int targetId, int amount)
    {
        if (!HasValidSession())
            return;

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

        string json = JsonUtility.ToJson(dto);
        string url = apiUrl + "/quests/progress";

        using UnityWebRequest req = BuildPostRequest(url, json, true);
        yield return req.SendWebRequest();

        if (req.result != UnityWebRequest.Result.Success)
        {
            Debug.LogError("Update quest progress FAIL: " + req.downloadHandler.text);
            yield break;
        }

        QuestDisplay questDisplay = GameObject.FindAnyObjectByType<QuestDisplay>();
        if (questDisplay != null)
            questDisplay.ReloadQuests();

        string text = req.downloadHandler.text;
        QuestProgressRewardResponse rewardResp = JsonUtility.FromJson<QuestProgressRewardResponse>(text);

        if (rewardResp?.reward == null)
            yield break;

        if (PlayerDataHolder1.CurrentPlayerState != null)
        {
            if (rewardResp.reward.gold > 0)
            {
                PlayerDataHolder1.CurrentPlayerState.gold += rewardResp.reward.gold;
                Debug.Log($"[REWARD] Nhận {rewardResp.reward.gold} vàng!");
            }

            if (rewardResp.reward.exp > 0)
            {
                PlayerDataHolder1.CurrentPlayerState.exp += rewardResp.reward.exp;
                Debug.Log($"[REWARD] Nhận {rewardResp.reward.exp} EXP!");
            }
        }

        if (rewardResp.reward.items != null && InventoryManager.Instance != null)
        {
            foreach (var item in rewardResp.reward.items)
            {
                InventoryManager.Instance.AddItem(item.itemId.ToString(), item.amount);
                Debug.Log($"[REWARD] Nhận {item.amount} x {item.itemId}!");
            }
        }

        string rewardMsg = BuildRewardMessage(rewardResp.reward);
        if (ItemDetailsPanel.Instance != null && !string.IsNullOrEmpty(rewardMsg))
        {
            ItemDetailsPanel.Instance.ShowEquipMessage(rewardMsg, 2.5f);
        }

        if (!string.IsNullOrEmpty(rewardMsg))
            Debug.Log("[QUEST REWARD] " + rewardMsg);
    }

    private string BuildRewardMessage(QuestReward reward)
    {
        if (reward == null)
            return "";

        StringBuilder sb = new StringBuilder();

        if (reward.gold > 0)
            sb.Append($"Nhận: {reward.gold} vàng");

        if (reward.exp > 0)
        {
            if (sb.Length > 0) sb.Append(", ");
            else sb.Append("Nhận: ");

            sb.Append($"{reward.exp} exp");
        }

        if (reward.items != null)
        {
            foreach (var it in reward.items)
            {
                if (sb.Length > 0) sb.Append(", ");
                else sb.Append("Nhận: ");

                sb.Append($"{it.amount} x {it.itemId}");
            }
        }

        return sb.ToString();
    }

    // =========================
    // Stats
    // =========================
    public IEnumerator GetPlayerStats(System.Action<PlayerStats> onDone)
    {
        if (!HasValidSession())
        {
            onDone?.Invoke(null);
            yield break;
        }

        string url = apiUrl + $"/stats/{UserSession.AccountId}";
        using UnityWebRequest req = UnityWebRequest.Get(url);
        req.SetRequestHeader("Authorization", "Bearer " + UserSession.Token);

        yield return req.SendWebRequest();

        if (req.result == UnityWebRequest.Result.Success)
        {
            PlayerStats stats = JsonUtility.FromJson<PlayerStats>(req.downloadHandler.text);
            onDone?.Invoke(stats);
        }
        else
        {
            Debug.LogError("Lỗi GetPlayerStats: " + req.downloadHandler.text);
            onDone?.Invoke(null);
        }
    }

    public IEnumerator AllocateStats(int addHp, int addStrength, int addSpeed, int addAgility, int addSpirit, int addDefense, System.Action<bool> onDone)
    {
        if (!HasValidSession())
        {
            onDone?.Invoke(false);
            yield break;
        }

        string url = apiUrl + "/stats/allocate";
        AllocateStatsDto dto = new AllocateStatsDto
        {
            HP = addHp,
            Strength = addStrength,
            Speed = addSpeed,
            Agility = addAgility,
            Spirit = addSpirit,
            Defense = addDefense
        };

        string json = JsonUtility.ToJson(dto);

        using UnityWebRequest req = BuildPostRequest(url, json, true);
        yield return req.SendWebRequest();

        if (req.result == UnityWebRequest.Result.Success)
        {
            Debug.Log("Cộng điểm thành công!");
            onDone?.Invoke(true);
        }
        else
        {
            Debug.LogError("Lỗi AllocateStats: " + req.downloadHandler.text);
            onDone?.Invoke(false);
        }
    }

    // =========================
    // Password Toggle
    // =========================
    public void ToggleLoginPasswordVisibility()
    {
        if (loginPassword == null)
            return;

        isLoginPasswordShown = !isLoginPasswordShown;

        loginPassword.contentType = isLoginPasswordShown
            ? TMP_InputField.ContentType.Standard
            : TMP_InputField.ContentType.Password;

        loginPassword.ForceLabelUpdate();

        if (loginPasswordEyeIcon != null)
            loginPasswordEyeIcon.sprite = isLoginPasswordShown ? eyeOpen : eyeClosed;
    }

    // =========================
    // Helpers
    // =========================
    private UnityWebRequest BuildPostRequest(string url, string json, bool withAuth)
    {
        UnityWebRequest request = new UnityWebRequest(url, "POST");
        byte[] bodyRaw = Encoding.UTF8.GetBytes(json);

        request.uploadHandler = new UploadHandlerRaw(bodyRaw);
        request.downloadHandler = new DownloadHandlerBuffer();
        request.SetRequestHeader("Content-Type", "application/json");
        request.timeout = 10;

        if (withAuth && !string.IsNullOrEmpty(UserSession.Token))
            request.SetRequestHeader("Authorization", "Bearer " + UserSession.Token);

        return request;
    }

    private bool HasValidSession()
    {
        return UserSession != null &&
               UserSession.AccountId > 0 &&
               !string.IsNullOrEmpty(UserSession.Token);
    }

    private string ExtractResponseMessage(UnityWebRequest request, string fallback)
    {
        string errorMsg = fallback;

        if (request == null || request.downloadHandler == null)
            return errorMsg;

        string text = request.downloadHandler.text;
        if (string.IsNullOrEmpty(text))
            return errorMsg;

        try
        {
            LoginResponse resp = JsonUtility.FromJson<LoginResponse>(text);
            if (resp != null && !string.IsNullOrEmpty(resp.message))
                errorMsg = resp.message;
        }
        catch
        {
        }

        return errorMsg;
    }

    private void ClearSession()
    {
        if (UserSession == null)
            UserSession = new ClientSession();

        UserSession.AccountId = 0;
        UserSession.Token = "";

        PlayerDataHolder1.AccountId = 0;
        PlayerDataHolder1.Token = "";

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
        Scene currentScene = SceneManager.GetActiveScene();
        if (currentScene.name != "Login")
        {
            SceneManager.LoadScene("Login");
        }
    }
}

[System.Serializable]
public class QuestProgressRewardResponse
{
    public string message;
    public QuestReward reward;
}

[System.Serializable]
public class QuestReward
{
    public int gold;
    public int exp;
    public ItemReward[] items;
}

[System.Serializable]
public class ItemReward
{
    public int itemId;
    public int amount;
}

[System.Serializable]
public class CharacterSimpleResponse
{
    public string name;
    public string characterJson;
}

[System.Serializable]
public class SaveCharacterDto
{
    public int AccountId;
    public string CharacterJson;
}

[System.Serializable]
public class LoginResponse
{
    public string message;
    public int accountId;
    public string token;
}

[System.Serializable]
public class RegisterDto
{
    public string Name;
    public string Email;
    public string Password;
}

[System.Serializable]
public class LoginDto
{
    public string Email;
    public string Password;
}