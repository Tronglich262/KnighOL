using UnityEngine;
using Fusion;

public class TestMapSwitch : NetworkBehaviour
{
    PlayerMapState map;

    void Start()
    {
        map = GetComponent<PlayerMapState>();
    }

    void Update()
    {
        if (!Object.HasInputAuthority) return;

        if (Input.GetKeyDown(KeyCode.Alpha1))
            TeleToMap(MapId.Town);

        if (Input.GetKeyDown(KeyCode.Alpha2))
            TeleToMap(MapId.Forest);

        if (Input.GetKeyDown(KeyCode.Alpha3))
            TeleToMap(MapId.Dungeon);
    }

    void TeleToMap(MapId targetMap)
    {
        // TIM PORTAL TRONG SCENE
        MapPortal[] portals = FindObjectsByType<MapPortal>(FindObjectsSortMode.None);
        foreach (var portal in portals)
        {
            if (portal.targetMap != targetMap)
                continue;

            if (portal.targetSpawn == null)
            {
                Debug.LogError($"Portal {portal.name} thiếu targetSpawn");
                return;
            }

            // TELE GIONG HE T CODE CU
            map.RPC_ChangeMap(
                portal.targetMap,
                portal.targetSpawn.transform.position,
                portal.targetSpawn.transform.rotation
            );
            return;
        }

        Debug.LogWarning($"Không tìm thấy portal cho map {targetMap}");
    }
}
