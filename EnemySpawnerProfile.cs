using UnityEngine;

[CreateAssetMenu(fileName = "EnemySpawnProfile", menuName = "Game/EnemySpawnProfile")]
public class EnemySpawnProfile : ScriptableObject
{
    [Header("Profile Info")]
    public string profileName;                // 추가된 필드

    [Header("Spawn Data")]
    public SpawnData[] levelSpawnData;
    public SpawnData bossSpawnData;

    [Header("Boss Settings")]
    public bool hasBoss = true;
    public bool spawnMinionsInBossStage = false;
}
