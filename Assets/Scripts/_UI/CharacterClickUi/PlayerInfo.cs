using Fusion;
using UnityEngine;

public class PlayerInfo : MonoBehaviour
{
    public Sprite playerIcon;

    private NetworkObject netObj;
    private NameTagManager nameTag;

    private void Awake()
    {
        netObj = GetComponent<NetworkObject>();
        nameTag = GetComponentInChildren<NameTagManager>();
    }

    // lấy tên từ NameTagManager 
    public string PlayerName
    {
        get
        {
            if (nameTag != null && !string.IsNullOrEmpty(nameTag.Nickname))
                return nameTag.Nickname;

            return "Player";
        }
    }

    public bool CanBeTargetedBy(NetworkObject requester)
    {
        if (netObj == null || requester == null) return false;
        return netObj != requester;
    }
}
