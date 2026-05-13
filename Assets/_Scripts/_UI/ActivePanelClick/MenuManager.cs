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
            Debug.LogError("No accountId. Login again.");
            return;
        }

        Debug.Log("Start CheckCharacterData with production URL");
        StartCoroutine(CheckCharacterData());
    }

    // ====================== KI?M TRA C� NH�N V?T CHUA ======================
    IEnumerator CheckCharacterData()
    {
        string endpoint = $"Account/get-character/{accountId}";

        yield return ApiClientBase.GetOrCreate().Get<CharacterSimpleResponse>(endpoint,
            response =>
            {
                Debug.Log("CheckCharacterData success");

                PlayerDataHolder1.PlayerName = response.name;
                PlayerDataHolder1.CharacterJson = response.characterJson;

                string raw = response.characterJson?.Trim();

                bool isEmptyCharacter = string.IsNullOrEmpty(raw) ||
                                        raw == "null" ||
                                        raw == "{}" ||
                                        raw == "\"{}\"";

                if (isEmptyCharacter)
                {
                    Debug.Log("No character found. Show Choi Moi.");
                    btnChoiMoi.SetActive(true);
                    btnChoiTiep.SetActive(false);
                }
                else
                {
                    Debug.Log("Character found. Show Choi Tiep.");
                    btnChoiMoi.SetActive(false);
                    btnChoiTiep.SetActive(true);
                }
            },
            error =>
            {
                Debug.LogError("Check character failed: " + error);
                // Fallback: n?u l?i th� coi nhu chua c� nh�n v?t
                btnChoiMoi.SetActive(true);
                btnChoiTiep.SetActive(false);
            });
    }

    // ====================== LOAD D? LI?U NH�N V?T ======================
    private IEnumerator LoadCharacterAndStartGame()
    {
        string endpoint = $"Account/get-character/{accountId}";

        yield return ApiClientBase.GetOrCreate().Get<CharacterSimpleResponse>(endpoint,
            response =>
            {
                if (!string.IsNullOrEmpty(response.characterJson) && response.characterJson != "null")
                {
                    PlayerDataHolder1.CharacterJson = response.characterJson;
                    Debug.Log("Loaded character data successfully");
                    // N?u c?n parse th�m CharacterData th� th�m ? d�y

                    // Kh?i d?ng Fusion + load scene
                    FusionManager.Instance.StartFusionSession("Test");
                }
                else
                {
                    Debug.LogError("Character data not found.");
                }
            },
            error => Debug.LogError("Get character data failed: " + error));
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
