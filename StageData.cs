using UnityEngine;

[CreateAssetMenu(fileName = "StageData", menuName = "Game/Stage Data")]
public class StageData : ScriptableObject
{
    [Header("스테이지 정보")]
    public int stageNumber = 1;
    public string stageName = "Stage 1";
    [TextArea(2, 4)]
    public string stageDescription = "첫 번째 스테이지";

    [Header("게임 조건")]
    public GameConditions gameConditions;

    [Header("적 스폰 설정")]
    public EnemySpawnProfile enemySpawnProfile; // ScriptableObject 참조
    public SpawnData[] customSpawnData; // 또는 직접 배열 설정

    [Header("배경/음악")]
    public GameObject backgroundPrefab;
    public AudioClip bgmClip;

    [Header("다음 스테이지")]
    public StageData nextStage;
    public string nextSceneName; // Scene 이름 (선택사항)

    [Header("특별 이벤트")]
    public bool hasSpecialEvent = false;
    public float specialEventTime = 30f; // 특별 이벤트 발생 시간

    // 스폰 데이터 가져오기 (우선순위: customSpawnData → enemySpawnProfile)
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

        // 기본값 반환
        return new SpawnData[]
        {
            new SpawnData { spriteType = 0, health = 50, speed = 1f, spawnTime = 3f }
        };
    }
}
