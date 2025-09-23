using UnityEngine;
using System.Collections;

/// <summary>
/// 스폰 데이터 정의
/// </summary>
[System.Serializable]
public class SpawnData
{
    public int spriteType;
    public int health;
    public float speed;
    public float spawnTime;
    public int maxEnemiesPerWave;
    public float difficultyMultiplier = 1f;
}
