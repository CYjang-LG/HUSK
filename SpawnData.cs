using UnityEngine;
using System.Collections;

/// <summary>
/// ���� ������ ����
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
