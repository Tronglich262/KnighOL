using Assets.HeroEditor.Common.CharacterScripts;
using Fusion;
using HeroEditor.Common.Enums;
using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using Unity.Cinemachine;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.SceneManagement;
using System.Linq;

public class PlayerAvatar : NetworkBehaviour
{
    public static PlayerAvatar Instance;
    public Character Character;


    public GameObject characterPrefab; // Gán trong prefab Player
    private GameObject characterInstance;

    // Các phần nhỏ của JSON  ( khởi tạo nhiều luồng để lưu dữ liệu ) vì dữ liệu quá 512
    [Networked] public NetworkString<_512> CharacterJsonPart1 { get; set; }
    [Networked] public NetworkString<_512> CharacterJsonPart2 { get; set; }
    [Networked] public NetworkString<_512> CharacterJsonPart3 { get; set; }
    [Networked] public NetworkString<_512> CharacterJsonPart4 { get; set; }
    [Networked] public NetworkString<_512> CharacterJsonPart5 { get; set; }
    [Networked] public NetworkString<_512> CharacterJsonPart6 { get; set; }
    [Networked] public NetworkString<_512> CharacterJsonPart7 { get; set; }

    public string _lastCharacterJson = "";
    public CinemachineCamera vCam;
    public Camera cam;
    void Awake()
    {
        if (HasStateAuthority)
        {
            Instance = this;
            UpdateCharacterJson(PlayerDataHolder1.CharacterJson, Character);

            Debug.Log(" PlayerAvatar.Instance đã được gán.");
        }

        if (HasStateAuthority)
        {
            UpdateCharacterJson(PlayerDataHolder1.CharacterJson, Character);
        }
        if (Character == null)
        {
            Character = GetComponentInChildren<Character>();
        }

    }

    public override void Spawned()
    {
        isSpawned = true;

        // 1. Tạo bản Character riêng biệt trước
        if (Character == null)
        {
            Debug.LogError(" Chưa gán Character trong playerPrefab!");
        }

        if (HasStateAuthority)
        {
            UpdateCharacterJson(PlayerDataHolder1.CharacterJson, Character);
        }

        _lastCharacterJson = GetFullCharacterJson();
        LoadCharacter(_lastCharacterJson);
        vCam = GetComponentInChildren<CinemachineCamera>();
        cam = GetComponentInChildren<Camera>();
        if (Object.HasInputAuthority)
        {
            // Kích hoạt virtual camera cho player local
            if (vCam != null)
                vCam.enabled = true;
            if (cam != null)
                cam.enabled = true;
        }
        else
        {
            // Tắt virtual camera cho các player không phải local
            if (vCam != null)
                vCam.enabled = false;
            if (cam != null)
                cam.enabled = false;
        }


    }

    private string _lastSyncedJson = "";
    private bool isSpawned = false;

    void Update()
    {
        if (!isSpawned) return;

        string currentJson = GetFullCharacterJson();

        if ((_lastSyncedJson != currentJson))
        {
            _lastSyncedJson = currentJson;
            LoadCharacter(currentJson);
            Debug.Log($"[SYNC] {Runner.LocalPlayer} thấy JSON đã thay đổi.");
        }
    }

    //xử lý load Character
    public void LoadCharacter(string json)
    {
        if (Character == null || string.IsNullOrEmpty(json)) return;

        try
        {
            Character.FromJson(json);
            Character.Initialize();

            var dict = JsonConvert.DeserializeObject<Dictionary<string, string>>(json);

            // ✅ Bổ sung xử lý Melee2H đúng chuẩn
            if (dict.TryGetValue("WeaponType", out var weaponType) && weaponType == "Melee2H")
            {
                if (dict.TryGetValue("SecondaryMeleeWeapon", out var weaponId))
                {
                    var entry = Character.SpriteCollection.MeleeWeapon2H.FirstOrDefault(e => e.Id == weaponId);
                    if (entry != null)
                    {
                        Character.WeaponType = WeaponType.Melee2H;
                        Character.Equip(entry, EquipmentPart.MeleeWeapon2H);
                        Debug.Log("[PlayerAvatar] Equip Melee2H thủ công sau khi FromJson.");
                    }
                    else
                    {
                        Debug.LogWarning($"[PlayerAvatar] Không tìm thấy Melee2H entry: {weaponId}");
                    }
                }
            }
            // ✅ GỌI LẠI PHỐI ARMOR MIX (Boots, Gloves, Belt, Pauldrons, Vest)
            string[] mixTypes = new[] { "Boots", "Gloves", "Belt", "Pauldrons", "Vest" };
            foreach (string t in mixTypes)
            {
                if (dict.TryGetValue(t, out string partId) && !string.IsNullOrEmpty(partId))
                {
                    CharacterEquipHandler.EquipPartialArmorFromEntry(Character, partId, t);
                }
            }
            // ✅ Bổ sung xử lý Armor (nếu dùng HeroEditor)
            if (dict.TryGetValue("Armor", out var armorId))
            {
                CharacterEquipHandler.TestEquipArmor(Character, armorId);
            }

            Debug.Log("Character loaded from JSON.");
        }
        catch (Exception ex)
        {
            Debug.LogError($"❌ Failed to load character from JSON: {ex.Message}");
        }
    }




    // Cập nhật và chia nhỏ JSON thành các phần nhỏ
    public void UpdateCharacterJson(string fullJson, Character targetCharacter)
    {


        if (!HasStateAuthority)
        {
            return;
        }

        int maxLen = 512;
        int maxTotalLen = maxLen * 7;

        if (fullJson.Length > maxTotalLen)
        {
            return;
        }

        CharacterJsonPart1 = fullJson.Substring(0, Mathf.Min(maxLen, fullJson.Length));
        CharacterJsonPart2 = fullJson.Length > maxLen ? fullJson.Substring(maxLen, Mathf.Min(maxLen, fullJson.Length - maxLen)) : "";
        CharacterJsonPart3 = fullJson.Length > maxLen * 2 ? fullJson.Substring(maxLen * 2, Mathf.Min(maxLen, fullJson.Length - maxLen * 2)) : "";
        CharacterJsonPart4 = fullJson.Length > maxLen * 3 ? fullJson.Substring(maxLen * 3, Mathf.Min(maxLen, fullJson.Length - maxLen * 3)) : "";
        CharacterJsonPart5 = fullJson.Length > maxLen * 4 ? fullJson.Substring(maxLen * 4, Mathf.Min(maxLen, fullJson.Length - maxLen * 4)) : "";
        CharacterJsonPart6 = fullJson.Length > maxLen * 5 ? fullJson.Substring(maxLen * 5, Mathf.Min(maxLen, fullJson.Length - maxLen * 5)) : "";
        CharacterJsonPart7 = fullJson.Length > maxLen * 6 ? fullJson.Substring(maxLen * 6, Mathf.Min(maxLen, fullJson.Length - maxLen * 6)) : "";

        Debug.Log("CharacterJson updated and split into parts.");
        //check melee2h đẻ hiển thị trên player ol 
        var dict = JsonConvert.DeserializeObject<Dictionary<string, string>>(fullJson);

        if (dict.TryGetValue("WeaponType", out var weaponType) && weaponType == "Melee2H")
        {
            if (dict.TryGetValue("SecondaryMeleeWeapon", out var weaponId))
            {
                var entry = targetCharacter.SpriteCollection.MeleeWeapon2H.FirstOrDefault(e => e.Id == weaponId);
                if (entry != null)
                {
                    targetCharacter.WeaponType = WeaponType.Melee2H;
                    targetCharacter.Equip(entry, EquipmentPart.MeleeWeapon2H);
                    return;
                }

            }
        }

        //  Nếu không phải Melee2H thì dùng FromJson
        targetCharacter.FromJson(fullJson);

        //  Gọi lại EquipPartialArmor cho từng phần giáp nếu có
        if (dict.TryGetValue("Armor", out var armorId))
        {
            CharacterEquipHandler.TestEquipArmor(targetCharacter, armorId);

        }

    }

    // Lấy lại JSON đầy đủ từ các phần nhỏ
    public string GetFullCharacterJson()
    {
        return CharacterJsonPart1.ToString() +
               CharacterJsonPart2.ToString() +
               CharacterJsonPart3.ToString() +
               CharacterJsonPart4.ToString() +
               CharacterJsonPart5.ToString() +
               CharacterJsonPart6.ToString() +
               CharacterJsonPart7.ToString();
    }
    //check token , kich login nếu trùng token , đẩy client về Scene Login
    [Rpc(RpcSources.StateAuthority, RpcTargets.All)]
    public void RPC_KickToLogin()
    {
        if (Object.HasInputAuthority)
        {
            Debug.Log("Bạn bị đá do đăng nhập trùng!");
            SceneManager.LoadScene("Login");
        }
    }


    public void SendCharacterJsonToAllClients()
    {
        if (!HasStateAuthority)
        {
            return;
        }

        // KHÔNG cần RPC. Fusion sẽ tự sync các Networked fields
        Debug.Log(" Fusion sẽ tự đồng bộ CharacterJsonPart1-7 đến tất cả client.");
    }


    public void UpdateCharacterJson(string fullJson)
    {
        UpdateCharacterJson(fullJson, Character); // dùng chính Character của PlayerAvatar
    }
    [Rpc(RpcSources.All, RpcTargets.StateAuthority)]
    public void RPC_ReceiveCharacterJson(string json)
    {
        UpdateCharacterJson(json); //  Gọi từ Player thật → Fusion sẽ sync CharacterJsonPart1-7
    }

    [Serializable]
    public class SaveCharacterDto
    {
        public int AccountId;
        public string CharacterJson;
    }
}
