using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class EquipmentSlotCache : MonoBehaviour
{
    public Image iconImage;
    public TextMeshProUGUI label;
    public EquipmentSlotUI equipmentSlotUI;

    private void Reset()
    {
        if (iconImage == null)
            iconImage = GetComponentInChildren<Image>(true);

        if (label == null)
            label = GetComponentInChildren<TextMeshProUGUI>(true);

        if (equipmentSlotUI == null)
            equipmentSlotUI = GetComponent<EquipmentSlotUI>();
    }
}