using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class dataMission : MonoBehaviour
{
    public ScriptableObject Mission;                 
    public List<TextMeshProUGUI> MissionTextList;    
    void Start()
    {
        var missionData = Mission as MissionScriptableObjetc;

        if (missionData != null && MissionTextList.Count >= 3)
        {
            MissionTextList[0].text = missionData.missionTitle;
            MissionTextList[1].text = missionData.missionTitle1;
            MissionTextList[2].text = missionData.missionTitle2;
        }
      
    }
}