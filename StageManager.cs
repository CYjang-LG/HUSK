using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

public class StageManager : MonoBehaviour
{
    public static StageManager instance;

    [Header("현재 스테이지")]
    public StageData currentStage;

    [Header("스테이지 UI")]
    public GameObject stageCompleteUI;
    public UnityEngine.UI.Text stageNameText;
    public UnityEngine.UI.Text stageDescText;
    public UnityEngine.UI.Button nextStageButton;
    public UnityEngine.UI.Button retryButton;

    private bool stageCompleted = false;

    void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void Start()
    {
        InitializeStage();
    }

    void InitializeStage()
    {
        if (currentStage == null)
        {
            Debug.LogWarning("StageManager: currentStage가 설정되지 않았습니다!");
            return;
        }

        // GameManager에 스테이지 조건 적용
        if (GameManager.instance != null)
        {
            GameManager.instance.gameConditions = currentStage.gameConditions;
            GameManager.instance.stageManager = this;

            // 스테이지의 시간 제한을 GameManager에 적용
            if (currentStage.gameConditions != null && currentStage.gameConditions.useTimeLimit)
            {
                GameManager.instance.maxGameTime = currentStage.gameConditions.timeLimit;
            }
        }

        // Spawner에 스폰 데이터 적용
        EnemySpawner spawner = FindFirstObjectByType<EnemySpawner>();
        if (spawner != null)
        {
            // StageData의 GetSpawnData() 메서드 사용
            SpawnData[] stageSpawnData = currentStage.GetSpawnData();
            if (stageSpawnData != null && stageSpawnData.Length > 0)
            {
                spawner.spawnData = stageSpawnData;
                Debug.Log($"Spawner에 {stageSpawnData.Length}개의 스폰 데이터 적용");
            }
        }
        else
        {
            Debug.LogWarning("StageManager: EnemySpawner를 찾을 수 없습니다!");
        }

        // 배경 변경
        if (currentStage.backgroundPrefab != null)
        {
            // 기존 배경 제거 (선택사항)
            GameObject existingBackground = GameObject.FindGameObjectWithTag("Background");
            if (existingBackground != null)
            {
                Destroy(existingBackground);
            }

            GameObject newBackground = Instantiate(currentStage.backgroundPrefab);
            newBackground.tag = "Background";
            Debug.Log($"배경 변경: {currentStage.backgroundPrefab.name}");
        }

        // BGM 변경
        if (currentStage.bgmClip != null && AudioManager.instance != null)
        {
            AudioManager.instance.bgmClip = currentStage.bgmClip;
            AudioManager.instance.PlayBgm(true);
            Debug.Log($"BGM 변경: {currentStage.bgmClip.name}");
        }

        // 특별 이벤트 설정
        if (currentStage.hasSpecialEvent)
        {
            StartCoroutine(SpecialEventCoroutine());
        }

        Debug.Log($"스테이지 초기화 완료: {currentStage.stageName}");
    }

    IEnumerator SpecialEventCoroutine()
    {
        yield return new WaitForSeconds(currentStage.specialEventTime);

        if (GameManager.instance != null && GameManager.instance.isLive)
        {
            TriggerSpecialEvent();
        }
    }

    void TriggerSpecialEvent()
    {
        Debug.Log("특별 이벤트 발생!");

        // 보스 스폰
        EnemySpawner spawner = FindFirstObjectByType<EnemySpawner>();
        if (spawner != null)
        {
            spawner.SpawnBoss();
        }

        // 추가 특별 이벤트들...
    }

    public void OnStageComplete()
    {
        if (stageCompleted) return;
        stageCompleted = true;

        StartCoroutine(StageCompleteSequence());
    }

    IEnumerator StageCompleteSequence()
    {
        // 적들 정리
        yield return new WaitForSeconds(1f);

        // 스테이지 클리어 UI 표시
        if (stageCompleteUI != null)
        {
            stageCompleteUI.SetActive(true);

            if (stageNameText != null)
                stageNameText.text = $"{currentStage.stageName} 클리어!";

            if (stageDescText != null)
                stageDescText.text = currentStage.stageDescription;

            // 버튼 설정
            SetupUIButtons();
        }
        else
        {
            Debug.LogWarning("StageManager: stageCompleteUI가 설정되지 않았습니다!");
            yield return new WaitForSeconds(3f);
            LoadNextStage();
        }

        Time.timeScale = 0; // 게임 일시정지
    }

    void SetupUIButtons()
    {
        if (nextStageButton != null)
        {
            bool hasNextStage = currentStage.nextStage != null || !string.IsNullOrEmpty(currentStage.nextSceneName);
            nextStageButton.gameObject.SetActive(hasNextStage);

            if (hasNextStage)
            {
                nextStageButton.onClick.RemoveAllListeners();
                nextStageButton.onClick.AddListener(LoadNextStage);

                // 버튼 텍스트 업데이트
                var buttonText = nextStageButton.GetComponentInChildren<UnityEngine.UI.Text>();
                if (buttonText != null)
                {
                    if (currentStage.nextStage != null)
                        buttonText.text = "다음 스테이지";
                    else
                        buttonText.text = "계속하기";
                }
            }
        }

        if (retryButton != null)
        {
            retryButton.onClick.RemoveAllListeners();
            retryButton.onClick.AddListener(RetryStage);
        }
    }

    public void LoadNextStage()
    {
        Time.timeScale = 1; // 게임 재개

        if (currentStage.nextStage != null)
        {
            // 다음 스테이지 데이터로 변경
            currentStage = currentStage.nextStage;

            // 현재 씬 재로드 (새로운 스테이지 데이터로)
            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
        }
        else if (!string.IsNullOrEmpty(currentStage.nextSceneName))
        {
            // 다른 씬으로 이동
            SceneManager.LoadScene(currentStage.nextSceneName);
        }
        else
        {
            // 게임 완료
            ShowGameComplete();
        }
    }

    public void RetryStage()
    {
        Time.timeScale = 1;
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    void ShowGameComplete()
    {
        Debug.Log("🎉 축하합니다! 모든 스테이지를 완주했습니다!");

        // 게임 완료 UI 표시 또는 크레딧
        if (stageCompleteUI != null)
        {
            if (stageNameText != null)
                stageNameText.text = "게임 완료!";
            if (stageDescText != null)
                stageDescText.text = "모든 스테이지를 클리어했습니다!";

            // 다음 스테이지 버튼 숨기기
            if (nextStageButton != null)
                nextStageButton.gameObject.SetActive(false);
        }
    }

    // GameManager에서 호출할 메서드
    public void OnGameVictory()
    {
        OnStageComplete();
    }

    // 현재 스테이지 정보 가져오기
    public string GetCurrentStageName()
    {
        return currentStage != null ? currentStage.stageName : "Unknown Stage";
    }

    public int GetCurrentStageNumber()
    {
        return currentStage != null ? currentStage.stageNumber : 0;
    }

    public int GetCurrentStageIndex()
    {
        return GetCurrentStageNumber() - 1;
    }

    // 스테이지 강제 변경 (디버그용)
    public void ForceChangeStage(StageData newStage)
    {
        if (newStage != null)
        {
            currentStage = newStage;
            InitializeStage();
            Debug.Log($"스테이지 강제 변경: {newStage.stageName}");
        }
    }
}