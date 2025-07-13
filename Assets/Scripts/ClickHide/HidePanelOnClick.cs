using UnityEngine;
using UnityEngine.EventSystems;

public class HidePanelOnClick : MonoBehaviour, IPointerClickHandler
{
    public GameObject panelToHide; // Kéo Panel popup vào

    public void OnPointerClick(PointerEventData eventData)
    {
        if (panelToHide != null)
            panelToHide.SetActive(false);
    }
}
