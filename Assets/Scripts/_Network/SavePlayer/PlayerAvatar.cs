using Assets.HeroEditor.Common.CharacterScripts;
using Fusion;
using HeroEditor.Common.Enums;
using System;
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
    private bool _spawned;

    public CinemachineCamera vCam;
    public Camera cam;

    private void Awake()
    {
        if (Character == null)
            Character = GetComponentInChildren<Character>(true);
    }

    public override void Spawned()
    {
        _spawned = true;

        if (Object.HasInputAuthority)
            Instance = this;

        if (HasStateAuthority && Object.HasInputAuthority)
        {
            string json = PlayerDataHolder1.CharacterJson;
            if (!string.IsNullOrEmpty(json))
                UpdateCharacterJson(json);

            string playerName = PlayerDataHolder1.PlayerName;
            if (!string.IsNullOrEmpty(playerName))
            {
                RPC_SetNick(playerName);
                RPC_SendDisplayNameToServer(playerName);
            }
        }

        ApplyCharacter(GetFullCharacterJson());
        SetupCamera();
    }

    public override void Render()
    {
        if (!_spawned)
            return;

        string json = GetFullCharacterJson();
        if (string.IsNullOrEmpty(json))
            return;

        if (_lastAppliedJson == json)
            return;

        ApplyCharacter(json);
    }

    private void ApplyCharacter(string json)
    {
        if (Character == null || string.IsNullOrEmpty(json))
            return;

        if (_lastAppliedJson == json)
            return;

        try
        {
            _lastAppliedJson = json;

            var dict = CharacterJsonService.LoadDict(json);

            Character.FromJson(json);

            ApplyWeapon(dict);
            ApplyMixedArmor(dict);

            Character.Initialize();
        }
        catch (Exception e)
        {
            Debug.LogError($"[PlayerAvatar] ApplyCharacter failed: {e}");
        }
    }

    private void ApplyWeapon(System.Collections.Generic.Dictionary<string, string> dict)
    {
        if (!dict.TryGetValue("WeaponType", out var weaponType))
            return;

        if (weaponType == EquipKeys.Weapon_Melee2H &&
            dict.TryGetValue(EquipKeys.PrimaryMeleeWeapon, out var melee2HId) &&
            !string.IsNullOrEmpty(melee2HId))
        {
            var entry = Character.SpriteCollection.MeleeWeapon2H.FirstOrDefault(e => e.Id == melee2HId);
            if (entry != null)
            {
                Character.WeaponType = WeaponType.Melee2H;
                Character.Equip(entry, EquipmentPart.MeleeWeapon2H);
            }

            return;
        }

        if (weaponType == EquipKeys.Weapon_Melee1H &&
            dict.TryGetValue(EquipKeys.PrimaryMeleeWeapon, out var melee1HId) &&
            !string.IsNullOrEmpty(melee1HId))
        {
            var entry = Character.SpriteCollection.MeleeWeapon1H.FirstOrDefault(e => e.Id == melee1HId);
            if (entry != null)
            {
                Character.WeaponType = WeaponType.Melee1H;
                Character.Equip(entry, EquipmentPart.MeleeWeapon1H);
            }

            return;
        }

        if (weaponType == EquipKeys.Weapon_Bow &&
            dict.TryGetValue(EquipKeys.Bow, out var bowId) &&
            !string.IsNullOrEmpty(bowId))
        {
            var entry = Character.SpriteCollection.Bow.FirstOrDefault(e => e.Id == bowId);
            if (entry != null)
            {
                Character.WeaponType = WeaponType.Bow;
                Character.Equip(entry, EquipmentPart.Bow);
            }
        }
    }

    private void ApplyMixedArmor(System.Collections.Generic.Dictionary<string, string> dict)
    {
        string[] mixedTypes = { "Boots", "Gloves", "Belt", "Pauldrons", "Vest" };

        foreach (var type in mixedTypes)
        {
            if (dict.TryGetValue(type, out var partId) && !string.IsNullOrEmpty(partId))
            {
                CharacterEquipHandler.EquipPartialArmorFromEntry(Character, partId, type);
            }
        }

        if (dict.TryGetValue("Armor", out var armorId) && !string.IsNullOrEmpty(armorId))
        {
            CharacterEquipHandler.TestEquipArmor(Character, armorId);
        }
    }

    public void UpdateCharacterJson(string fullJson)
    {
        if (!HasStateAuthority || string.IsNullOrEmpty(fullJson))
            return;

        const int max = 512;

        CharacterJsonPart1 = SafeChunk(fullJson, 0, max);
        CharacterJsonPart2 = SafeChunk(fullJson, max, max);
        CharacterJsonPart3 = SafeChunk(fullJson, max * 2, max);
        CharacterJsonPart4 = SafeChunk(fullJson, max * 3, max);
        CharacterJsonPart5 = SafeChunk(fullJson, max * 4, max);
        CharacterJsonPart6 = SafeChunk(fullJson, max * 5, max);
        CharacterJsonPart7 = SafeChunk(fullJson, max * 6, max);
    }

    private string SafeChunk(string source, int start, int length)
    {
        if (string.IsNullOrEmpty(source))
            return "";

        if (start >= source.Length)
            return "";

        int count = Mathf.Min(length, source.Length - start);
        return source.Substring(start, count);
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
        if (vCam == null)
            vCam = GetComponentInChildren<CinemachineCamera>(true);

        if (cam == null)
            cam = GetComponentInChildren<Camera>(true);

        bool isLocal = Object.HasInputAuthority;

        if (vCam != null)
            vCam.enabled = isLocal;

        if (cam != null)
            cam.enabled = isLocal;
    }

    public bool IsLocalPlayer()
    {
        return Object != null && Object.HasInputAuthority;
    }

    public void LoadCharacter(string json)
    {
        ApplyCharacter(json);
    }

    [Rpc(RpcSources.All, RpcTargets.StateAuthority)]
    public void RPC_SendDisplayNameToServer(string name)
    {
        RPC_SetDisplayName(name);
    }

    [Rpc(RpcSources.StateAuthority, RpcTargets.All)]
    private void RPC_SetDisplayName(string name)
    {
        DisplayName = name;
    }

    [Rpc(RpcSources.InputAuthority, RpcTargets.StateAuthority)]
    private void RPC_SetNick(string name)
    {
        NickName = name;
    }

    [Rpc(RpcSources.StateAuthority, RpcTargets.All)]
    public void RPC_KickToLogin()
    {
        if (Object.HasInputAuthority)
        {
            Debug.Log("[PlayerAvatar] Bạn bị đá do đăng nhập trùng.");
            SceneManager.LoadScene("Login");
        }
    }

    [Rpc(RpcSources.InputAuthority, RpcTargets.StateAuthority)]
    public void RPC_UpdateCharacterJson(string fullJson)
    {
        UpdateCharacterJson(fullJson);
    }
}