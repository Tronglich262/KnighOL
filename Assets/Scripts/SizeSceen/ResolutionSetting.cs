using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class ResolutionSetting : MonoBehaviour
{
    public Dropdown resolutionDropdown;

    // 3 độ phân giải cố định
    private readonly Vector2Int[] availableResolutions = new Vector2Int[]
    {
        new Vector2Int(1280, 720),
        new Vector2Int(1600, 900),
        new Vector2Int(1920, 1080)
    };

    void Start()
    {
        // Cài đặt option cho Dropdown
        resolutionDropdown.ClearOptions();
        foreach (var res in availableResolutions)
        {
            resolutionDropdown.options.Add(new Dropdown.OptionData($"{res.x} x {res.y}"));
        }

        // Gán sự kiện đổi resolution
        resolutionDropdown.onValueChanged.AddListener(OnResolutionChange);

        // Tự động chọn đúng độ phân giải hiện tại nếu trùng với 1 trong 3 cái
        for (int i = 0; i < availableResolutions.Length; i++)
        {
            if (Screen.width == availableResolutions[i].x && Screen.height == availableResolutions[i].y)
            {
                resolutionDropdown.value = i;
                break;
            }
        }
        resolutionDropdown.RefreshShownValue();
    }

    void OnResolutionChange(int index)
    {
        var res = availableResolutions[index];
        Screen.SetResolution(res.x, res.y, Screen.fullScreen);
    }
}