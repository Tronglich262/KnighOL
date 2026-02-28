using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Hiển thị icon debuff trên đầu enemy. Gắn lên Canvas con của enemy (world space).
/// Gán Sprite cho mỗi loại debuff trong Inspector; khi xong có thể thêm icon tượng trưng.
/// </summary>
public class DebuffIconDisplay : MonoBehaviour
{
    [Header("Icon (gán sau)")]
    public Image stunIcon;
    public Image burnIcon;
    public Image dizzyIcon;

    [Header("Thời gian còn lại (tùy chọn)")]
    public TextMeshProUGUI stunText;
    public TextMeshProUGUI burnText;
    public TextMeshProUGUI dizzyText;

    public void SetStunActive(bool active)
    {
        if (stunIcon != null) stunIcon.gameObject.SetActive(active);
        if (stunText != null) stunText.gameObject.SetActive(active);
    }

    public void SetBurnActive(bool active)
    {
        if (burnIcon != null) burnIcon.gameObject.SetActive(active);
        if (burnText != null) burnText.gameObject.SetActive(active);
    }

    public void SetDizzyActive(bool active)
    {
        if (dizzyIcon != null) dizzyIcon.gameObject.SetActive(active);
        if (dizzyText != null) dizzyText.gameObject.SetActive(active);
    }

    public void SetRemainingTimes(float stunRem, float burnRem, float dizzyRem)
    {
        if (stunText != null && stunText.gameObject.activeInHierarchy)
            stunText.text = stunRem > 0f ? stunRem.ToString("F1") : "";
        if (burnText != null && burnText.gameObject.activeInHierarchy)
            burnText.text = burnRem > 0f ? burnRem.ToString("F1") : "";
        if (dizzyText != null && dizzyText.gameObject.activeInHierarchy)
            dizzyText.text = dizzyRem > 0f ? dizzyRem.ToString("F1") : "";
    }
}
