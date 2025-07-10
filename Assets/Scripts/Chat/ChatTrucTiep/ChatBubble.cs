using UnityEngine;
using TMPro;

public class ChatBubble : MonoBehaviour
{
    public TextMeshProUGUI chatText;
    public CanvasGroup canvasGroup;
    public float displayTime = 3f;

    private void Awake()
    {
        Hide();
    }

    public void Show(string message)
    {
        chatText.text = message;
        canvasGroup.alpha = 1;
        CancelInvoke();
        Invoke(nameof(Hide), displayTime);
    }

    public void Hide()
    {
        canvasGroup.alpha = 0;
    }

    // Bubble luôn hướng về camera
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
