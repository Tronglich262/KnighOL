using UnityEngine;

public static class EquipmentSyncService
{
    public static void ApplyFullJson(string fullJson, GameObject playerClone = null)
    {
        if (string.IsNullOrEmpty(fullJson))
            return;

        PlayerDataHolder1.CharacterJson = fullJson;

        if (AuthManager.GetOrCreate() != null)
            AuthManager.GetOrCreate().StartCoroutine(AuthManager.GetOrCreate().SaveCharacterToServer(fullJson));

        if (playerClone != null)
        {
            var cloneController = playerClone.GetComponent<PlayerCloneController>();
            if (cloneController != null)
            {
                cloneController.SendCharacterJsonToTarget(fullJson);
            }
        }

        if (PlayerAvatar.Instance != null)
        {
            PlayerAvatar.Instance.LoadCharacter(fullJson);

            if (PlayerAvatar.Instance.HasStateAuthority)
                PlayerAvatar.Instance.UpdateCharacterJson(fullJson);
            else
                PlayerAvatar.Instance.RPC_UpdateCharacterJson(fullJson);
        }
    }
}