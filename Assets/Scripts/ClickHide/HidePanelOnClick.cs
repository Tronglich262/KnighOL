using UnityEngine;
using UnityEngine.EventSystems;

public class HidePanelOnClick : MonoBehaviour, IPointerClickHandler
{
    public GameObject panelToHide; // K�o Panel popup v�o

    public void OnPointerClick(PointerEventData eventData)
    {
        if (panelToHide != null)
            panelToHide.SetActive(false);
    }
}
