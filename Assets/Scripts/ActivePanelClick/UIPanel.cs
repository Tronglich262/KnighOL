using UnityEngine;

public class UIPanel : MonoBehaviour
{
    public PanelType panelType = PanelType.Overlay;

    private void OnEnable()
    {
        UIManager1.RegisterPanel(this);
    }

    private void OnDisable()
    {
        UIManager1.UnregisterPanel(this);
    }

    public void OpenPanel()
    {
        UIManager1.HideAllExcept(this);
        gameObject.SetActive(true);
    }

    public void ClosePanel()
    {
        gameObject.SetActive(false);
        UIManager1.ShowDefaultPanels();
    }
}
