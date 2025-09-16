using UnityEngine;

public class Spawner : MonoBehaviour
{
    [Header("Spawn Points")]
    public Transform[] spawnPoints;

    [Header("Spawn Data (per level)")]
    public SpawnData[] spawnData;

    private float levelDuration;
    private int currentLevel;
    private float timer;

    void Awake()
    {
        // 자식 Transform 중 첫 번째(자신)는 제외
        spawnPoints = GetComponentsInChildren<Transform>();
        levelDuration = GameManager.instance.maxGameTime / spawnData.Length;
    }

    void Update()
    {
        if (!GameManager.instance.isLive)
            return;

        // 시간 경과에 따라 레벨 결정
        timer += Time.deltaTime;
        currentLevel = Mathf.Min(
            Mathf.FloorToInt(GameManager.instance.gameTime / levelDuration),
            spawnData.Length - 1
        );

        // 스폰 타이밍 체크
        if (timer > spawnData[currentLevel].spawnTime)
        {
            timer = 0f;
            Spawn();
        }
    }

    private void Spawn()
    {
        // 오브젝트 풀에서 적 가져오기
        GameObject enemyGO = GameManager.instance.pool.Get(0);

        // 랜덤 스폰 지점 선택 (0은 자신이므로 1부터)
        int index = Random.Range(1, spawnPoints.Length);
        enemyGO.transform.position = spawnPoints[index].position;

        // Enemy 컴포넌트 초기화
        Enemy enemy = enemyGO.GetComponent<Enemy>();
        enemy.Init(spawnData[currentLevel]);
    }
}

[System.Serializable]
public class SpawnData
{
    [Header("Enemy Appearance")]
    public int spriteType;
    public int health;
    public float speed;

    [Header("Spawn Timing")]
    public float spawnTime;
}
