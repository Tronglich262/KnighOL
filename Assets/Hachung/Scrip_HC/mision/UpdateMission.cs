using UnityEngine;

public class UpdateMission : MonoBehaviour
{
  
    public GameObject TimeHcfalse;
    public GameObject TimeHcTrue;
    
    public GameObject ItemslotHcfalse;
    public GameObject ItemslotHcTrue;
 
    public void TimeHc()
    {
        TimeHcfalse.SetActive(true);
        TimeHcTrue.SetActive(false);
    }
    public void slotItemHc()
    {
        ItemslotHcfalse.SetActive(true);
        ItemslotHcTrue.SetActive(false);
        Debug.Log("đã oke");
    }

    public void DeathBossHc()
    {
        
    }

}
