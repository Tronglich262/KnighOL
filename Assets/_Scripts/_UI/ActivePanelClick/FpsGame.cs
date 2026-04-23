using TMPro;
using UnityEngine;

public class FpsGame : MonoBehaviour
{

    public TextMeshProUGUI fpsText;
    public GameObject BangHienThiThongTin;
    private float deltaTime;
    public static FpsGame Instance { get; private set; }
    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }
    private void Start()
    {
        Application.targetFrameRate = 240;
        QualitySettings.vSyncCount = 0; // Tắt V-Sync để FPS limit có hiệu lực

    }
    void Update()
    {
        deltaTime += (Time.unscaledDeltaTime - deltaTime) * 0.1f;

        float fps = 1.0f / deltaTime;
        fpsText.text = $"FPS: {Mathf.Ceil(fps)}";
    }
    //bat tat bang thong tin
    public void ToggleTatBanghienthithongtin()
    {
        BangHienThiThongTin.SetActive(false);
    }
    public void ToggleBatBanghienthithongtin() 
    { 
  
        BangHienThiThongTin.SetActive(true);
    }
}