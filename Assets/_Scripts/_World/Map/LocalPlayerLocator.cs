using UnityEngine;

public class LocalPlayerLocator : MonoBehaviour
{
    public static PlayerMapState LocalMapState;

    public static void Register(PlayerMapState mapState)
    {
        if (mapState.Object != null && mapState.Object.HasInputAuthority)
        {
            LocalMapState = mapState;
            Debug.Log("[LocalPlayerLocator] Registered local player");
        }
    }
}
