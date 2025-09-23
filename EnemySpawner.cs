using UnityEngine;

/// <summary>
/// 적 스폰을 관리하는 통합 스포너 시스템
/// 기존 Spawner.cs + 보스 스테이지 지원 기능 추가
/// </summary>
public class EnemySpawner : MonoBehaviour
{
    [Header("=== 스폰 포인트 ===")]
    public Transform[] spawnPoints;

    [Header("=== 스폰 설정 ===")]
    public SpawnData[] spawnData; // 기본 배열 (일반 스테이지용)
    public EnemySpawnProfile spawnProfile; // ScriptableObject 프로필 (선택사항)
    
    [Header("=== 보스 스테이지 ===")]
    public EnemySpawnProfile[] bossSpawnProfiles; // 보스 스테이지 프로필 (6개)
    public bool isBossStage = false;

    [Header("=== 스폰 제한 ===")]
    public int maxEnemiesOnScreen = 50;
    public float minSpawnDistance = 2f; // 플레이어로부터 최소 거리

    [Header("=== 스테이지 설정 ===")]
    public float stageTimeLimit = 180f; // 3분 = 180초
    
    // 내부 변수
    private float levelDuration;
    private int currentLevel;
    private float timer;
    private int currentEnemyCount;
    private bool bossSpawned = false;

    void Awake()
    {
        InitializeSpawner();
    }

    void Start()
    {
        SetupStageData();
    }

    void Update()
    {
        if (!GameManager.instance.isLive) return;

        UpdateEnemyCount();
        
        if (isBossStage)
        {
            HandleBossStage();
        }
        else
        {
            HandleNormalStage();
        }
    }

    #region 초기화
    private void InitializeSpawner()
    {
        // 스폰 포인트 자동 검색
        if (spawnPoints == null || spawnPoints.Length == 0)
        {
            spawnPoints = GetComponentsInChildren<Transform>();
        }

        // 기본 스폰 데이터 생성
        if (spawnData == null || spawnData.Length == 0)
        {
            CreateDefaultSpawnData();
        }
    }

    private void SetupStageData()
    {
        // StageManager에서 현재 스테이지 정보 가져오기
        if (StageManager.instance != null)
        {
            int currentStage = StageManager.instance.GetCurrentStageIndex();
            isBossStage = (currentStage + 1) % 5 == 0; // 5, 10, 15, 20, 25, 30단계
            
            if (isBossStage && bossSpawnProfiles != null)
            {
                int bossIndex = (currentStage + 1) / 5 - 1; // 0~5 인덱스
                if (bossIndex < bossSpawnProfiles.Length)
                {
                    spawnProfile = bossSpawnProfiles[bossIndex];
                }
            }
        }

        // SpawnProfile이 설정되어 있으면 우선 사용
        if (spawnProfile != null && spawnProfile.levelSpawnData != null)
        {
            spawnData = spawnProfile.levelSpawnData;
        }

        // 레벨 지속시간 계산
        if (GameManager.instance != null)
        {
            levelDuration = stageTimeLimit / spawnData.Length;
        }
        else
        {
            levelDuration = 30f; // 기본값
        }

        Debug.Log($"EnemySpawner 초기화: 보스스테이지={isBossStage}, 레벨지속시간={levelDuration}초");
    }

    private void CreateDefaultSpawnData()
    {
        // 기본 스폰 데이터 생성
        spawnData = new SpawnData[]
        {
            new SpawnData { spriteType = 0, health = 50, speed = 1f, spawnTime = 3f, maxEnemiesPerWave = 2 },
            new SpawnData { spriteType = 0, health = 75, speed = 1.2f, spawnTime = 2.5f, maxEnemiesPerWave = 3 },
            new SpawnData { spriteType = 0, health = 100, speed = 1.5f, spawnTime = 2f, maxEnemiesPerWave = 4 }
        };

        Debug.LogWarning("EnemySpawner: 기본 스폰 데이터를 생성했습니다. EnemySpawnProfile을 설정하는 것을 권장합니다.");
    }
    #endregion

    #region 스폰 시스템
    private void UpdateEnemyCount()
    {
        currentEnemyCount = GameObject.FindGameObjectsWithTag("Enemy").Length;
    }

    private void HandleNormalStage()
    {
        if (spawnData == null || spawnData.Length == 0) return;
        if (currentEnemyCount >= maxEnemiesOnScreen) return;

        timer += Time.deltaTime;

        // 현재 레벨 계산 (시간 기반)
        currentLevel = Mathf.Min(
            Mathf.FloorToInt(GameManager.instance.gameTime / levelDuration),
            spawnData.Length - 1
        );

        // 스폰 타이밍 체크
        SpawnData currentSpawnData = spawnData[currentLevel];
        if (timer >= currentSpawnData.spawnTime)
        {
            timer = 0f;

            // 여러 마리 스폰
            int spawnCount = Mathf.Min(currentSpawnData.maxEnemiesPerWave,
                maxEnemiesOnScreen - currentEnemyCount);

            for (int i = 0; i < spawnCount; i++)
            {
                SpawnEnemy(currentSpawnData);
            }
        }
    }

    private void HandleBossStage()
    {
        // 보스가 아직 스폰되지 않았다면 스폰
        if (!bossSpawned && GameManager.instance.gameTime > 1f) // 1초 후 보스 스폰
        {
            SpawnBoss();
            bossSpawned = true;
        }

        // 보스 스테이지에서도 일반 적들을 소량 스폰 (선택적)
        if (spawnProfile != null && spawnProfile.spawnMinionsInBossStage)
        {
            HandleNormalStage(); // 일반 스폰 로직 사용
        }
    }
    #endregion

    #region 적 생성
    private void SpawnEnemy(SpawnData data)
    {
        Vector3 spawnPosition = GetValidSpawnPosition();
        if (spawnPosition == Vector3.zero) return;

        // 오브젝트 풀에서 적 가져오기
        GameObject enemyGO = GameManager.instance.pool.Get(0); // 0: Enemy prefab index
        if (enemyGO == null) return;

        enemyGO.transform.position = spawnPosition;

        // EnemyController 컴포넌트 초기화
        EnemyController enemy = enemyGO.GetComponent<EnemyController>();
        if (enemy != null)
        {
            // 난이도에 따른 스탯 조정
            SpawnData adjustedData = new SpawnData
            {
                spriteType = data.spriteType,
                health = Mathf.RoundToInt(data.health * data.difficultyMultiplier),
                speed = data.speed * data.difficultyMultiplier,
                spawnTime = data.spawnTime,
                maxEnemiesPerWave = data.maxEnemiesPerWave
            };

            enemy.Initialize(adjustedData);
        }
    }

    public void SpawnBoss()
    {
        if (spawnProfile == null || !spawnProfile.hasBoss)
        {
            Debug.LogWarning("보스 스폰 프로필이 설정되지 않았습니다.");
            return;
        }

        Vector3 bossSpawnPos = GetValidSpawnPosition();
        if (bossSpawnPos == Vector3.zero)
        {
            // 보스는 중앙에 강제 스폰
            bossSpawnPos = new Vector3(0, 2, 0);
        }

        // 보스 전용 프리팹 사용 (PoolManager에서 인덱스 1로 가정)
        GameObject bossGO = GameManager.instance.pool.Get(1); // 1: Boss prefab index
        if (bossGO == null) return;

        bossGO.transform.position = bossSpawnPos;

        // 보스 초기화
        EnemyController bossEnemy = bossGO.GetComponent<EnemyController>();
        if (bossEnemy != null)
        {
            bossEnemy.Initialize(spawnProfile.bossSpawnData);
            bossEnemy.SetAsBoss(true); // 보스 플래그 설정
        }

        Debug.Log($"보스 스폰 완료: {spawnProfile.bossSpawnData.spriteType}번 보스");

        // GameManager에 보스 스폰 알림
        if (GameManager.instance != null)
        {
            GameManager.instance.OnBossSpawned();
        }
    }
    #endregion

    #region 유틸리티
    private Vector3 GetValidSpawnPosition()
    {
        if (spawnPoints.Length <= 1) return Vector3.zero;

        Vector3 playerPos = GameManager.instance.player.transform.position;

        // 최대 10번 시도
        for (int attempts = 0; attempts < 10; attempts++)
        {
            // 랜덤 스폰 포인트 선택 (0은 자신이므로 1부터)
            int randomIndex = Random.Range(1, spawnPoints.Length);
            Vector3 candidatePos = spawnPoints[randomIndex].position;

            // 플레이어와의 거리 체크
            float distance = Vector3.Distance(candidatePos, playerPos);
            if (distance >= minSpawnDistance)
            {
                return candidatePos;
            }
        }

        // 적절한 위치를 찾지 못한 경우 첫 번째 스폰 포인트 사용
        return spawnPoints.Length > 1 ? spawnPoints[1].position : Vector3.zero;
    }

    public void ClearAllEnemies()
    {
        GameObject[] enemies = GameObject.FindGameObjectsWithTag("Enemy");
        foreach (GameObject enemy in enemies)
        {
            enemy.SetActive(false);
        }
        
        currentEnemyCount = 0;
        Debug.Log("모든 적 제거 완료");
    }

    public void SetSpawnProfile(EnemySpawnProfile profile)
    {
        spawnProfile = profile;
        if (profile != null && profile.levelSpawnData != null)
        {
            spawnData = profile.levelSpawnData;
        }
    }

    public void SetBossStage(bool isBoss)
    {
        isBossStage = isBoss;
        bossSpawned = false; // 보스 스폰 상태 리셋
    }

    public int GetCurrentEnemyCount()
    {
        return currentEnemyCount;
    }

    public bool IsBossStage()
    {
        return isBossStage;
    }
    #endregion

    #region 디버그
    void OnDrawGizmosSelected()
    {
        if (spawnPoints == null) return;

        Gizmos.color = Color.red;
        for (int i = 1; i < spawnPoints.Length; i++) // 0은 자신이므로 스킵
        {
            if (spawnPoints[i] != null)
            {
                Gizmos.DrawWireSphere(spawnPoints[i].position, 0.5f);

                // 플레이어와의 최소 거리 표시
                if (Application.isPlaying && GameManager.instance != null)
                {
                    Gizmos.color = Color.yellow;
                    Gizmos.DrawWireSphere(spawnPoints[i].position, minSpawnDistance);
                    Gizmos.color = Color.red;
                }
            }
        }

        // 보스 스테이지 표시
        if (isBossStage)
        {
            Gizmos.color = Color.purple;
            Gizmos.DrawWireCube(transform.position, Vector3.one * 2);
        }
    }
    #endregion
}
