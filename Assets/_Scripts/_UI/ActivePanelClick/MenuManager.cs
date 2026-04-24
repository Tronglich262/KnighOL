using Fusion;
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MenuManager : MonoBehaviour
{
    public GameObject btnChoiMoi;
    public GameObject btnChoiTiep;

    private int accountId;

    [System.Serializable]
    public class CharacterSimpleResponse
    {
        public string name;
        public string characterJson;
    }

    void Start()
    {
        accountId = SessionManager.AccountId;
        if (accountId == 0)
        {
            Debug.LogError("Không có accountId, cần đăng nhập lại.");
            return;
        }

        Debug.Log("🔄 Bắt đầu CheckCharacterData với Production URL");
        StartCoroutine(CheckCharacterData());
    }

    // ====================== KIỂM TRA CÓ NHÂN VẬT CHƯA ======================
    IEnumerator CheckCharacterData()
    {
        string endpoint = $"Account/get-character/{accountId}";

        yield return ApiClientBase.Instance.Get<CharacterSimpleResponse>(endpoint,
            response =>
            {
                Debug.Log("✅ CheckCharacterData thành công");

                PlayerDataHolder1.PlayerName = response.name;
                PlayerDataHolder1.CharacterJson = response.characterJson;

                string raw = response.characterJson?.Trim();

                bool isEmptyCharacter = string.IsNullOrEmpty(raw) ||
                                        raw == "null" ||
                                        raw == "{}" ||
                                        raw == "\"{}\"";

                if (isEmptyCharacter)
                {
                    Debug.Log("Chưa có nhân vật → Hiện CHƠI MỚI");
                    btnChoiMoi.SetActive(true);
                    btnChoiTiep.SetActive(false);
                }
                else
                {
                    Debug.Log("Đã có nhân vật → Hiện CHƠI TIẾP");
                    btnChoiMoi.SetActive(false);
                    btnChoiTiep.SetActive(true);
                }
            },
            error =>
            {
                Debug.LogError("Lỗi khi kiểm tra nhân vật: " + error);
                // Fallback: nếu lỗi thì coi như chưa có nhân vật
                btnChoiMoi.SetActive(true);
                btnChoiTiep.SetActive(false);
            });
    }

    // ====================== LOAD DỮ LIỆU NHÂN VẬT ======================
    private IEnumerator LoadCharacterAndStartGame()
    {
        string endpoint = $"Account/get-character/{accountId}";

        yield return ApiClientBase.Instance.Get<CharacterSimpleResponse>(endpoint,
            response =>
            {
                if (!string.IsNullOrEmpty(response.characterJson) && response.characterJson != "null")
                {
                    PlayerDataHolder1.CharacterJson = response.characterJson;
                    Debug.Log("Đã tải dữ liệu nhân vật thành công");
                    // Nếu cần parse thêm CharacterData thì thêm ở đây

                    // Khởi động Fusion + load scene
                    FusionManager.Instance.StartFusionSession("Test");
                }
                else
                {
                    Debug.LogError("Không tìm thấy dữ liệu nhân vật.");
                }
            },
            error => Debug.LogError("Lỗi lấy dữ liệu nhân vật: " + error));
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