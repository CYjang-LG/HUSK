using UnityEngine;

/// <summary>
/// 적 스폰을 관리하는 통합 스포너 시스템
/// GameManager의 gameTime/maxGameTime을 참조하여 스폰 타이밍 동기화
/// 보스 스테이지 지원 기능 포함
/// </summary>
public class EnemySpawner : MonoBehaviour
{
    [Header("=== 스폰 포인트 ===")]
    public Transform[] spawnPoints;

    [Header("=== 스폰 설정 ===")]
    public SpawnData[] spawnData;                // 일반 스테이지용 데이터
    public EnemySpawnProfile spawnProfile;       // ScriptableObject 프로필 (선택사항)

    [Header("=== 보스 스테이지 ===")]
    public EnemySpawnProfile[] bossSpawnProfiles; // 보스 스테이지별 프로필 (6개)
    public bool isBossStage = false;

    [Header("=== 스폰 제한 ===")]
    public int maxEnemiesOnScreen = 50;
    public float minSpawnDistance = 2f;          // 플레이어로부터 최소 거리

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
            HandleBossStage();
        else
            HandleNormalStage();
    }

    #region 초기화
    private void InitializeSpawner()
    {
        if (spawnPoints == null || spawnPoints.Length == 0)
            spawnPoints = GetComponentsInChildren<Transform>();

        if (spawnData == null || spawnData.Length == 0)
            CreateDefaultSpawnData();
    }

    private void SetupStageData()
    {
        if (StageManager.instance != null)
        {
            int currentStage = StageManager.instance.GetCurrentStageIndex();
            isBossStage = (currentStage + 1) % 5 == 0;

            if (isBossStage && bossSpawnProfiles != null)
            {
                int bossIndex = (currentStage + 1) / 5 - 1;
                if (bossIndex < bossSpawnProfiles.Length)
                    spawnProfile = bossSpawnProfiles[bossIndex];
            }
        }

        if (spawnProfile != null && spawnProfile.levelSpawnData != null)
            spawnData = spawnProfile.levelSpawnData;

        if (GameManager.instance != null)
            levelDuration = GameManager.instance.maxGameTime / spawnData.Length;
        else
            levelDuration = 30f;

        Debug.Log($"EnemySpawner 초기화: 보스스테이지={isBossStage}, 레벨지속시간={levelDuration}초");
    }

    private void CreateDefaultSpawnData()
    {
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

        currentLevel = Mathf.Min(
            Mathf.FloorToInt(GameManager.instance.gameTime / levelDuration),
            spawnData.Length - 1
        );

        SpawnData currentSpawnData = spawnData[currentLevel];
        if (timer >= currentSpawnData.spawnTime)
        {
            timer = 0f;
            int spawnCount = Mathf.Min(currentSpawnData.maxEnemiesPerWave,
                                       maxEnemiesOnScreen - currentEnemyCount);
            for (int i = 0; i < spawnCount; i++)
                SpawnEnemy(currentSpawnData);
        }
    }

    private void HandleBossStage()
    {
        if (!bossSpawned && GameManager.instance.gameTime > 1f)
        {
            SpawnBoss();
            bossSpawned = true;
        }
        // 보스 스폰 후에도 일반 적 소량 스폰 여부
        if (spawnProfile != null && spawnProfile.spawnMinionsInBossStage)
            HandleNormalStage();
    }
    #endregion

    #region 적 생성
    private void SpawnEnemy(SpawnData data)
    {
        Vector3 pos = GetValidSpawnPosition();
        if (pos == Vector3.zero) return;

        GameObject enemyGO = GameManager.instance.pool.Get(0);
        if (enemyGO == null) return;

        enemyGO.transform.position = pos;

        EnemyController enemy = enemyGO.GetComponent<EnemyController>();
        if (enemy != null)
        {
            SpawnData adj = new SpawnData
            {
                spriteType = data.spriteType,
                health = Mathf.RoundToInt(data.health * data.difficultyMultiplier),
                speed = data.speed * data.difficultyMultiplier,
                spawnTime = data.spawnTime,
                maxEnemiesPerWave = data.maxEnemiesPerWave
            };
            enemy.Initialize(adj);
        }
    }

    public void SpawnBoss()
    {
        if (spawnProfile == null || !spawnProfile.hasBoss)
        {
            Debug.LogWarning("보스 스폰 프로필이 설정되지 않았습니다.");
            return;
        }

        Vector3 pos = GetValidSpawnPosition();
        if (pos == Vector3.zero)
            pos = new Vector3(0, 2, 0);

        GameObject bossGO = GameManager.instance.pool.Get(1);
        if (bossGO == null) return;

        bossGO.transform.position = pos;

        EnemyController boss = bossGO.GetComponent<EnemyController>();
        if (boss != null)
        {
            boss.Initialize(spawnProfile.bossSpawnData);
            boss.SetAsBoss(true);
        }

        Debug.Log($"보스 스폰 완료");
        GameManager.instance.OnBossSpawned();
    }
    #endregion

    #region 유틸리티
    private Vector3 GetValidSpawnPosition()
    {
        if (spawnPoints.Length <= 1) return Vector3.zero;
        Vector3 playerPos = GameManager.instance.player.transform.position;

        for (int i = 1; i < spawnPoints.Length; i++)
        {
            Vector3 candidate = spawnPoints[Random.Range(1, spawnPoints.Length)].position;
            if (Vector3.Distance(candidate, playerPos) >= minSpawnDistance)
                return candidate;
        }
        return spawnPoints.Length > 1 ? spawnPoints[1].position : Vector3.zero;
    }

    public void ClearAllEnemies()
    {
        foreach (GameObject e in GameObject.FindGameObjectsWithTag("Enemy"))
            e.SetActive(false);
        currentEnemyCount = 0;
        Debug.Log("모든 적 제거 완료");
    }

    public void SetSpawnProfile(EnemySpawnProfile profile)
    {
        spawnProfile = profile;
        if (profile != null && profile.levelSpawnData != null)
            spawnData = profile.levelSpawnData;
    }

    public void SetBossStage(bool boss)
    {
        isBossStage = boss;
        bossSpawned = false;
    }

    public int GetCurrentEnemyCount() => currentEnemyCount;
    public bool IsBossStage() => isBossStage;
    #endregion

    #region 디버그
    void OnDrawGizmosSelected()
    {
        if (spawnPoints == null) return;

        Gizmos.color = Color.red;
        for (int i = 1; i < spawnPoints.Length; i++)
        {
            if (spawnPoints[i] != null)
            {
                Gizmos.DrawWireSphere(spawnPoints[i].position, 0.5f);
                Gizmos.color = Color.yellow;
                Gizmos.DrawWireSphere(spawnPoints[i].position, minSpawnDistance);
                Gizmos.color = Color.red;
            }
        }

        if (isBossStage)
        {
            Gizmos.color = Color.magenta;
            Gizmos.DrawWireCube(transform.position, Vector3.one * 2);
        }
    }
    #endregion
}
