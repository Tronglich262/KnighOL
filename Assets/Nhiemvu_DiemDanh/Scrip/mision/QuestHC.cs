using UnityEngine;

[CreateAssetMenu(fileName = "NewTimeQuest", menuName = "Quest/Time Quest")]
public class QuestHC : ScriptableObject
{
    public string questName;
    public float requiredTimeInMinutes = 15f;
    public bool isCompleted = false;
}