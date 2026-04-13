using UnityEngine;
using TMPro;

public class ChatBubble : MonoBehaviour
{
    public TextMeshProUGUI chatText;
    public CanvasGroup canvasGroup;
    public float displayTime = 3f;

    private Camera mainCam;

    private void Awake()
    {
        mainCam = Camera.main;
        Hide();
    }

    public void Show(string message)
    {
        chatText.text = message;
        canvasGroup.alpha = 1f;
        CancelInvoke();
        Invoke(nameof(Hide), displayTime);
    }

    public void Hide()
    {
        canvasGroup.alpha = 0f;
    }

    private void LateUpdate()
    {
        if (canvasGroup.alpha > 0f)
        {
            if (mainCam == null)
                mainCam = Camera.main;

            if (mainCam != null)
                transform.forward = mainCam.transform.forward;
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