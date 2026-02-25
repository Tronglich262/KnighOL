using UnityEngine;
using UnityEngine.UI;
using Fusion;
using System;

[Serializable]
public class SkillSlot1
{
    public Image cooldownOverlay;
    public Image skillIcon;
    public Button button;
    public Text cooldownText;

    [HideInInspector] public int skillIndex = -1;
}

public class SkillCooldownUI : MonoBehaviour
{
    public SkillSlot1[] skillSlots;   // <-- thêm bao nhiêu skill cũng được
    public float cooldownDuration = 10f;

    private BuffSkillNetwork buff;

    void Start()
    {
        FindLocalPlayer();
    }

    void Update()
    {
        if (buff == null)
        {
            FindLocalPlayer();
            return;
        }

        for (int i = 0; i < skillSlots.Length; i++)
        {
            UpdateSlot(skillSlots[i]);
        }
    }

    void UpdateSlot(SkillSlot1 slot)
    {
        if (slot.skillIndex < 0 || slot.skillIndex >= buff.Cooldowns.Length)
        {
            ResetSlot(slot);
            return;
        }

        float remain = buff.Cooldowns[slot.skillIndex]
            .RemainingTime(buff.Runner) ?? 0;

        if (remain > 0)
        {
            float total = buff.skillCooldownTimes[slot.skillIndex];
            slot.cooldownOverlay.fillAmount = remain / total;
            slot.cooldownOverlay.gameObject.SetActive(true);

            slot.cooldownText.text = Mathf.Ceil(remain).ToString();
            slot.cooldownText.gameObject.SetActive(true);

            slot.button.interactable = false;
            slot.skillIcon.color = new Color(1, 1, 1, 0.5f);
        }
        else
        {
            ResetSlot(slot);
        }
    }

    public void SetSkillIndex(int slotIndex, int newSkillIndex)
    {
        if (slotIndex < 0 || slotIndex >= skillSlots.Length)
            return;

        skillSlots[slotIndex].skillIndex = newSkillIndex;
        ResetSlot(skillSlots[slotIndex]);
    }

    void ResetSlot(SkillSlot1 slot)
    {
        slot.cooldownOverlay.fillAmount = 0;
        slot.cooldownOverlay.gameObject.SetActive(false);
        slot.cooldownText.gameObject.SetActive(false);

        slot.button.interactable = true;
        slot.skillIcon.color = Color.white;
    }

    void FindLocalPlayer()
    {
        foreach (var p in GameObject.FindGameObjectsWithTag("Player"))
        {
            var net = p.GetComponent<NetworkObject>();
            if (net != null && net.HasInputAuthority)
            {
                buff = p.GetComponent<BuffSkillNetwork>();
                break;
            }
        }
    }
}