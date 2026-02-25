using Fusion;

public class AutoDespawn : NetworkBehaviour
{
    private TickTimer lifeTimer;

    public float lifeTime = 5f;

    public override void Spawned()
    {
        if (HasStateAuthority)
        {
            lifeTimer = TickTimer.CreateFromSeconds(Runner, lifeTime);
        }
    }

    public override void FixedUpdateNetwork()
    {
        if (HasStateAuthority && lifeTimer.Expired(Runner))
        {
            Runner.Despawn(Object);
        }
    }
}