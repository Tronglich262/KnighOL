using Fusion;
using System.Collections;
using Unity.VisualScripting.Antlr3.Runtime;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.SceneManagement;

public class MenuManager : MonoBehaviour
{
    public GameObject btnChoiMoi;
    public GameObject btnChoiTiep;

    private string apiUrl = "https://localhost:7124/api/Account";
    private int accountId;

    [System.Serializable]
    public class CharacterResponse
    {
        public string characterJson;
    }

    void Start()
    {
        accountId = AuthManager.Instance.UserSession.AccountId; // ĐÚNG!
        if (accountId == 0)
        {
            Debug.LogError("Không có accountId, cần đăng nhập lại.");
            return;
        }
        StartCoroutine(CheckCharacterData());
    }
    //Check dữ liệu CharacterData trên database của account ( nếu null hiện chơi mới , nếu !null  hiện chơi tiếp)
    IEnumerator CheckCharacterData()
    {
        string url = apiUrl + "/get-character/" + accountId;
        UnityWebRequest request = UnityWebRequest.Get(url);
        request.SetRequestHeader("Content-Type", "application/json");

        yield return request.SendWebRequest();

        if (request.result == UnityWebRequest.Result.Success)
        {
            string json = request.downloadHandler.text;
            Debug.Log("Response JSON: " + json);

            // Parse JSON
            CharacterSimpleResponse response = JsonUtility.FromJson<CharacterSimpleResponse>(json);
            PlayerDataHolder1.PlayerName = response.name;
            PlayerDataHolder1.CharacterJson = response.characterJson;

            string raw = response.characterJson?.Trim();

            bool isEmptyCharacter =
                string.IsNullOrEmpty(raw) ||
                raw == "null" ||
                raw == "{}" ||
                raw == "\"{}\""; // Trường hợp bị bao nháy

            if (isEmptyCharacter)
            {
                Debug.Log("Chưa có nhân vật, hiện Chơi Mới");
                btnChoiMoi.SetActive(true);
                btnChoiTiep.SetActive(false);
            }
            else
            {
                Debug.Log("Đã có nhân vật, hiện Chơi Tiếp");
                btnChoiMoi.SetActive(false);
                btnChoiTiep.SetActive(true);
            }

        }
        else
        {
            Debug.LogError("Lỗi khi kiểm tra nhân vật: " + request.error);
        }
    }
    //Load dữ liệu từ Database
    private IEnumerator LoadCharacterAndStartGame()
    {
        string url = apiUrl + "/get-character/" + accountId;
        UnityWebRequest request = UnityWebRequest.Get(url);
        request.SetRequestHeader("Content-Type", "application/json");

        yield return request.SendWebRequest();

        if (request.result == UnityWebRequest.Result.Success)
        {
            string json = request.downloadHandler.text;
            CharacterResponse response = JsonUtility.FromJson<CharacterResponse>(json);

            if (!string.IsNullOrEmpty(response.characterJson) && response.characterJson != "null")
            {
                PlayerDataHolder1.CharacterJson = response.characterJson;
                Debug.Log("Đã tải dữ liệu nhân vật: " + PlayerDataHolder1.CharacterJson);
                PlayerDataHolder1.Character = JsonUtility.FromJson<CharacterData>(PlayerDataHolder1.CharacterJson);

                // Gọi luôn Fusion khởi động chế độ Shared và load scene Test
                FusionManager.Instance.StartFusionSession("Test");
            }
            else
            {
                Debug.LogError("Không tìm thấy dữ liệu nhân vật.");
            }
        }
        else
        {
            Debug.LogError(" Lỗi lấy dữ liệu nhân vật: " + request.error);
        }
    }


    public void OnClickChoiMoi()
    {
        SceneManager.LoadScene("Megapack");
    }

    public void OnClickChoiTiep()
    {
        StartCoroutine(LoadCharacterAndStartGame());
    }
}
