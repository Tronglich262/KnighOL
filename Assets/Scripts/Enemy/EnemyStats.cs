using Fusion;

public class EnemyStats : NetworkBehaviour
{
    [Networked] public int MaxHP { get; set; }
    [Networked] public int HP { get; set; }
    [Networked] public int Attack { get; set; }

    public void Init(int monsterId)
    {
        MaxHP = 500;
        HP = MaxHP;
        Attack = 50;
    }
}
