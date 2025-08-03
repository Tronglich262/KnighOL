using System.Collections.Generic;
using UnityEngine;

public static class UIManager1
{
    private static List<UIPanel> panels = new List<UIPanel>();

    public static void RegisterPanel(UIPanel panel)
    {
        if (!panels.Contains(panel))
            panels.Add(panel);
    }

    public static void UnregisterPanel(UIPanel panel)
    {
        if (panels.Contains(panel))
            panels.Remove(panel);
    }

    public static void HideAllExcept(UIPanel activePanel)
    {
        foreach (var panel in panels)
        {
            if (panel == null) continue;

            if (panel != activePanel)
                panel.gameObject.SetActive(false);
        }
    }

    public static void ShowDefaultPanels()
    {
        foreach (var panel in panels)
        {
            if (panel != null && panel.panelType == PanelType.Default)
                panel.gameObject.SetActive(true);
        }
    }
}
