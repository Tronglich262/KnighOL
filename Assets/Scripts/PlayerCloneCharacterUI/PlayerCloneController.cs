using Assets.HeroEditor.Common.CharacterScripts;
using Fusion;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class PlayerCloneController : MonoBehaviour
{
    public NetworkObject targetPlayerNetworkObject;
    private PlayerAvatar targetAvatar;
    public static PlayerCloneController Instante;
    public void Awake()
    {
            Instante = this;
    }
    public void Update()
    {
        if (CharacterUIManager.Instance != null)
        {
            LoadJson(PlayerDataHolder1.CharacterJson);
        }
    }


    public void SetTarget(NetworkObject playerObj)
    {
        targetPlayerNetworkObject = playerObj;
        targetAvatar = playerObj.GetComponent<PlayerAvatar>();
    }

    public void SendCharacterJsonToTarget(string json)
    {
        if (targetAvatar != null && targetAvatar.HasStateAuthority)
        {
            targetAvatar.UpdateCharacterJson(json); //  Chia JSON thành 7 phần
            targetAvatar.SendCharacterJsonToAllClients(); //  Fusion tự sync

        }
 
    }


    public void LoadJson(string json)
    {

        if (string.IsNullOrEmpty(json))
        {
            return;
        }

        if (TryGetComponent<Character>(out var character))
        {

            character.FromJson(json);
            character.Initialize();

            // Thay vì Equip ngay, dùng Coroutine đợi 1 frame
            var dict = Newtonsoft.Json.JsonConvert.DeserializeObject<Dictionary<string, string>>(json);
            string[] mixTypes = new[] { "Boots", "Gloves", "Belt", "Pauldrons", "Vest" };
            foreach (string t in mixTypes)
            {
                if (dict.TryGetValue(t, out string partId) && !string.IsNullOrEmpty(partId))
                {
                    CharacterEquipHandler.EquipPartialArmorFromEntry(character, partId, t);
                }
            }

        }
    }

}
