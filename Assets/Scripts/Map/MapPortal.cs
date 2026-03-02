using Fusion;
using UnityEngine;

public class MapPortal : MonoBehaviour
{
    public MapId targetMap;
    public MapSpawnPoint targetSpawn;

    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player")) return;

        var netObj = other.GetComponent<NetworkObject>();
        if (netObj == null || !netObj.HasInputAuthority) return;

        var mapState = other.GetComponent<PlayerMapState>();
        if (mapState == null) return;

        // TRUYEN VI TRI + ROTATION SPAWN CU THE
        mapState.RPC_ChangeMap(
            targetMap,
            targetSpawn.transform.position,
            targetSpawn.transform.rotation
        );
    }
}
