using UnityEngine;

public class PlayerLevelManager : MonoBehaviour
{
    public int exp;
    public int level;

    void Start()
    {
        // Lấy giá trị từ PlayerState đã load từ server (PlayerDataHolder1.CurrentPlayerState)
        var state = PlayerDataHolder1.CurrentPlayerState;
        if (state != null)
        {
            exp = state.exp;
            level = state.level;
        }

    }

    public void AddExp(int amount)
    {
        // Lấy lại state mới nhất từ backend đã lưu ở PlayerDataHolder1
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
        int expMax = PlayerLevelUI.Instante.ExpToNextLevel(level);

        while (exp >= expMax)
        {
            exp -= expMax;
            this.level++;
            levelUp = true;
            Debug.Log($"[PlayerLevelManager] Lên Level! {level}");
            expMax = PlayerLevelUI.Instante.ExpToNextLevel(level);
        }

        // Cập nhật lại PlayerState local
        PlayerDataHolder1.CurrentPlayerState.exp = exp;
        PlayerDataHolder1.CurrentPlayerState.level = level;

        // Đẩy lên server mỗi lần thay đổi
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

        AuthManager.Instance.StartCoroutine(
            AuthManager.Instance.UpdatePlayerState(dto, (success) =>
            {
                if (success)
                    Debug.Log("[PlayerLevelManager] Sync exp/level/gold/diamond lên server OK");
                else
                    Debug.LogError("[PlayerLevelManager] Sync exp/level/gold/diamond lên server FAIL");
            })
        );
    }

}
