using Assets.HeroEditor.Common.CharacterScripts;
using Fusion;
using HeroEditor.Common.Enums;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using Unity.Cinemachine;
using UnityEngine;
using UnityEngine.SceneManagement;

public class PlayerAvatar : NetworkBehaviour
{
    [Networked] public NetworkString<_32> DisplayName { get; set; }
    [Networked] public NetworkString<_32> NickName { get; set; }

    public static PlayerAvatar Instance;
    public Character Character;

    [Networked] public NetworkString<_512> CharacterJsonPart1 { get; set; }
    [Networked] public NetworkString<_512> CharacterJsonPart2 { get; set; }
    [Networked] public NetworkString<_512> CharacterJsonPart3 { get; set; }
    [Networked] public NetworkString<_512> CharacterJsonPart4 { get; set; }
    [Networked] public NetworkString<_512> CharacterJsonPart5 { get; set; }
    [Networked] public NetworkString<_512> CharacterJsonPart6 { get; set; }
    [Networked] public NetworkString<_512> CharacterJsonPart7 { get; set; }

    private string _lastAppliedJson = "";
    private bool isSpawned = false;

    public CinemachineCamera vCam;
    public Camera cam;

    void Awake()
    {
        if (Character == null)
            Character = GetComponentInChildren<Character>();
    }

    public override void Spawned()
    {
        isSpawned = true;

        if (Object.HasInputAuthority)
            Instance = this;

        if (HasStateAuthority && Object.HasInputAuthority)
        {
            UpdateCharacterJson(PlayerDataHolder1.CharacterJson);
            RPC_SetNick(PlayerDataHolder1.PlayerName);
            RPC_SendDisplayNameToServer(PlayerDataHolder1.PlayerName);
        }

        string json = GetFullCharacterJson();
        ApplyCharacter(json);

        SetupCamera();
    }

    public override void Render()
    {
        if (!isSpawned) return;

        string json = GetFullCharacterJson();
        if (_lastAppliedJson == json) return;

        ApplyCharacter(json);
    }

    private void ApplyCharacter(string json)
    {
        if (Character == null || string.IsNullOrEmpty(json)) return;
        if (_lastAppliedJson == json) return;

        _lastAppliedJson = json;

        try
        {
            var dict = JsonConvert.DeserializeObject<Dictionary<string, string>>(json);

            Character.FromJson(json);

            if (dict.TryGetValue("WeaponType", out var weaponType))
            {
                if (weaponType == "Melee2H" &&
                    dict.TryGetValue("PrimaryMeleeWeapon", out var melee2HId))
                {
                    EquipMelee2H(melee2HId);
                }
                else if (weaponType == "Melee1H" &&
                         dict.TryGetValue("PrimaryMeleeWeapon", out var melee1HId))
                {
                    EquipMelee1H(melee1HId);
                }
                else if (weaponType == "Bow" &&
                         dict.TryGetValue("Bow", out var bowId))
                {
                    EquipBow(bowId);
                }
            }

            string[] mixTypes = { "Boots", "Gloves", "Belt", "Pauldrons", "Vest" };
            foreach (var t in mixTypes)
            {
                if (dict.TryGetValue(t, out var partId) && !string.IsNullOrEmpty(partId))
                    CharacterEquipHandler.EquipPartialArmorFromEntry(Character, partId, t);
            }

            if (dict.TryGetValue("Armor", out var armorId) && !string.IsNullOrEmpty(armorId))
                CharacterEquipHandler.TestEquipArmor(Character, armorId);

            Character.Initialize();

            Debug.Log("[PlayerAvatar] ApplyCharacter OK");
        }
        catch (Exception e)
        {
            Debug.LogError($"[PlayerAvatar] ApplyCharacter failed: {e}");
        }
    }

    private void EquipMelee2H(string id)
    {
        var entry = Character.SpriteCollection.MeleeWeapon2H.FirstOrDefault(e => e.Id == id);
        if (entry == null)
        {
            Debug.LogError($"[PlayerAvatar] Melee2H not found: {id}");
            return;
        }

        Character.WeaponType = WeaponType.Melee2H;
        Character.Equip(entry, EquipmentPart.MeleeWeapon2H);
    }

    private void EquipMelee1H(string id)
    {
        var entry = Character.SpriteCollection.MeleeWeapon1H.FirstOrDefault(e => e.Id == id);
        if (entry == null) return;

        Character.WeaponType = WeaponType.Melee1H;
        Character.Equip(entry, EquipmentPart.MeleeWeapon1H);
    }

    private void EquipBow(string id)
    {
        var entry = Character.SpriteCollection.Bow.FirstOrDefault(e => e.Id == id);
        if (entry == null) return;

        Character.WeaponType = WeaponType.Bow;
        Character.Equip(entry, EquipmentPart.Bow);
    }

    public void UpdateCharacterJson(string fullJson)
    {
        if (!HasStateAuthority || string.IsNullOrEmpty(fullJson)) return;

        int max = 512;
        CharacterJsonPart1 = fullJson.Substring(0, Mathf.Min(max, fullJson.Length));
        CharacterJsonPart2 = fullJson.Length > max ? fullJson.Substring(max, Mathf.Min(max, fullJson.Length - max)) : "";
        CharacterJsonPart3 = fullJson.Length > max * 2 ? fullJson.Substring(max * 2, Mathf.Min(max, fullJson.Length - max * 2)) : "";
        CharacterJsonPart4 = fullJson.Length > max * 3 ? fullJson.Substring(max * 3, Mathf.Min(max, fullJson.Length - max * 3)) : "";
        CharacterJsonPart5 = fullJson.Length > max * 4 ? fullJson.Substring(max * 4, Mathf.Min(max, fullJson.Length - max * 4)) : "";
        CharacterJsonPart6 = fullJson.Length > max * 5 ? fullJson.Substring(max * 5, Mathf.Min(max, fullJson.Length - max * 5)) : "";
        CharacterJsonPart7 = fullJson.Length > max * 6 ? fullJson.Substring(max * 6, Mathf.Min(max, fullJson.Length - max * 6)) : "";
    }

    public string GetFullCharacterJson()
    {
        return CharacterJsonPart1.ToString()
             + CharacterJsonPart2.ToString()
             + CharacterJsonPart3.ToString()
             + CharacterJsonPart4.ToString()
             + CharacterJsonPart5.ToString()
             + CharacterJsonPart6.ToString()
             + CharacterJsonPart7.ToString();
    }

    private void SetupCamera()
    {
        vCam = GetComponentInChildren<CinemachineCamera>();
        cam = GetComponentInChildren<Camera>();

        bool isLocal = Object.HasInputAuthority;
        if (vCam) vCam.enabled = isLocal;
        if (cam) cam.enabled = isLocal;
    }

    public bool IsLocalPlayer()
    {
        return Object != null && Object.HasInputAuthority;
    }

    public void LoadCharacter(string json)
    {
        ApplyCharacter(json);
    }

    public void SendCharacterJsonToAllClients()
    {
    }

    [Rpc(RpcSources.All, RpcTargets.StateAuthority)]
    public void RPC_SendDisplayNameToServer(string name) => RPC_SetDisplayName(name);

    [Rpc(RpcSources.StateAuthority, RpcTargets.All)]
    void RPC_SetDisplayName(string name) => DisplayName = name;

    [Rpc(RpcSources.InputAuthority, RpcTargets.StateAuthority)]
    void RPC_SetNick(string name) => NickName = name;

    [Rpc(RpcSources.StateAuthority, RpcTargets.All)]
    public void RPC_KickToLogin()
    {
        if (Object.HasInputAuthority)
        {
            Debug.Log("Bạn bị đá do đăng nhập trùng!");
            SceneManager.LoadScene("Login");
        }
    }
    [Rpc(RpcSources.InputAuthority, RpcTargets.StateAuthority)]
    public void RPC_UpdateCharacterJson(string fullJson)
    {
        UpdateCharacterJson(fullJson);
    }
}