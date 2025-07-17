[System.Serializable]
public class QuestResponse
{
    public int quest_ID;
    public string name;
    public string description;
    public string targetType;
    public int targetId;
    public int targetAmount;
    public int progress;
    public bool is_completed;
    public int reward_gold;
    public int reward_exp;

}
