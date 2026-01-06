using UnityEngine;

[System.Serializable]
public class EnemySpawnPoint
{
    public Transform spawnTransform;
    public int monsterId;
    public float respawnTime = 5f;

    [HideInInspector] public bool IsOccupied;
}
