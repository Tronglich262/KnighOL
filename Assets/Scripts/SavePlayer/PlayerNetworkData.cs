using Fusion;
using UnityEngine;

public class PlayerNetworkData : NetworkBehaviour
{
    [Networked]
    public NetworkString<_256> CharacterJson { get; set; }

    private PlayerAvatar playerAvatar;
    private string lastJson = "";

    private void Awake()
    {
        playerAvatar = GetComponent<PlayerAvatar>();
    }

    public override void FixedUpdateNetwork()
    {
        if (!Object.HasStateAuthority)
        {
            string currentJson = CharacterJson.ToString();

            if (currentJson != lastJson)
            {
                lastJson = currentJson;
                playerAvatar.LoadCharacter(currentJson);
            }
        }
    }

    public void SetCharacterJson(string json)
    {
        if (Object.HasStateAuthority)
        {
            CharacterJson = json;
        }
    }
}