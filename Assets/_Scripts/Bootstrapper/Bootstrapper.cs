// Scripts/Bootstrap/Bootstrapper.cs
using UnityEngine;
using UnityEngine.SceneManagement;

public class Bootstrapper : MonoBehaviour
{
    [Header("=== Core Services Prefabs ===")]
    public ItemStatDatabase itemStatDatabasePrefab;
    public PlayerSessionService playerSessionServicePrefab;

    [Header("=== Next Scene ===")]
    [Tooltip("Tên scene Login của bạn (thường là 'Login')")]
    public string nextSceneName = "Login";

    private void Awake()
    {
        DontDestroyOnLoad(gameObject);
        InitializeAllServices();
    }

    private void InitializeAllServices()
    {
        Debug.Log("[Bootstrapper] Khởi tạo Core Services...");

        // 1. ItemStatDatabase
        if (ItemStatDatabase.Instance == null && itemStatDatabasePrefab != null)
        {
            Instantiate(itemStatDatabasePrefab);
            Debug.Log("[Bootstrapper] → ItemStatDatabase đã tạo");
        }

        // 2. PlayerSessionService
        if (PlayerSessionService.Instance == null && playerSessionServicePrefab != null)
        {
            Instantiate(playerSessionServicePrefab);
            Debug.Log("[Bootstrapper] → PlayerSessionService đã tạo");
        }
        else if (PlayerSessionService.Instance == null)
        {
            var go = new GameObject("PlayerSessionService");
            go.AddComponent<PlayerSessionService>();
            Debug.Log("[Bootstrapper] → PlayerSessionService tạo fallback");
        }

        // 3. ApiConfigManager
        ApiConfigManager.GetInstance();

        Debug.Log("[Bootstrapper] ✅ TẤT CẢ CORE SERVICES ĐÃ KHỞI TẠO THÀNH CÔNG!");

        // ==================== TỰ ĐỘNG LOAD SCENE LOGIN ====================
        Invoke("LoadLoginScene", 0.3f); // delay nhẹ để log hiện rõ
    }

    private void LoadLoginScene()
    {
        if (!string.IsNullOrEmpty(nextSceneName))
        {
            Debug.Log($"[Bootstrapper] Đang chuyển sang scene: {nextSceneName}");
            SceneManager.LoadScene(nextSceneName);
        }
        else
        {
            Debug.LogError("[Bootstrapper] Chưa set tên scene Login!");
        }
    }
}