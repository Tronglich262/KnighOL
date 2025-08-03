using UnityEngine;
using TMPro;
using Assets.HeroEditor.Common.CommonScripts; // Bỏ nếu bạn dùng Text thường

public class LoadingDots : MonoBehaviour
{
    public TextMeshProUGUI loadingText; // Kéo text "Loading" vào
    public float interval = 0.3f;        // Thời gian giữa mỗi lần đổi

    private string baseText = "Loading";
    private int dotCount = 0;
    private int direction = 1; // 1 = tăng, -1 = giảm

    private float timer = 0f;
    private void Start()
    {
        loadingText.SetActive(false);
    }

    void Update()
    {
        timer += Time.deltaTime;

        if (timer >= interval)
        {
            timer = 0f;

            dotCount += direction;

            if (dotCount >= 3)
            {
                dotCount = 3;
                direction = -1;
            }
            else if (dotCount <= 0)
            {
                dotCount = 0;
                direction = 1;
            }

            loadingText.text = baseText + new string('.', dotCount);
        }
    }
    public void OnTexloading()
    {
        loadingText.SetActive (true);
    }
}
