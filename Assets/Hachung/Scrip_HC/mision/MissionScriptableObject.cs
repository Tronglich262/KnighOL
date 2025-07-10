using UnityEngine;

[CreateAssetMenu(fileName = "NewTimeQuest", menuName = "Quest/mission ")]
public class MissionScriptableObjetc : ScriptableObject
{
    [Header("3 đoạn text nhiệm vụ")]
    [TextArea] public string missionTitle;  
    [TextArea] public string missionTitle1;  
    [TextArea] public string missionTitle2;     
}