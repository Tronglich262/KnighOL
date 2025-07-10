using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// Script quản lý nhiều danh sách UI (Panel).
/// Bấm button (không toggle) để mở panel tương ứng,
/// và tự động tắt các panel khác.
/// </summary>
public class ListControllerVK : MonoBehaviour
{
    public List<GameObject> allMyLists;
    private int currentActiveIndex = -1;

    void Start()
    {
        HideAll();
    }

    private void HideAll()
    {
        foreach (var list in allMyLists)
        {
            if (list != null)
                list.SetActive(false);
        }

        currentActiveIndex = -1;
    }

    /// <summary>
    /// Gọi từ Button (mỗi lần bấm là mở panel tương ứng)
    /// </summary>
    /// <param name="listIndex">Index panel muốn mở</param>
    public void ShowList(int listIndex)
    {
        if (listIndex < 0 || listIndex >= allMyLists.Count || allMyLists[listIndex] == null)
            return;

        // Nếu đang mở 1 panel khác → tắt nó
        if (currentActiveIndex != -1 && currentActiveIndex != listIndex)
        {
            var current = allMyLists[currentActiveIndex];
            if (current != null)
                current.SetActive(false);
        }

        // Nếu chưa mở panel nào hoặc mở panel khác → bật panel mới
        if (!allMyLists[listIndex].activeSelf)
        {
            allMyLists[listIndex].SetActive(true);
            currentActiveIndex = listIndex;
        }
        // Nếu đang bấm lại chính nó → không làm gì (giữ nguyên)
    }
}
