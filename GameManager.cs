using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public static GameManager instance;

    [Header("# Game Control")]
    public bool isLive;
    public float gameTime;
    public float maxGameTime = 20f;
    public GameConditions gameConditions;

    [Header("# Player Info")]
    public int playerId;
    public float health;
    public float maxHealth = 100;
    public int level;
    public int kill;
    public int exp;
    public int[] nextExp = { 10, 30, 60, 100, 150, 210, 280, 360, 450, 600 };

    [Header("# Game Object")]
    public GameObject player;
    public PoolManager pool;
    public GameObject uiLevelUp;
    public GameObject uiResult;
    public GameObject enemyCleaner;

    [Header("# Stage System")]
    public StageManager stageManager;

    private PlayerController playerController;
    private bool bossKilled = false;

    void Awake()
    {
        instance = this;
    }

    void Start()
    {
        GameStart(0);
    }

    public void GameStart(int id)
    {
        playerId = id;

        // 게임 조건이 설정되어 있으면 시간 제한 적용
        if (gameConditions != null && gameConditions.useTimeLimit)
        {
            maxGameTime = gameConditions.timeLimit;
        }

        if (player != null)
        {
            playerController = player.GetComponent<PlayerController>();
            if (playerController != null)
            {
                maxHealth = playerController.maxHealth;
                health = maxHealth;
            }
        }

        if (player != null)
            player.SetActive(true);

        // UI 레벨업 설정 (UIManager 컴포넌트가 있으면)
        var uiManager = FindFirstObjectByType<UIManager>();
        if (uiManager != null && playerId >= 0 && playerId < 2)
        {
            // UIManager에서 처리
        }

        Resume();
        if (AudioManager.instance != null)
        {
            AudioManager.instance.PlayBgm(true);
            AudioManager.instance.PlaySfx(AudioManager.Sfx.Select);
        }
    }

    void Update()
    {
        if (!isLive) return;

        gameTime += Time.deltaTime;

        // Health 동기화
        if (playerController != null)
            health = playerController.currentHealth;

        // 승리 조건 체크 (기본 시간 제한)
        if (gameTime >= maxGameTime)
        {
            gameTime = maxGameTime;
            GameVictory();
            return;
        }

        // 추가 게임 조건 체크
        CheckWinConditions();
        CheckLoseConditions();
    }

    void CheckWinConditions()
    {
        if (gameConditions == null) return;

        bool shouldWin = false;

        // 킬 수 승리
        if (gameConditions.useKillTarget && kill >= gameConditions.killTarget)
        {
            shouldWin = true;
        }

        // 보스 처치 승리
        if (gameConditions.useBossKill && bossKilled)
        {
            shouldWin = true;
        }

        if (shouldWin)
        {
            GameVictory();
        }
    }

    void CheckLoseConditions()
    {
        if (gameConditions == null) return;

        // 체력 0 패배
        if (gameConditions.useHealthZero && health <= 0)
        {
            GameOver();
        }

        // 최대 적 수 초과 패배
        if (gameConditions.useMaxEnemies)
        {
            int enemyCount = GameObject.FindGameObjectsWithTag("Enemy").Length;
            if (enemyCount >= gameConditions.maxEnemiesOnScreen)
            {
                GameOver();
            }
        }
    }

    public void OnBossSpawned()
    {
        Debug.Log("보스가 스폰되었습니다!");
    }

    public void OnBossKilled()
    {
        bossKilled = true;
        Debug.Log("보스가 처치되었습니다!");
    }

    public void GameOver()
    {
        StartCoroutine(GameOverRoutine());
    }

    IEnumerator GameOverRoutine()
    {
        isLive = false;
        yield return new WaitForSeconds(0.5f);

        // UIManager를 통한 결과 표시
        var uiManager = FindFirstObjectByType<UIManager>();
        if (uiManager != null)
        {
            uiManager.ShowGameResult(false);
        }
        else if (uiResult != null)
        {
            uiResult.SetActive(true);
        }

        Stop();

        if (AudioManager.instance != null)
        {
            AudioManager.instance.PlayBgm(false);
            AudioManager.instance.PlaySfx(AudioManager.Sfx.Lose);
        }
    }

    public void GameVictory()
    {
        StartCoroutine(GameVictoryRoutine());
    }

    IEnumerator GameVictoryRoutine()
    {
        isLive = false;
        if (enemyCleaner != null)
            enemyCleaner.SetActive(true);

        yield return new WaitForSeconds(0.5f);

        // StageManager가 있으면 스테이지 클리어 처리
        if (stageManager != null)
        {
            stageManager.OnGameVictory();
        }
        else
        {
            // 기본 승리 처리
            var uiManager = FindFirstObjectByType<UIManager>();
            if (uiManager != null)
            {
                uiManager.ShowGameResult(true);
            }
            else if (uiResult != null)
            {
                uiResult.SetActive(true);
            }
            Stop();
        }

        if (AudioManager.instance != null)
        {
            AudioManager.instance.PlayBgm(false);
            AudioManager.instance.PlaySfx(AudioManager.Sfx.Win);
        }
    }

    public void GameRetry()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    public void GetExp()
    {
        if (!isLive) return;

        exp++;
        int levelIndex = Mathf.Min(level, nextExp.Length - 1);
        if (exp >= nextExp[levelIndex])
        {
            level++;
            exp = 0;

            // UIManager를 통한 레벨업 표시
            var uiManager = FindFirstObjectByType<UIManager>();
            if (uiManager != null)
            {
                uiManager.ShowLevelUp();
            }
            else if (uiLevelUp != null)
            {
                uiLevelUp.SetActive(true);
            }
        }
    }

    public void Stop()
    {
        isLive = false;
        Time.timeScale = 0;
    }

    public void Resume()
    {
        isLive = true;
        Time.timeScale = 1;
    }
}