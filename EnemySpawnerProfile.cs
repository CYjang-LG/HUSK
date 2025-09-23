using UnityEngine;

[CreateAssetMenu(fileName = "EnemySpawnProfile", menuName = "Game/EnemySpawnProfile")]
public class EnemySpawnProfile : ScriptableObject
{
    [Header("Profile Info")]
    public string profileName;                // Ãß°¡µÈ ÇÊµå

    [Header("Spawn Data")]
    public SpawnData[] levelSpawnData;
    public SpawnData bossSpawnData;

    [Header("Boss Settings")]
    public bool hasBoss = true;
    public bool spawnMinionsInBossStage = false;
}
