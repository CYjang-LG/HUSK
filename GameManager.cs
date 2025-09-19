using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public static GameManager instance;

    [Header("# Game Control")]
    public bool isLive;
    public float gameTime;
    public float maxGameTime = 20f; // 🔥 maxGameTime 필드 추가
    public GameConditions gameConditions; // 게임 조건 설정

    [Header("# Player Info")]
    public int playerId;
    public float health;
    public float maxHealth = 100;
    public int level;
    public int kill;
    public int exp;
    public int[] nextExp = { 10, 30, 60, 100, 150, 210, 280, 360, 450, 600 };

    [Header("# Game Object")]
    public PlayerSetup player;
    public PoolManager pool;
    public LevelUp uiLevelUp;
    public Result uiResult;
    public GameObject enemyCleaner;

    [Header("# Stage System")]
    public StageManager stageManager; // 스테이지 매니저 참조

    private Health playerHealth;
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
            playerHealth = player.GetComponent<Health>();
            if (playerHealth != null)
            {
                maxHealth = playerHealth.maxHealth;
                health = maxHealth;
            }
        }

        player.gameObject.SetActive(true);

        // 안전한 배열 접근
        if (playerId >= 0 && playerId < 2)
            uiLevelUp.Select(playerId);
        else
            uiLevelUp.Select(0);

        Resume();
        AudioManager.instance.PlayBgm(true);
        AudioManager.instance.PlaySfx(AudioManager.Sfx.Select);
    }

    void Update()
    {
        if (!isLive) return;

        gameTime += Time.deltaTime;

        // Health 동기화
        if (playerHealth != null)
            health = playerHealth.currentHealth;

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

    public void OnBossKilled()
    {
        bossKilled = true;
    }

    public void GameOver()
    {
        StartCoroutine(GameOverRoutine());
    }

    IEnumerator GameOverRoutine()
    {
        isLive = false;
        yield return new WaitForSeconds(0.5f);

        uiResult.gameObject.SetActive(true);
        uiResult.Lose();
        Stop();

        AudioManager.instance.PlayBgm(false);
        AudioManager.instance.PlaySfx(AudioManager.Sfx.Lose);
    }

    public void GameVictory()
    {
        StartCoroutine(GameVictoryRoutine());
    }

    IEnumerator GameVictoryRoutine()
    {
        isLive = false;
        enemyCleaner.SetActive(true);

        yield return new WaitForSeconds(0.5f);

        // 🔥 StageManager가 있으면 스테이지 클리어 처리, 없으면 기본 승리 UI
        if (stageManager != null)
        {
            stageManager.OnGameVictory();
        }
        else
        {
            // 기본 승리 처리
            uiResult.gameObject.SetActive(true);
            uiResult.Win();
            Stop();
        }

        AudioManager.instance.PlayBgm(false);
        AudioManager.instance.PlaySfx(AudioManager.Sfx.Win);
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
            uiLevelUp.Show();
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
