using UnityEngine;

[System.Serializable]
public class SpawnData
{
    [Header("적 외형")]
    public int spriteType = 0;
    public int health = 100;
    public float speed = 2f;

    [Header("스폰 타이밍")]
    public float spawnTime = 2f; // 스폰 간격 (초)

    [Header("추가 설정")]
    public int maxEnemiesPerWave = 5; // 한 번에 스폰할 최대 적 수
    public float difficultyMultiplier = 1f; // 난이도 배수
}
