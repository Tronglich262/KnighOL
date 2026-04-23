using UnityEngine;
using TMPro;
using Fusion;

public class PlayerLevelUI : MonoBehaviour
{
    public TextMeshProUGUI levelText;
    public static PlayerLevelUI Instante;
    public void Awake()
    {
        Instante = this;
    }
    void Update()
    {


        var state = PlayerDataHolder1.CurrentPlayerState;
        if (state == null) return;

        int expMax = ExpToNextLevel(state.level);
        float expPercent = expMax > 0 ? (float)state.exp / expMax * 100f : 0;

        levelText.text = $"Lv: {state.level} ({expPercent:F1}%)";
    }
    public  int ExpToNextLevel(int level)
    {
        return 100 * level;
    }
}

