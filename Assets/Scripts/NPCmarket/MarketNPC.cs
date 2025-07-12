using UnityEngine;

public class MarketNPC : MonoBehaviour
{
    public GameObject shopUIPanel;
    private bool playerInRange = false;

    void Start()
    {
        if (shopUIPanel != null) shopUIPanel.SetActive(false);
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            playerInRange = true;
            // Hiện thông báo UI: "Nhấn E để mở Market"
        }
    }

    void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            playerInRange = false;
            if (shopUIPanel != null) shopUIPanel.SetActive(false);
            // Ẩn thông báo UI
        }
    }
    public void ToggleTat()
    {
        shopUIPanel.SetActive(false);
        SkillButtonManager.Instance.Skillbutton.SetActive(true);
    }
    void Update()
    {
        if (playerInRange && Input.GetKeyDown(KeyCode.E))
        {
            if (shopUIPanel != null) shopUIPanel.SetActive(true);
            SkillButtonManager.Instance.Skillbutton.SetActive(false);
        }
    }
}
