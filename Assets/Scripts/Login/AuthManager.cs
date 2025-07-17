using Assets.HeroEditor.FantasyInventory.Scripts.Interface.Elements;
using System.Collections;
using System.Text;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.SceneManagement;
using static MenuManager;
using UnityEngine.UI;

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

    private string apiUrl = "https://localhost:7124/api/Account";
    private Coroutine tokenCheckCoroutine;  // token

    [Header("Thông báo UI")]
    public TMP_Text loginMessageText;
    public TMP_Text registerMessageText;

    // Thêm biến coroutine cho từng thông báo
    private Coroutine loginMessageCoroutine;
    private Coroutine registerMessageCoroutine;

    //ẩn hiện mk
    [Header("Toggle mật khẩu đăng nhập")]
    public Button loginPasswordToggleBtn;      // Button mắt bên cạnh ô mật khẩu đăng nhập
    public Image loginPasswordEyeIcon;         // Optional: Image icon trong button (nếu có)
    public Sprite eyeOpen;                     // Optional
    public Sprite eyeClosed;                   // Optional

    private bool isLoginPasswordShown = false;
    public ClientSession UserSession = new ClientSession();

    private void Start()
    {
        //ẩn hiện mk
        if (loginPasswordToggleBtn != null)
            loginPasswordToggleBtn.onClick.AddListener(ToggleLoginPasswordVisibility);



    }
    public static AuthManager Instance;

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

    IEnumerator TokenChecker()
    {
        while (true)
        {
            yield return new WaitForSeconds(1f); // kiểm tra mỗi 1 giây
            yield return StartCoroutine(GetUserProfile()); // gọi API kiểm tra token
        }
    }

    // === HÀM HIỂN THỊ & TỰ ẨN THÔNG BÁO ===
    public void ShowLoginMessage(string msg, float duration = 3.5f)
    {
        if (loginMessageCoroutine != null) StopCoroutine(loginMessageCoroutine);
        loginMessageCoroutine = StartCoroutine(ClearLoginMessageAfterDelay(msg, duration));
    }
    IEnumerator ClearLoginMessageAfterDelay(string msg, float delay)
    {
        loginMessageText.text = msg;
        yield return new WaitForSeconds(delay);
        loginMessageText.text = "";
    }
    public void ShowRegisterMessage(string msg, float duration = 3.5f)
    {
        if (registerMessageCoroutine != null) StopCoroutine(registerMessageCoroutine);
        registerMessageCoroutine = StartCoroutine(ClearRegisterMessageAfterDelay(msg, duration));
    }
    IEnumerator ClearRegisterMessageAfterDelay(string msg, float delay)
    {
        registerMessageText.text = msg;
        yield return new WaitForSeconds(delay);
        registerMessageText.text = "";
    }

    // ==== ĐĂNG KÝ ====
    public void OnRegisterClick()
    {
        string username = registerUsername.text.Trim();
        string password = registerPassword.text.Trim();

        // Kiểm tra username
        if (string.IsNullOrEmpty(username))
        {
            ShowRegisterMessage("Vui lòng nhập tên tài khoản!");
            return;
        }
        if (username.Length < 6)
        {
            ShowRegisterMessage("Tên tài khoản phải có ít nhất 4 ký tự.");
            return;
        }
        if (System.Text.RegularExpressions.Regex.IsMatch(username, "[A-Z]"))
        {
            ShowRegisterMessage("Tên tài khoản không được chứa chữ in hoa.");
            return;
        }
        if (!System.Text.RegularExpressions.Regex.IsMatch(username, @"^[a-z0-9_]+$"))
        {
            ShowRegisterMessage("Tên tài khoản chỉ chứa chữ thường, số và dấu _.");
            return;
        }

        // Kiểm tra email
        if (string.IsNullOrEmpty(registerEmail.text))
        {
            ShowRegisterMessage("Vui lòng nhập email!");
            return;
        }

        // Kiểm tra password
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
        if (System.Text.RegularExpressions.Regex.IsMatch(password, "[A-Z]"))
        {
            ShowRegisterMessage("Mật khẩu không được chứa chữ in hoa.");
            return;
        }
        if (!System.Text.RegularExpressions.Regex.IsMatch(password, @"^[a-z0-9_]+$"))
        {
            ShowRegisterMessage("Mật khẩu chỉ chứa chữ thường, số và dấu _.");
            return;
        }

        // Nếu qua hết thì mới gọi coroutine
        StartCoroutine(Register());
    }


    IEnumerator Register()
    {
        RegisterDto registerDto = new RegisterDto
        {
            Name = registerUsername.text,
            Email = registerEmail.text,
            Password = registerPassword.text
        };

        string json = JsonUtility.ToJson(registerDto);

        UnityWebRequest request = new UnityWebRequest(apiUrl + "/register", "POST");
        byte[] bodyRaw = Encoding.UTF8.GetBytes(json);
        request.uploadHandler = new UploadHandlerRaw(bodyRaw);
        request.downloadHandler = new DownloadHandlerBuffer();
        request.SetRequestHeader("Content-Type", "application/json");

        yield return request.SendWebRequest();

        if (request.result == UnityWebRequest.Result.Success)
        {
            ShowRegisterMessage("Đăng ký thành công!\n Vui lòng đăng nhập.");
            yield return new WaitForSeconds(1f);

            registerPanel.SetActive(false);
            loginPanel.SetActive(true);
        }
        else
        {
            // Mặc định lỗi
            string errorMsg = "Đăng ký thất bại!";
            if (!string.IsNullOrEmpty(request.downloadHandler.text))
            {
                try
                {
                    var resp = JsonUtility.FromJson<LoginResponse>(request.downloadHandler.text);
                    if (!string.IsNullOrEmpty(resp.message))
                    {
                        errorMsg = resp.message;
                    }
                }
                catch { }
            }
            ShowRegisterMessage(errorMsg);
            Debug.LogError(errorMsg);
        }
    }

    // ==== ĐĂNG NHẬP ====
    public void OnLoginClick()
    {
        if (string.IsNullOrEmpty(loginEmail.text))
        {
            ShowLoginMessage("Vui lòng nhập email!");
            return;
        }
        if (string.IsNullOrEmpty(loginPassword.text))
        {
            ShowLoginMessage("Vui lòng nhập mật khẩu!");
            return;
        }

        StartCoroutine(Login());
    }

    IEnumerator Login()
    {
        LoginDto loginDto = new LoginDto
        {
            Email = loginEmail.text,
            Password = loginPassword.text
        };

        string json = JsonUtility.ToJson(loginDto);

        UnityWebRequest request = new UnityWebRequest(apiUrl + "/login", "POST");
        byte[] bodyRaw = Encoding.UTF8.GetBytes(json);
        request.uploadHandler = new UploadHandlerRaw(bodyRaw);
        request.downloadHandler = new DownloadHandlerBuffer();
        request.SetRequestHeader("Content-Type", "application/json");

        yield return request.SendWebRequest();

        if (request.result == UnityWebRequest.Result.Success)
        {
            ShowLoginMessage("Đăng nhập thành công!");
            string responseJson = request.downloadHandler.text;

            LoginResponse loginResponse = JsonUtility.FromJson<LoginResponse>(responseJson);
            // Lưu vào UserSession
            UserSession.AccountId = loginResponse.accountId;
            UserSession.Token = loginResponse.token;


            PlayerDataHolder1.AccountId = loginResponse.accountId;   // Nếu muốn lưu static (dùng trong các script khác)
            PlayerDataHolder1.Token = loginResponse.token;

            Debug.Log($"[LOGIN OK] This user: {UserSession.AccountId}, token: {UserSession.Token}");


            StartCoroutine(TokenChecker());  // bắt đầu kiểm tra token định kỳ
                                             // Sau khi login thành công:
            UserSession.AccountId = loginResponse.accountId;
            UserSession.Token = loginResponse.token;
            yield return new WaitForSeconds(1f);

            SceneManager.LoadScene("MenuGame");
        }
        else
        {
            string errorMsg = "Đăng nhập thất bại!";
            if (!string.IsNullOrEmpty(request.downloadHandler.text))
            {
                try
                {
                    LoginResponse resp = JsonUtility.FromJson<LoginResponse>(request.downloadHandler.text);
                    if (!string.IsNullOrEmpty(resp.message))
                    {
                        errorMsg = resp.message;
                    }
                }
                catch { }
            }
            ShowLoginMessage(errorMsg);
            Debug.LogError(errorMsg);
        }
    }

    //Send Token , để Check
    public async Task<UnityWebRequest> SendAuthRequest(string url)
    {
        UnityWebRequest req = UnityWebRequest.Get(url);
        req.SetRequestHeader("Authorization", "Bearer " + UserSession.Token);
        await req.SendWebRequest();

        if (req.responseCode == 401)
        {
            // Token không hợp lệ - bị login trùng
            Debug.Log("Bị kick về login do đăng nhập trùng!");
            SceneManager.LoadScene("LoginScene");
        }

        return req;
    }

    //  gọi API có xác thực token , nếu trùng Token  , đẩy client đầu về Scene Login
    public IEnumerator GetUserProfile()
    {
        string token = AuthManager.Instance.UserSession.Token;
        if (string.IsNullOrEmpty(token))
        {
            Debug.LogError("Token không tồn tại, vui lòng đăng nhập lại.");
            yield break;
        }

        UnityWebRequest request = UnityWebRequest.Get(apiUrl + "/profile");
        request.SetRequestHeader("Authorization", "Bearer " + token);

        // Thêm timeout để tránh treo
        request.timeout = 5; // 5 giây

        yield return request.SendWebRequest();

        // Nếu bị timeout, mất mạng, hoặc lỗi bất kỳ
        if (request.result != UnityWebRequest.Result.Success)
        {
            Debug.LogError($"Lỗi lấy dữ liệu user. Response code: {request.responseCode}, Error: {request.error}, Body: {request.downloadHandler.text}");

            // == THÊM PHẦN NÀY: Out game khi lỗi kết nối hoặc không phải lỗi 401 ==
            // Lưu ý: có thể bỏ điều kiện dưới nếu muốn cứ lỗi là out
            if (request.responseCode == 401)
            {
                Debug.LogWarning("Token không hợp lệ hoặc đã đăng nhập ở nơi khác.");
            }
            else
            {
                Debug.LogWarning("Không kết nối được đến API. Về màn hình Login!");
            }

            // XÓA PlayerPrefs và thay bằng reset biến session!
            UserSession.AccountId = 0;
            UserSession.Token = "";

            if (tokenCheckCoroutine != null)
            {
                StopCoroutine(tokenCheckCoroutine);
            }

            SceneManager.LoadScene("Login"); // hoặc tên scene login bạn dùng
            yield break;
        }

        // Thành công thì có thể cập nhật gì đó ở đây
    }


    // Gọi API , lấy dữ liệu CharacterData từ database xuống
    public IEnumerator SaveCharacterToServer(string characterJson)
    {
        int accountId = AuthManager.Instance.UserSession.AccountId;
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

        UnityWebRequest request = new UnityWebRequest(apiUrl + "/save-character", "POST");
        byte[] bodyRaw = Encoding.UTF8.GetBytes(json);
        request.uploadHandler = new UploadHandlerRaw(bodyRaw);
        request.downloadHandler = new DownloadHandlerBuffer();
        request.SetRequestHeader("Content-Type", "application/json");
        request.SetRequestHeader("Authorization", "Bearer " + UserSession.Token);

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
    //ẩn hiện mk 
    public void ToggleLoginPasswordVisibility()
    {
        isLoginPasswordShown = !isLoginPasswordShown;
        loginPassword.contentType = isLoginPasswordShown
            ? TMP_InputField.ContentType.Standard
            : TMP_InputField.ContentType.Password;
        loginPassword.ForceLabelUpdate();

        //  Nếu có Image và Sprite, cập nhật icon mắt luôn cho đẹp:
        if (loginPasswordEyeIcon != null)
            loginPasswordEyeIcon.sprite = isLoginPasswordShown ? eyeOpen : eyeClosed;
    }
    //lấy dữ liệu playstas
    public IEnumerator GetPlayerState(System.Action<PlayerState> onDone)
    {
        int accountId = UserSession.AccountId;
        if (accountId == 0)
        {
            Debug.LogError("Chưa có AccountId!");
            onDone?.Invoke(null);
            yield break;
        }

        string url = apiUrl + $"/playerstate/{accountId}";
        UnityWebRequest req = UnityWebRequest.Get(url);
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
    //update playstas
    public IEnumerator UpdatePlayerState(UpdatePlayerStateDto dto, System.Action<bool> onDone)
    {
        string url = apiUrl + "/playerstate/update";
        string json = JsonUtility.ToJson(dto);

        UnityWebRequest req = new UnityWebRequest(url, "POST");
        byte[] bodyRaw = Encoding.UTF8.GetBytes(json);
        req.uploadHandler = new UploadHandlerRaw(bodyRaw);
        req.downloadHandler = new DownloadHandlerBuffer();
        req.SetRequestHeader("Content-Type", "application/json");
        req.SetRequestHeader("Authorization", "Bearer " + UserSession.Token);

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
    //lấy nhiệm vụ 
    public IEnumerator GetUserQuests(System.Action<QuestResponse[]> onDone)
    {
        string url = apiUrl + "/quests";
        UnityWebRequest req = UnityWebRequest.Get(url);
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

    //Update nhiệm vụ 
    public void UpdateQuestProgress(string targetType, int targetId, int amount)
    {
        StartCoroutine(UpdateQuestProgressCoroutine(targetType, targetId, amount));
    }

    private IEnumerator UpdateQuestProgressCoroutine(string targetType, int targetId, int amount)
    {
        var dto = new QuestProgressDto
        {
            targetType = targetType,
            targetId = targetId,
            amount = amount
        };
        string json = JsonUtility.ToJson(dto);

        string url = apiUrl + "/quests/progress";
        UnityWebRequest req = new UnityWebRequest(url, "POST");
        byte[] bodyRaw = Encoding.UTF8.GetBytes(json);
        req.uploadHandler = new UploadHandlerRaw(bodyRaw);
        req.downloadHandler = new DownloadHandlerBuffer();
        req.SetRequestHeader("Content-Type", "application/json");
        req.SetRequestHeader("Authorization", "Bearer " + UserSession.Token);

        yield return req.SendWebRequest();

        if (req.result == UnityWebRequest.Result.Success)
        {
            Debug.Log("Cập nhật tiến độ nhiệm vụ thành công!");
            // Tìm và reload UI quest:
            var questDisplay = GameObject.FindObjectOfType<QuestDisplay>();
            if (questDisplay != null) questDisplay.ReloadQuests();

        }
        else
        {
            Debug.LogError("Update quest progress FAIL: " + req.downloadHandler.text);
        }
    }
}
//tắt api thì out game



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