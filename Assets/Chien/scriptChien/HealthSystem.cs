using Fusion;
using UnityEngine;

public class HealthSystem : NetworkBehaviour
{
    [Networked] public float CurrentHealth { get; set; }

    private float maxHealth;
    private HealthUIController uiController;

    public override void Spawned()
    {
        Debug.Log("[HealthSystem] Spawned() chạy!");

        if (Object.HasInputAuthority)
        {
            uiController = HealthUIController.Instance;

            if (uiController != null)
            {
                Debug.Log("[HealthSystem] Đã gắn UIController.Instance thành công.");
            }
            else
            {
                Debug.LogError("[HealthSystem] UIController.Instance bị null! Không tìm thấy HealthUI.");
            }
        }

        // ❌ KHÔNG gán máu ở đây nữa vì chỉ số chưa được gán tại thời điểm này
    }

    public void InitHealthFromStats()
    {
        var stats = GetComponent<CharacterStats>();
        if (stats != null)
        {
            maxHealth = stats.vitality + stats.finalVitality;
        }
        CurrentHealth = maxHealth;
        Debug.Log($"[HealthSystem] InitHealthFromStats: maxHealth = {maxHealth}");
    }

    void Update()
    {
        if (Object.HasInputAuthority && uiController != null)
        {
            uiController.SetHealth(CurrentHealth, maxHealth);
        }

        if (Object.HasInputAuthority && Input.GetKeyDown(KeyCode.P))
        {
            Debug.Log("[HealthSystem] Nhấn P để trừ máu!");
            TakeDamage(10f);
        }
    }

    public void TakeDamage(float amount)
    {
        if (HasStateAuthority)
        {
            float before = CurrentHealth;
            CurrentHealth = Mathf.Max(0, CurrentHealth - amount);
            Debug.Log($"[HealthSystem] Máu trước: {before}, sau: {CurrentHealth}");
        }
    }
}

