using Fusion;
using TMPro;
using UnityEngine;

public class NameTagManager : NetworkBehaviour
{
    [Networked] public string Nickname { get; set; }

    public TextMeshProUGUI nameText;  // Kéo đúng text vào inspector (Không dùng GetComponentInChildren cho chắc ăn!)
    public CanvasGroup canvasGroup;

    public override void Spawned()
    {
        UpdateNameTag();
    }

    public override void FixedUpdateNetwork()
    {
        UpdateNameTag();
    }

    private void UpdateNameTag()
    {
        if (nameText != null)
        {
            nameText.text = Nickname;
        }
    }

    [Rpc(sources: RpcSources.InputAuthority, targets: RpcTargets.StateAuthority)]
    public void RPC_SetNickname(string name)
    {
        Nickname = name;
    }
    private void LateUpdate()
    {
        if (canvasGroup.alpha > 0 && Camera.main != null)
        {
            transform.forward = Camera.main.transform.forward;
        }
        if (transform.parent != null)
        {
            Vector3 parentScale = transform.parent.lossyScale;
            Vector3 myScale = transform.localScale;
            myScale.x = Mathf.Sign(parentScale.x) * Mathf.Abs(myScale.x);
            transform.localScale = myScale;
        }
    }
}
