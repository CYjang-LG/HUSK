using UnityEngine;

/// <summary>
/// 적 스폰을 관리하는 통합 스포너 시스템
/// GameManager의 gameTime/maxGameTime을 참조하여 스폰 타이밍 동기화
/// </summary>
public class EnemySpawner : MonoBehaviour
{
    [Header("=== 스폰 포인트 ===")]
    public Transform[] spawnPoints;

    [Header("=== 스폰 설정 ===")]
    public SpawnData[] spawnData;

    [Header("=== 스폰 제한 ===")]
    public int maxEnemiesOnScreen = 50;
    public float minSpawnDistance = 2f;

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
        if (GameManager.instance != null)
            levelDuration = GameManager.instance.maxGameTime / spawnData.Length;
        else
            levelDuration = 30f;

        Debug.Log($"EnemySpawner 초기화: 레벨지속시간={levelDuration}초");
    }

    private void CreateDefaultSpawnData()
    {
        spawnData = new SpawnData[]
        {
            new SpawnData { spriteType = 0, health = 50, speed = 1f, spawnTime = 3f, maxEnemiesPerWave = 2 },
            new SpawnData { spriteType = 0, health = 75, speed = 1.2f, spawnTime = 2.5f, maxEnemiesPerWave = 3 },
            new SpawnData { spriteType = 0, health = 100, speed = 1.5f, spawnTime = 2f, maxEnemiesPerWave = 4 }
        };

        Debug.LogWarning("EnemySpawner: 기본 스폰 데이터를 생성했습니다.");
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
            enemy.Initialize(data);
        }
    }

    public void SpawnBoss()
    {
        if (bossSpawned) return;

        Vector3 pos = GetValidSpawnPosition();
        if (pos == Vector3.zero)
            pos = new Vector3(0, 2, 0);

        GameObject bossGO = GameManager.instance.pool.Get(0); // 같은 프리팹 사용
        if (bossGO == null) return;

        bossGO.transform.position = pos;

        EnemyController boss = bossGO.GetComponent<EnemyController>();
        if (boss != null)
        {
            SpawnData bossData = new SpawnData
            {
                spriteType = 0,
                health = 300,
                speed = 0.8f,
                spawnTime = 1f,
                maxEnemiesPerWave = 1
            };
            boss.Initialize(bossData);
            boss.SetAsBoss(true);
        }

        bossSpawned = true;
        Debug.Log("보스 스폰 완료");
    }
    #endregion

    #region 유틸리티
    private Vector3 GetValidSpawnPosition()
    {
        if (spawnPoints.Length <= 1) return Vector3.zero;
        if (GameManager.instance?.player == null) return Vector3.zero;

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
        bossSpawned = false;
        Debug.Log("모든 적 제거 완료");
    }

    public int GetCurrentEnemyCount() => currentEnemyCount;
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
    }
    #endregion
}

// SpawnData 클래스 정의
[System.Serializable]
public class SpawnData
{
    public int spriteType;
    public float health = 50f;
    public float speed = 1f;
    public float spawnTime = 3f;
    public int maxEnemiesPerWave = 2;
    public float difficultyMultiplier = 1f;
}