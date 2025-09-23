using UnityEngine;
using UnityEngine.UI;
using System.Collections;

/// <summary>
/// HUD, LevelUp, Result UI를 통합 관리하는 UI 매니저
/// HUD.cs + LevelUp.cs + Result.cs 기능 통합
/// </summary>
public class UIManager : MonoBehaviour
{
    [Header("=== HUD 요소 ===")]
    public Slider expSlider;
    public Slider healthSlider;
    public Text levelText;
    public Text killText;
    public Text timeText;
    
    [Header("=== 레벨업 UI ===")]
    public GameObject levelUpPanel;
    public RectTransform uiGroup;
    public Item[] items;
    
    [Header("=== 결과 UI ===")]
    public GameObject resultPanel;
    public GameObject[] resultTitles; // : Lose, : Win
    public GameObject stageCompletePanel;
    
    [Header("=== 기타 설정 ===")]
    public AudioClip levelUpSound;
    public AudioClip buttonClickSound;
    
    private bool isLevelUpActive = false;
    
    void Start()
    {
        InitializeUI();
    }
    
    void Update()
    {
        if (GameManager.instance == null) return;
        
        UpdateHUD();
    }
    
    #region 초기화
    private void InitializeUI()
    {
        // 레벨업 패널 초기 비활성화
        if (levelUpPanel != null)
            levelUpPanel.SetActive(false);
            
        // 결과 패널 초기 비활성화
        if (resultPanel != null)
            resultPanel.SetActive(false);
    }
    #endregion
    
    #region HUD 업데이트
    private void UpdateHUD()
    {
        UpdateExpBar();
        UpdateHealthBar();
        UpdateLevelText();
        UpdateKillText();
        UpdateTimeText();
    }
    
    private void UpdateExpBar()
    {
        if (expSlider == null) return;
        
        float currentExp = GameManager.instance.exp;
        int levelIndex = Mathf.Min(GameManager.instance.level, GameManager.instance.nextExp.Length - 1);
        float maxExp = GameManager.instance.nextExp[levelIndex];
        
        expSlider.value = currentExp / maxExp;
    }
    
    private void UpdateHealthBar()
    {
        if (healthSlider == null) return;
        
        float currentHealth = GameManager.instance.health;
        float maxHealth = GameManager.instance.maxHealth;
        
        healthSlider.value = maxHealth > 0 ? currentHealth / maxHealth : 0;
    }
    
    private void UpdateLevelText()
    {
        if (levelText == null) return;
        
        levelText.text = $"Lv.{GameManager.instance.level}";
    }
    
    private void UpdateKillText()
    {
        if (killText == null) return;
        
        killText.text = $"{GameManager.instance.kill}";
    }
    
    private void UpdateTimeText()
    {
        if (timeText == null) return;
        
        float remainingTime = GameManager.instance.maxGameTime - GameManager.instance.gameTime;
        remainingTime = Mathf.Max(0, remainingTime);
        
        int minutes = Mathf.FloorToInt(remainingTime / 60);
        int seconds = Mathf.FloorToInt(remainingTime % 60);
        
        timeText.text = $"{minutes:D2}:{seconds:D2}";
    }
    #endregion
    
    #region 레벨업 시스템
    public void ShowLevelUp()
    {
        if (isLevelUpActive) return;
        
        isLevelUpActive = true;
        levelUpPanel.SetActive(true);
        GameManager.instance.Stop();
        
        // 레벨업 아이템 선택지 표시
        SetupLevelUpItems();
        
        // 사운드 재생
        if (levelUpSound != null)
            AudioManager.instance?.PlaySfx(AudioManager.Sfx.LevelUp);
    }
    
    private void SetupLevelUpItems()
    {
        if (items == null) return;
        
        // 랜덤한 아이템들을 활성화
        for (int i = 0; i < items.Length; i++)
        {
            if (items[i] != null)
            {
                items[i].gameObject.SetActive(true);
                items[i].SetupRandomItem();
            }
        }
    }
    
    public void SelectLevelUpItem(int itemIndex)
    {
        if (itemIndex < 0 || itemIndex >= items.Length) return;
        
        // 선택한 아이템 적용
        items[itemIndex].OnSelect();
        
        // 레벨업 UI 닫기
        HideLevelUp();
        
        // 사운드 재생
        PlayButtonClickSound();
    }
    
    private void HideLevelUp()
    {
        isLevelUpActive = false;
        levelUpPanel.SetActive(false);
        GameManager.instance.Resume();
        
        // 모든 아이템 비활성화
        foreach (var item in items)
        {
            if (item != null)
                item.gameObject.SetActive(false);
        }
    }
    #endregion
    
    #region 결과 화면 시스템
    public void ShowGameResult(bool isWin)
    {
        if (resultPanel != null)
            resultPanel.SetActive(true);
            
        if (isWin)
        {
            ShowWin();
        }
        else
        {
            ShowLose();
        }
    }
    
    public void ShowWin()
    {
        if (resultTitles.Length > 1 && resultTitles != null)
            resultTitles.SetActive(true);
            
        // 스테이지 매니저에 승리 알림
        if (StageManager.instance != null)
        {
            StageManager.instance.OnGameVictory();
        }
    }
    
    public void ShowLose()
    {
        if (resultTitles.Length > 0 && resultTitles != null)
            resultTitles.SetActive(true);
    }
    
    public void ShowStageComplete()
    {
        if (stageCompletePanel != null)
            stageCompletePanel.SetActive(true);
    }
    
    public void HideResult()
    {
        if (resultPanel != null)
            resultPanel.SetActive(false);
            
        foreach (var title in resultTitles)
        {
            if (title != null)
                title.SetActive(false);
        }
        
        if (stageCompletePanel != null)
            stageCompletePanel.SetActive(false);
    }
    #endregion
    
    #region 버튼 이벤트
    public void OnRetryButtonClick()
    {
        PlayButtonClickSound();
        GameManager.instance.GameRetry();
    }
    
    public void OnNextStageButtonClick()
    {
        PlayButtonClickSound();
        
        if (StageManager.instance != null)
        {
            StageManager.instance.LoadNextStage();
        }
    }
    
    public void OnMainMenuButtonClick()
    {
        PlayButtonClickSound();
        // 메인 메뉴로 이동하는 로직 추가
        UnityEngine.SceneManagement.SceneManager.LoadScene(0);
    }
    
    private void PlayButtonClickSound()
    {
        AudioManager.instance?.PlaySfx(AudioManager.Sfx.Select);
    }
    #endregion
    
    #region 애니메이션 효과
    public void AnimateHealthBar(float targetValue, float duration = 0.5f)
    {
        if (healthSlider != null)
            StartCoroutine(AnimateSlider(healthSlider, targetValue, duration));
    }
    
    public void AnimateExpBar(float targetValue, float duration = 0.3f)
    {
        if (expSlider != null)
            StartCoroutine(AnimateSlider(expSlider, targetValue, duration));
    }
    
    private IEnumerator AnimateSlider(Slider slider, float targetValue, float duration)
    {
        float startValue = slider.value;
        float elapsed = 0f;
        
        while (elapsed < duration)
        {
            elapsed += Time.unscaledDeltaTime;
            float progress = elapsed / duration;
            slider.value = Mathf.Lerp(startValue, targetValue, progress);
            yield return null;
        }
        
        slider.value = targetValue;
    }
    #endregion
    
    #region 공용 메서드
    public void SetHealthBarValue(float value)
    {
        if (healthSlider != null)
            healthSlider.value = Mathf.Clamp01(value);
    }
    
    public void SetExpBarValue(float value)
    {
        if (expSlider != null)
            expSlider.value = Mathf.Clamp01(value);
    }
    
    public void UpdateKillCount(int kills)
    {
        if (killText != null)
            killText.text = kills.ToString();
    }
    
    public void UpdateLevel(int level)
    {
        if (levelText != null)
            levelText.text = $"Lv.{level}";
    }
    
    public bool IsLevelUpActive() => isLevelUpActive;
    #endregion
}
