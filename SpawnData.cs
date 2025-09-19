using UnityEngine;

[System.Serializable]
public class SpawnData
{
    [Header("�� ����")]
    public int spriteType = 0;
    public int health = 100;
    public float speed = 2f;

    [Header("���� Ÿ�̹�")]
    public float spawnTime = 2f; // ���� ���� (��)

    [Header("�߰� ����")]
    public int maxEnemiesPerWave = 5; // �� ���� ������ �ִ� �� ��
    public float difficultyMultiplier = 1f; // ���̵� ���
}