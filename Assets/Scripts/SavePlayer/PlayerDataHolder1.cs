using UnityEngine;
using Assets.HeroEditor.Common.CharacterScripts;

public static class PlayerDataHolder1
{
    //lưu trữ dữ liệu Character , accountid , token  ( dữ liệu sẽ truyền qua )
    //nơi lấy dữ liệu sẽ là PlayerAvatar
    public static string PlayerName;

    private static string _characterJson;

    public static PlayerState CurrentPlayerState; // <-- Dùng cho lưu state hiện tại

    public static string CharacterJson
    {
        get => _characterJson;
        set
        {
            _characterJson = value;
            Debug.Log("🟢 CharacterJson đã được gán lại.");

            // Chỉ update UI local
            if (CharacterUIManager1.Instance != null &&
                CharacterUIManager1.Instance.character != null)
            {
                CharacterUIManager1.Instance.character.FromJson(_characterJson);
                Debug.Log("Character UI updated.");
            }
        }
    }


    public static int AccountId;
    public static string Token;
    public static CharacterData Character;
    public static PlayerStats CurrentStats;

}
