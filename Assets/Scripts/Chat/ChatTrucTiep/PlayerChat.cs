using Fusion;
using UnityEngine;

public class PlayerChat : NetworkBehaviour
{
    [SerializeField] private ChatBubble chatBubble;

    public override void Spawned()
    {
        if (Object.HasInputAuthority)
        {
            var chatInputUI = FindFirstObjectByType<ChatInputUI>();
            if (chatInputUI != null)
                chatInputUI.SetPlayerChat(this);
        }
    }

    public void SendChat(string message)
    {
        if (!Object.HasInputAuthority) return;

        message = message?.Trim();
        if (string.IsNullOrEmpty(message)) return;

        RPC_ShowChat(message);
    }

    [Rpc(RpcSources.InputAuthority, RpcTargets.All)]
    public void RPC_ShowChat(string message)
    {
        if (chatBubble != null)
            chatBubble.Show(message);
    }
}
