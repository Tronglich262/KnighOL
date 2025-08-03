using UnityEngine;

// Đặt tên file là ToggleListVisibility.cs
public class ToggleListVisibility : MonoBehaviour
{
    // Tham chiếu đến GameObject chứa toàn bộ Scroll View mà bạn muốn ẩn/hiện.
    // Kéo thả đối tượng Scroll View vào đây trong Inspector của Unity.
    public GameObject listContainerObject;

    // Hàm này được gọi một lần khi game bắt đầu
    void Start()
    {
        // Mặc định, chúng ta sẽ tắt (ẩn) danh sách đi khi game bắt đầu.
        // Nếu bạn muốn nó hiện sẵn thì có thể xóa dòng này đi.
        if (listContainerObject != null)
        {
            listContainerObject.SetActive(false);
        }
    }

    // Đây là hàm public mà chúng ta sẽ gọi từ sự kiện OnClick của Button.
    public void ToggleVisibility()
    {
        // Kiểm tra xem đối tượng có đang được tham chiếu không
        if (listContainerObject != null)
        {
            // Lấy trạng thái active hiện tại và đảo ngược nó.
            // Nếu đang true (hiện) -> thành false (ẩn).
            // Nếu đang false (ẩn) -> thành true (hiện).
            bool isActive = listContainerObject.activeSelf;
            listContainerObject.SetActive(!isActive);
        }
        else
        {
            Debug.LogWarning("Chưa gán đối tượng listContainerObject trong Inspector!");
        }
    }
}