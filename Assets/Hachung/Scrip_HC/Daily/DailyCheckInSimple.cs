using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class DailyCheckInSimple : MonoBehaviour
{
    private string playerKeyPrefix;

    [Header("7 ô phần thưởng (GameObject) tương ứng từng ngày")]
    public List<GameObject> rewardDays = new List<GameObject>();

    [Header("7 ô thông báo (GameObject) tương ứng từng ngày")]
    public List<GameObject> Notification = new List<GameObject>();

    [Header("7 ô thông báo Clamed (GameObject) tương ứng từng ngày")]
    public List<GameObject> Clamed = new List<GameObject>();

    [Header("Hiển thị số ngày hiện tại (Text)")]
    public Text dayText;

    private int clamedIndex = 0;
    private int currentDay = 0;


    //text
    public GameObject shopNotifyPanel;      // Toàn bộ panel (cả nền và text)
    public Image shopNotifyBg;              // Image nền
    public TextMeshProUGUI shopNotifyText;  // Text để hiện thông báo
    private Coroutine notifyCoroutine;

    private void Start()
    {
        // Prefix theo tên người chơi
        if (!string.IsNullOrEmpty(PlayerDataHolder1.PlayerName))
        {
            playerKeyPrefix = PlayerDataHolder1.PlayerName + "_";
        }
        else
        {
            playerKeyPrefix = "Guest_";
            Debug.LogWarning("⚠️ PlayerName rỗng – đang dùng fallback prefix.");
        }

        clamedIndex = PlayerPrefs.GetInt(playerKeyPrefix + "ClamedIndex", 0);

        // Gán currentDay trước để xử lý Notification chính xác
        CheckLoginDay();

        // Hiện các Clamed đã nhận
        for (int i = 0; i < clamedIndex && i < Clamed.Count; i++)
        {
            Clamed[i].SetActive(true);
        }

        // Bật Notification chỉ khi chưa nhận thưởng hôm nay
        for (int i = 0; i < Notification.Count; i++)
        {
            Notification[i].SetActive(i == currentDay - 1 && clamedIndex < currentDay);
        }

        ShowCurrentDay();
    }

    private void CheckLoginDay()
    {
        string today = DateTime.Now.ToString("yyyyMMdd");
        string lastLogin = PlayerPrefs.GetString(playerKeyPrefix + "LastLoginDate", "");

        if (lastLogin != today)
        {
            currentDay = PlayerPrefs.GetInt(playerKeyPrefix + "LoginDayIndex", 0);
            currentDay = Mathf.Clamp(currentDay + 1, 1, 7); // Từ 1 đến 7

            PlayerPrefs.SetInt(playerKeyPrefix + "LoginDayIndex", currentDay);
            PlayerPrefs.SetString(playerKeyPrefix + "LastLoginDate", today);
            PlayerPrefs.Save();

            Debug.Log("🎯 Đăng nhập mới - Ngày điểm danh: " + currentDay);
        }
        else
        {
            currentDay = PlayerPrefs.GetInt(playerKeyPrefix + "LoginDayIndex", 1);
            Debug.Log("⏳ Đã login hôm nay - Không tăng ngày.");
        }
    }

    private void ShowCurrentDay()
    {
        for (int i = 0; i < rewardDays.Count; i++)
        {
            rewardDays[i].SetActive(i != currentDay - 1);
        }

        // Chỉ bật thông báo nếu chưa nhận ngày hôm nay
        for (int i = 0; i < Notification.Count; i++)
        {
            Notification[i].SetActive(i == currentDay - 1 && clamedIndex < currentDay);
        }

        if (dayText != null)
        {
            dayText.text = "Ngày hiện tại: Day " + currentDay;
        }
    }

    public void ClamedDaily()
    {
        if (clamedIndex >= currentDay)
        {
            Debug.Log("❌ Đã nhận phần thưởng hôm nay rồi.");
            return;
        }

        if (clamedIndex >= Clamed.Count)
        {
            Debug.Log("✅ Tất cả các Clamed đã được bật.");
            return;
        }

        Clamed[clamedIndex].SetActive(true);
        Debug.Log("✅ Đã nhận phần thưởng ngày " + (clamedIndex + 1));
        int ifor = clamedIndex + 1;
        clamedIndex++;
        ShowShopNotify($"Nhận điểm danh thành công ngày {ifor}");

        // Tắt tất cả thông báo sau khi nhận
        for (int i = 0; i < Notification.Count; i++)
        {
            Notification[i].SetActive(false);
        }

        PlayerPrefs.SetInt(playerKeyPrefix + "ClamedIndex", clamedIndex);
        PlayerPrefs.Save();
    }

    public void faleCurrentDat()
    {
        for (int i = 0; i < rewardDays.Count; i++)
        {
            rewardDays[i].SetActive(true);
        }
    }

    public void NextTestDayButton()
    {
        currentDay = PlayerPrefs.GetInt(playerKeyPrefix + "LoginDayIndex", 1);
        currentDay = Mathf.Clamp(currentDay + 1, 1, 7);

        PlayerPrefs.SetInt(playerKeyPrefix + "LoginDayIndex", currentDay);
        PlayerPrefs.SetString(playerKeyPrefix + "LastLoginDate", Guid.NewGuid().ToString());
        PlayerPrefs.Save();

        Debug.Log("🧪 Test next day: " + currentDay);
        ShowCurrentDay();
    }

    public void ResetLoginProgress()
    {
        PlayerPrefs.DeleteKey(playerKeyPrefix + "LoginDayIndex");
        PlayerPrefs.DeleteKey(playerKeyPrefix + "LastLoginDate");
        PlayerPrefs.DeleteKey(playerKeyPrefix + "ClamedIndex");
        PlayerPrefs.Save();

        currentDay = 0;
        clamedIndex = 0;

        foreach (var reward in rewardDays)
        {
            reward.SetActive(false);
        }

        foreach (var notif in Notification)
        {
            notif.SetActive(false);
        }

        foreach (var claim in Clamed)
        {
            claim.SetActive(false);
        }

        Debug.Log("🧹 Đã reset ngày điểm danh.");
        CheckLoginDay();
        ShowCurrentDay();
    }
    //text lichtrong thêm
    public void ShowShopNotify(string message, float typeSpeed = 0.03f, float stayTime = 1.3f, float fadeTime = 0.28f)
    {
        Debug.Log($"ShowShopNotify: {message}, panel={shopNotifyPanel}, bg={shopNotifyBg}, text={shopNotifyText}");

        if (notifyCoroutine != null) StopCoroutine(notifyCoroutine);
        notifyCoroutine = StartCoroutine(ShowShopNotifyRoutine(message, typeSpeed, stayTime, fadeTime));
    }

    private IEnumerator ShowShopNotifyRoutine(string message, float typeSpeed, float stayTime, float fadeTime)
    {
        shopNotifyPanel.SetActive(true);

        // Reset opacity (ẩn panel)
        Color cBg = shopNotifyBg.color; cBg.a = 0; shopNotifyBg.color = cBg;
        Color cText = shopNotifyText.color; cText.a = 0; shopNotifyText.color = cText;
        shopNotifyText.text = "";

        // Fade in nền + text (cùng lúc)
        float t = 0f;
        while (t < fadeTime)
        {
            float a = t / fadeTime;
            cBg.a = Mathf.Lerp(0, 1, a);
            cText.a = Mathf.Lerp(0, 1, a);
            shopNotifyBg.color = cBg;
            shopNotifyText.color = cText;
            t += Time.deltaTime;
            yield return null;
        }
        cBg.a = 1; cText.a = 1;
        shopNotifyBg.color = cBg;
        shopNotifyText.color = cText;

        // Hiệu ứng chữ chạy
        shopNotifyText.text = "";
        foreach (char ch in message)
        {
            shopNotifyText.text += ch;
            yield return new WaitForSeconds(typeSpeed);
        }

        // Chờ text stayTime giây
        yield return new WaitForSeconds(stayTime);

        // Fade out
        t = 0f;
        while (t < fadeTime)
        {
            float a = 1 - t / fadeTime;
            cBg.a = Mathf.Lerp(0, 1, a);
            cText.a = Mathf.Lerp(0, 1, a);
            shopNotifyBg.color = cBg;
            shopNotifyText.color = cText;
            t += Time.deltaTime;
            yield return null;
        }
        shopNotifyPanel.SetActive(false);
    }
}
