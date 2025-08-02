using TMPro;
using UnityEngine;

public class FpsGame : MonoBehaviour
{

    public TextMeshProUGUI fpsText;

    private float deltaTime;
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
}