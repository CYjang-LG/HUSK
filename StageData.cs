using UnityEngine;

[CreateAssetMenu(fileName = "StageData", menuName = "Game/Stage Data")]
public class StageData : ScriptableObject
{
    [Header("�������� ����")]
    public int stageNumber = 1;
    public string stageName = "Stage 1";
    [TextArea(2, 4)]
    public string stageDescription = "ù ��° ��������";

    [Header("���� ����")]
    public GameConditions gameConditions;

    [Header("�� ���� ����")]
    public EnemySpawnProfile enemySpawnProfile; // ScriptableObject ����
    public SpawnData[] customSpawnData; // �Ǵ� ���� �迭 ����

    [Header("���/����")]
    public GameObject backgroundPrefab;
    public AudioClip bgmClip;

    [Header("���� ��������")]
    public StageData nextStage;
    public string nextSceneName; // Scene �̸� (���û���)

    [Header("Ư�� �̺�Ʈ")]
    public bool hasSpecialEvent = false;
    public float specialEventTime = 30f; // Ư�� �̺�Ʈ �߻� �ð�

    // ���� ������ �������� (�켱����: customSpawnData �� enemySpawnProfile)
    public SpawnData[] GetSpawnData()
    {
        if (customSpawnData != null && customSpawnData.Length > 0)
        {
            return customSpawnData;
        }

        if (enemySpawnProfile != null && enemySpawnProfile.levelSpawnData != null)
        {
            return enemySpawnProfile.levelSpawnData;
        }

        // �⺻�� ��ȯ
        return new SpawnData[]
        {
            new SpawnData { spriteType = 0, health = 50, speed = 1f, spawnTime = 3f }
        };
    }
}