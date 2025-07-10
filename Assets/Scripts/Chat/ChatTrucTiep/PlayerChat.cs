using Fusion;
using UnityEngine;

public class PlayerChat : NetworkBehaviour
{
    [SerializeField] private ChatBubble chatBubble;

    public override void Spawned()
    {
        // Chỉ gán cho player do client này điều khiển
        if (Object.HasInputAuthority)
        {
            var chatInputUI = FindFirstObjectByType<ChatInputUI>();
            if (chatInputUI != null)
                chatInputUI.SetPlayerChat(this);
        }
    }

    public void SendChat(string message)
    {
        if (!string.IsNullOrWhiteSpace(message))
        {
            RPC_ShowChat(message);
        }
    }

    [Rpc(RpcSources.All, RpcTargets.All)]
    public void RPC_ShowChat(string message)
    {
        if (chatBubble != null)
        {
            chatBubble.Show(message);
        }
    }
}
