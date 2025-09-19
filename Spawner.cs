using UnityEngine;

public class Spawner : MonoBehaviour
{
    [Header("스폰 포인트")]
    public Transform[] spawnPoints;

    [Header("스폰 설정")]
    public SpawnData[] spawnData; // 기본 배열
    public EnemySpawnProfile spawnProfile; // ScriptableObject 프로필 (선택사항)

    [Header("스폰 제한")]
    public int maxEnemiesOnScreen = 50;
    public float minSpawnDistance = 2f; // 플레이어로부터 최소 거리

    private float levelDuration;
    private int currentLevel;
    private float timer;
    private int currentEnemyCount;

    void Awake()
    {
        spawnPoints = GetComponentsInChildren<Transform>();

        // SpawnProfile이 설정되어 있으면 우선 사용
        if (spawnProfile != null && spawnProfile.levelSpawnData != null)
        {
            spawnData = spawnProfile.levelSpawnData;
        }

        // 기본값 설정
        if (spawnData == null || spawnData.Length == 0)
        {
            CreateDefaultSpawnData();
        }

        // 레벨 지속시간 계산
        if (GameManager.instance != null)
        {
            levelDuration = GameManager.instance.maxGameTime / spawnData.Length;
        }
        else
        {
            levelDuration = 10f; // 기본값
        }
    }

    void CreateDefaultSpawnData()
    {
        // 기본 스폰 데이터 생성
        spawnData = new SpawnData[]
        {
            new SpawnData { spriteType = 0, health = 50, speed = 1f, spawnTime = 3f },
            new SpawnData { spriteType = 0, health = 75, speed = 1.2f, spawnTime = 2.5f },
            new SpawnData { spriteType = 0, health = 100, speed = 1.5f, spawnTime = 2f }
        };

        Debug.LogWarning("Spawner: 기본 스폰 데이터를 생성했습니다. EnemySpawnProfile을 설정하는 것을 권장합니다.");
    }

    void Update()
    {
        if (!GameManager.instance.isLive || spawnData == null || spawnData.Length == 0)
            return;

        // 현재 적 수 체크
        currentEnemyCount = GameObject.FindGameObjectsWithTag("Enemy").Length;

        if (currentEnemyCount >= maxEnemiesOnScreen)
            return;

        timer += Time.deltaTime;

        // 현재 레벨 계산
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

    private void SpawnEnemy(SpawnData data)
    {
        // 적절한 스폰 위치 찾기
        Vector3 spawnPosition = GetValidSpawnPosition();
        if (spawnPosition == Vector3.zero) return; // 적절한 위치를 찾지 못함

        // 오브젝트 풀에서 적 가져오기
        GameObject enemyGO = GameManager.instance.pool.Get(0);
        if (enemyGO == null) return;

        enemyGO.transform.position = spawnPosition;

        // Enemy 컴포넌트 초기화
        Enemy enemy = enemyGO.GetComponent<Enemy>();
        if (enemy != null)
        {
            // 난이도에 따른 스탯 조정
            SpawnData adjustedData = new SpawnData
            {
                spriteType = data.spriteType,
                health = Mathf.RoundToInt(data.health * data.difficultyMultiplier),
                speed = data.speed * data.difficultyMultiplier,
                spawnTime = data.spawnTime
            };

            enemy.Init(adjustedData);
        }
    }

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

    // 보스 스폰 (특별한 경우)
    public void SpawnBoss()
    {
        if (spawnProfile != null && spawnProfile.hasBoss)
        {
            SpawnEnemy(spawnProfile.bossSpawnData);
            Debug.Log("보스 스폰!");
        }
    }

    // 디버그용 기즈모
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
    }
}