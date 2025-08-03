using UnityEngine;
using TMPro;

public class QuestManager : MonoBehaviour
{ 
    // đoạn code này là về thời gian chơi được 15' thì nhận thuởng
    public QuestHC questData; 
    private float playTime = 0f;
    public UpdateMission updateMission;
    private void Update()
    {
        if (questData.isCompleted) return;

        playTime += Time.deltaTime;

        if (playTime >= questData.requiredTimeInMinutes * 60f)
        {
            CompleteQuest();
        }
    }
    private void CompleteQuest()
    {
        questData.isCompleted = true;
        updateMission.TimeHc();
    }


    
}
