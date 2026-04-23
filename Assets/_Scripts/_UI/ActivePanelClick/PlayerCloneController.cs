using Assets.HeroEditor.Common.CharacterScripts;
using Fusion;
using System.Collections.Generic;
using UnityEngine;

public class PlayerCloneController : MonoBehaviour
{
    public NetworkObject targetPlayerNetworkObject;
    private PlayerAvatar targetAvatar;

    public static PlayerCloneController Instante;

    private Character cachedCharacter;
    private string lastJson = "";

    private void Awake()
    {
        Instante = this;
        cachedCharacter = GetComponent<Character>();
    }

    private void Update()
    {
        string json = PlayerDataHolder1.CharacterJson;

        if (string.IsNullOrEmpty(json))
            return;

        if (json == lastJson)
            return;

        LoadJson(json);
    }

    public void SetTarget(NetworkObject playerObj)
    {
        targetPlayerNetworkObject = playerObj;
        targetAvatar = playerObj != null ? playerObj.GetComponent<PlayerAvatar>() : null;
    }

    public void SendCharacterJsonToTarget(string json)
    {
        if (targetAvatar == null || string.IsNullOrEmpty(json))
            return;

        if (targetAvatar.HasStateAuthority)
        {
            targetAvatar.UpdateCharacterJson(json);
        }
        else
        {
            targetAvatar.RPC_UpdateCharacterJson(json);
        }
    }

    public void LoadJson(string json)
    {
        if (string.IsNullOrEmpty(json))
            return;

        if (cachedCharacter == null)
            cachedCharacter = GetComponent<Character>();

        if (cachedCharacter == null)
            return;

        lastJson = json;

        cachedCharacter.FromJson(json);
        cachedCharacter.Initialize();

        var dict = Newtonsoft.Json.JsonConvert.DeserializeObject<Dictionary<string, string>>(json);
        if (dict == null)
            return;

        string[] mixTypes = { "Boots", "Gloves", "Belt", "Pauldrons", "Vest" };

        foreach (string t in mixTypes)
        {
            if (dict.TryGetValue(t, out string partId) && !string.IsNullOrEmpty(partId))
            {
                CharacterEquipHandler.EquipPartialArmorFromEntry(cachedCharacter, partId, t);
            }
        }
    }
}