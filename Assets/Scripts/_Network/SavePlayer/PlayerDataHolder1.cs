using UnityEngine;

public static class PlayerDataHolder1
{
    public static string PlayerName;

    private static string _characterJson;

    public static PlayerState CurrentPlayerState;
    public static int AccountId;
    public static string Token;
    public static CharacterData Character;
    public static PlayerStats CurrentStats;

    public static string CharacterJson
    {
        get => _characterJson;
        set
        {
            _characterJson = value;
            Debug.Log("[PlayerDataHolder1] CharacterJson da duoc cap nhat.");
        }
    }
}