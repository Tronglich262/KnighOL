using UnityEngine;

public class PlayerLevelManager : MonoBehaviour
{
    public int exp;
    public int level;

    void Start()
    {
        var state = PlayerDataHolder1.CurrentPlayerState;
        if (state != null)
        {
            exp = state.exp;
            level = state.level;
        }

    }

    public void AddExp(int amount)
    {
        var state = PlayerDataHolder1.CurrentPlayerState;
        if (state != null)
        {
            exp = state.exp;
            level = state.level;
        }
        else
        {
            exp = 0;
            level = 1;
        }

        exp += amount;
        Debug.Log($"[PlayerLevelManager] Nhận EXP: {amount} => Tổng: {exp}");

        bool levelUp = false;
        _ = levelUp; // Suppress unused variable warning
        int expMax = PlayerLevelUI.Instante.ExpToNextLevel(level);

        while (exp >= expMax)
        {
            exp -= expMax;
            this.level++;
            levelUp = true;
            Debug.Log($"[PlayerLevelManager] Lên Level! {level}");
            expMax = PlayerLevelUI.Instante.ExpToNextLevel(level);
        }

        PlayerDataHolder1.CurrentPlayerState.exp = exp;
        PlayerDataHolder1.CurrentPlayerState.level = level;
        SyncToServer();
    }


    private void SyncToServer()
    {
        var state = PlayerDataHolder1.CurrentPlayerState;
        var dto = new UpdatePlayerStateDto
        {
            AccountId = PlayerDataHolder1.AccountId,
            Level = state.level,
            Exp = state.exp,
            Gold = state.gold,
            Diamond = state.diamond
        };
        Debug.Log($"UpdatePlayerState: AccountId={dto.AccountId}, Level={dto.Level}, Exp={dto.Exp}, Gold={dto.Gold}, Diamond={dto.Diamond}");

        AuthManager.GetOrCreate().StartCoroutine(
            AuthManager.GetOrCreate().UpdatePlayerState(dto, (success) =>
            {
                if (success)
                    Debug.Log("[PlayerLevelManager] Sync exp/level/gold/diamond lên server OK");
                else
                    Debug.LogError("[PlayerLevelManager] Sync exp/level/gold/diamond lên server FAIL");
            })
        );
    }

}
