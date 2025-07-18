using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class HealthBar : MonoBehaviour
{
    public Slider slider;
    public TextMeshProUGUI hpText;

    public Slider manaSlider;
    public TextMeshProUGUI manaText;

    public void SetHealth(int current, int max)
    {
        if (slider != null)
        {
            slider.maxValue = max;
            slider.value = current;
        }
        if (hpText != null)
        {
            hpText.text = $"{current}/{max}";
        }
    }
    public void SetMana(int current, int max)
    {
        if (manaSlider != null)
        {
            manaSlider.maxValue = max;
            manaSlider.value = current;
        }
        if (manaText != null)
        {
            manaText.text = $"{current}/{max}";
        }
    }
}
