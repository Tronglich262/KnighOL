using UnityEngine;

public class BuyButton : MonoBehaviour
{
    private EquipmentSlotUI selectedSlot;

    public void SetSelectedSlot(EquipmentSlotUI slot)
    {
        selectedSlot = slot;
    }

    public void OnBuy()
    {
        if (selectedSlot != null)
        {
            // Gọi logic thêm vào inventory nếu cần ở đây...

            // Ẩn item đã mua khỏi shop
            selectedSlot.gameObject.SetActive(false);

            // Ẩn panel chi tiết nếu đang hiển thị
            ItemDetailsPanel.Instance.Hide();
        }
    }
}
