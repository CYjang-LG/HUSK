using UnityEngine;

[CreateAssetMenu(fileName = "EnemySpawnProfile", menuName = "Game/Enemy Spawn Profile")]
public class EnemySpawnProfile : ScriptableObject
{
    [Header("스폰 프로필 정보")]
    public string profileName = "기본 스폰";
    [TextArea(2, 4)]
    public string description = "기본적인 적 스폰 설정";

    [Header("레벨별 스폰 데이터")]
    public SpawnData[] levelSpawnData = new SpawnData[]
    {
        new SpawnData { spriteType = 0, health = 50, speed = 1f, spawnTime = 3f },
        new SpawnData { spriteType = 0, health = 75, speed = 1.2f, spawnTime = 2.5f },
        new SpawnData { spriteType = 1, health = 100, speed = 1.5f, spawnTime = 2f },
        new SpawnData { spriteType = 1, health = 150, speed = 1.8f, spawnTime = 1.5f },
        new SpawnData { spriteType = 2, health = 200, speed = 2f, spawnTime = 1f }
    };

    [Header("보스 스폰 설정")]
    public bool hasBoss = false;
    public SpawnData bossSpawnData = new SpawnData { spriteType = 3, health = 500, speed = 0.8f, spawnTime = 60f };
}
