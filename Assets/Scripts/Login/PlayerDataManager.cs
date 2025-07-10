using Assets.HeroEditor.Common.EditorScripts;
using UnityEngine;

public class PlayerDataManager : MonoBehaviour
{
    public static PlayerDataManager Instance;
    public int AccountId { get; private set; } = -1;
    public string CharacterJson { get; private set; }

    public string Token { get; private set; }


    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void SetAccountId(int accountId)
    {
        AccountId = accountId;
        Debug.Log($"AccountId được đặt: {AccountId}");
    }

    public void SetCharacterJson(string characterJson)
    {
        CharacterJson = characterJson;
        Debug.Log($"CharacterJson được đặt: {CharacterJson}");
        // Cập nhật CharacterJson trong CharacterEditor
        CharacterEditor.CharacterJson = characterJson;
    }

    public string GetCharacterJson()
    {
        return CharacterJson;
    }

}