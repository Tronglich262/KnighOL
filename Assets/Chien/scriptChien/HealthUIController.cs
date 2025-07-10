using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class HealthUIController : MonoBehaviour
{
    public static HealthUIController Instance;

    [Header("UI Components")]
    public Slider healthSlider;
    public TMP_Text healthText; // <-- Thêm Text để hiển thị máu

    private float targetHealth = 100f;
    private float displayHealth = 100f;
    private float maxHealth = 100f;
    private float lerpSpeed = 5f;

    private void Awake()
    {
        Instance = this;
    }

    /// <summary>
    /// Gọi từ HealthSystem để cập nhật máu hiện tại và tối đa
    /// </summary>
    public void SetHealth(float current, float max)
    {
        targetHealth = Mathf.Clamp(current, 0, max);
        maxHealth = max;
    }

    void Update()
    {
        if (healthSlider == null) return;

        // Mượt mà hiệu ứng thay đổi thanh máu
        displayHealth = Mathf.Lerp(displayHealth, targetHealth, Time.deltaTime * lerpSpeed);

        // Snap về đúng nếu gần
        if (Mathf.Abs(displayHealth - targetHealth) < 0.05f)
        {
            displayHealth = targetHealth;
        }

        healthSlider.maxValue = maxHealth;
        healthSlider.value = displayHealth;

        // Cập nhật text
        if (healthText != null)
        {
            healthText.text = $"{Mathf.CeilToInt(displayHealth)} / {Mathf.CeilToInt(maxHealth)}";
        }
    }
}

