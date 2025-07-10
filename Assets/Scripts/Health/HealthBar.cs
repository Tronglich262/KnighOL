using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class HealthBar : MonoBehaviour
{
    public Slider slider;
    public TextMeshProUGUI hpText;

    public Slider manaSlider;
    public TextMeshProUGUI manaText;
    public void Update()
    {
        if(Input.GetKeyDown(KeyCode.Y))
        {
            ThongTin.instance.currentHP -= 10;
            if (ThongTin.instance.currentHP < 0) ThongTin.instance.currentHP = 0;
            SetHealth(ThongTin.instance.currentHP, ThongTin.instance.maxHP);
        }
        if (Input.GetKeyDown(KeyCode.U))
        {
            ThongTin.instance.currentHP += 10;
            if (ThongTin.instance.currentHP > ThongTin.instance.maxHP) ThongTin.instance.currentHP = ThongTin.instance.maxHP;
            SetHealth(ThongTin.instance.currentHP, ThongTin.instance.maxHP);
        }
        if (Input.GetKeyDown(KeyCode.I))
        {
            ThongTin.instance.currentMana -= 10;
            if (ThongTin.instance.currentMana < 0) ThongTin.instance.currentMana = 0;
            SetMana(ThongTin.instance.currentMana, ThongTin.instance.maxMana);
        }
        if (Input.GetKeyDown(KeyCode.O))
        {
            ThongTin.instance.currentMana += 10;
            if (ThongTin.instance.currentMana > ThongTin.instance.maxMana)
                ThongTin.instance.currentMana = ThongTin.instance.maxMana;
            SetMana(ThongTin.instance.currentMana, ThongTin.instance.maxMana);
        }
    }
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
