using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;

public class LoadingScreen : MonoBehaviour
{
    public Slider loadingSlider;
    public TextMeshProUGUI loadingText;

    public float loadDuration = 5f; // Tổng thời gian loading giả lập
    public Vector2 textScaleRange = new Vector2(1f, 1.3f); // Phạm vi scale cho animation

    public GameObject objectToActivateAfterLoad; // Việc cần làm sau khi load xong
    public GameObject loading;

    private float elapsedTime = 0f;

    void Start()
    {
        StartCoroutine(LoadRoutine());
    }

    IEnumerator LoadRoutine()
    {
        while (elapsedTime < loadDuration)
        {
            float delta = Time.deltaTime;

            float progress = Mathf.Clamp01(elapsedTime / loadDuration);

            // Làm chậm lại sau khi đạt 90%
            if (progress >= 0.9f)
            {
                delta *= 0.1f; // Giảm tốc độ còn 30% để tạo cảm giác chờ đợi
            }

            elapsedTime += delta;

            // Cập nhật Slider
            progress = Mathf.Clamp01(elapsedTime / loadDuration);
            loadingSlider.value = progress;

            // Animate "Loading..." Text
            float scale = Mathf.Lerp(textScaleRange.x, textScaleRange.y, (Mathf.Sin(Time.time * 3f) + 1f) / 2f);
            loadingText.transform.localScale = new Vector3(scale, scale, 1f);

            yield return null;
        }

        // Load complete
        loadingSlider.value = 1f;
        loadingText.text = "Done!";

        yield return new WaitForSeconds(0.5f);

        if (objectToActivateAfterLoad != null)
        {
            objectToActivateAfterLoad.SetActive(true);
        }

        // Ẩn màn hình loading
        loading.SetActive(false);
    }
}
