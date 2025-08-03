using Fusion;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SettingPanel : MonoBehaviour
{
    public static SettingPanel Instance;
    public GameObject Setting;
    private void Awake()
    {
            Instance = this;
    }
    public GameObject settingPanel;
    public NetworkRunner runner;
    void Start()
    {
        settingPanel.SetActive(false);
    }

    public void showSettingPanel()
    {
        settingPanel.SetActive(true);
    }

    public void offSettingPanel()
    {
        settingPanel.SetActive(false);
    }
    public async void ToggleDangXuat()
    {
        // Despawn player object của mình nếu còn
        if (runner != null && PlayerSpawner.LocalPlayerObject != null)
        {
            runner.Despawn(PlayerSpawner.LocalPlayerObject);
            PlayerSpawner.LocalPlayerObject = null; // Tránh lỗi double despawn nếu logout nhiều lần
        }

        // Shutdown runner (ngắt kết nối)
        if (runner != null)
        {
            await runner.Shutdown();
        }

        // Xóa session nếu có
        PlayerPrefs.DeleteKey("accessToken");

        // Quay về login/menu
        SceneManager.LoadScene("Login");
    }
}
