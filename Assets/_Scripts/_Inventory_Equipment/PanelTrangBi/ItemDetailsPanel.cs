
//new gỡ trang bị swap
using Newtonsoft.Json;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
// hiển thị bảng panel thông tin trang bị nhân vật
public class ItemDetailsPanel : MonoBehaviour
{
    public static ItemDetailsPanel Instance;

    public GameObject panel;
    public Image icon;
    public TMP_Text description;
    public TMP_Text level;
    public TMP_Text Type;
    public TMP_Text Name;
    public Button unequipButton; // Gán trong Inspector
    private string currentItemId;
    private string currentType;

    //text
    public TextMeshProUGUI equipMessageText;
    private Coroutine equipMessageCoroutine;
    private Vector3 equipMsgOriginPos;
    public bool ischeckgodo = false;
    private void Awake()
    {
        Instance = this;
        panel.SetActive(false);
    }

    public void Show(string id, Sprite iconSprite, string type = null)
    {
        if (!panel.activeSelf) panel.SetActive(true);
        Debug.Log($"da goi");
        icon.sprite = iconSprite;

        currentItemId = id;
        currentType = type;


        var stats = ItemStatDatabase.GetOrCreate().GetStats(id);
        string name = id.Split('.').Length > 0 ? id.Split('.').Last() : id;
        string displayType = type ?? "Không rõ loại";

        description.text = $"{displayType}\n\n{GetStatsFromId(id)}";
        Type.text = $"Loại: {displayType}";
        Name.text = $"Tên: {name}";
        level.text = $"Cấp yêu cầu: {stats?.LevelRequired ?? 0}";
    }


    public void Hide()
    {
        panel.SetActive(false);
    }
   

    private string GetStatsFromId(string id)
    {
        var stats = ItemStatDatabase.GetOrCreate().GetStats(id);
        if (stats == null)
            return "Không có thông tin.";

        return
            $"Sức mạnh: {stats.Strength}\n" +
            $"Phòng thủ: {stats.Defense}\n" +
            $"Nhanh nhẹn: {stats.Agility}\n" +
            $"Trí tuệ: {stats.Intelligence}\n" +
            $"Thể lực: {stats.Vitality}";
    }
    //gỡ ttrang bị 
    public void OnUnequipButtonClick()
    {
        if (string.IsNullOrEmpty(currentType))
            return;

        bool ok = EquipmentCoordinator.Unequip(currentType, out string message);
        ShowEquipMessage(message);

        if (ok)
            Hide();
    }

    // THEM TU CODE B
    public bool IsVisible()
    {
        return panel != null && panel.activeSelf;
    }

    public bool IsShowingItem(string id)
    {
        return currentItemId == id;
    }

    //hiệu ứng text 
    public void ShowEquipMessage(string msg, float duration = 2.5f)
    {
        if (equipMessageCoroutine != null) StopCoroutine(equipMessageCoroutine);
        equipMessageCoroutine = StartCoroutine(FlyUpEquipMessage(msg, duration));
    }
    IEnumerator FlyUpEquipMessage(string msg, float duration)
    {
        // Set lại vị trí, scale, alpha ban đầu mỗi lần gọi
        equipMessageText.text = msg;
        var rect = equipMessageText.rectTransform;
        rect.anchoredPosition = equipMsgOriginPos;
        equipMessageText.color = new Color(1, 1, 1, 0);
        equipMessageText.transform.localScale = Vector3.one * 1.15f;

        // Fade in + scale in (0.15s)
        float t = 0f;
        while (t < 0.15f)
        {
            equipMessageText.color = new Color(1, 1, 1, Mathf.Lerp(0, 1, t / 0.15f));
            equipMessageText.transform.localScale = Vector3.Lerp(Vector3.one * 1.15f, Vector3.one, t / 0.15f);
            t += Time.deltaTime;
            yield return null;
        }
        equipMessageText.color = new Color(1, 1, 1, 1);
        equipMessageText.transform.localScale = Vector3.one;

        // Bay lên (move y lên), giữ trong duration-0.3s
        float moveTime = duration - 0.3f;
        float yStart = equipMsgOriginPos.y;
        float yEnd = yStart + 60f; // Bay lên 60 đơn vị pixel, tuỳ UI chỉnh lại số này
        t = 0f;
        while (t < moveTime)
        {
            float percent = t / moveTime;
            float y = Mathf.Lerp(yStart, yEnd, percent);
            rect.anchoredPosition = new Vector2(rect.anchoredPosition.x, y);
            t += Time.deltaTime;
            yield return null;
        }
        rect.anchoredPosition = new Vector2(rect.anchoredPosition.x, yEnd);

        // Fade out + scale out (0.15s)
        t = 0f;
        while (t < 0.15f)
        {
            equipMessageText.color = new Color(1, 1, 1, Mathf.Lerp(1, 0, t / 0.15f));
            equipMessageText.transform.localScale = Vector3.Lerp(Vector3.one, Vector3.one * 0.95f, t / 0.15f);
            t += Time.deltaTime;
            yield return null;
        }
        equipMessageText.text = "";
        equipMessageText.color = new Color(1, 1, 1, 0);
        rect.anchoredPosition = equipMsgOriginPos; // Reset lại vị trí cho lần sau
    }
}