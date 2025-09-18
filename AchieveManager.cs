using System;
using System.Collections;
using UnityEngine;

public class AchievementManager : MonoBehaviour
{
    public GameObject[] lockCharacter;
    public GameObject[] unlockCharacter;
    public GameObject uiNotice;
    
    enum Achievement { UnlockPotato, UnlockBean }
    Achievement[] achievements;
    WaitForSecondsRealtime wait;

    void Awake()
    {
        achievements = (Achievement[])Enum.GetValues(typeof(Achievement));
        wait = new WaitForSecondsRealtime(5);

        if (!PlayerPrefs.HasKey("MyData"))
        {
            Init();
        }
    }

    void Init()
    {
        PlayerPrefs.SetInt("MyData", 1);

        foreach (Achievement achievement in achievements)
        {
            PlayerPrefs.SetInt(achievement.ToString(), 0);
        }
    }

    void Start()
    {
        UnlockCharacter(); // 오타 수정
    }

    void UnlockCharacter() // 오타 수정
    {
        for (int index = 0; index < lockCharacter.Length; index++)
        {
            string achievementName = achievements[index].ToString();
            bool isUnlock = PlayerPrefs.GetInt(achievementName) == 1;
            lockCharacter[index].SetActive(!isUnlock);
            unlockCharacter[index].SetActive(isUnlock);
        }
    }

    void LateUpdate()
    {
        foreach (Achievement achievement in achievements)
        {
            CheckAchievement(achievement);
        }
    }

    void CheckAchievement(Achievement achievement) // 오타 수정
    {
        bool isAchieved = false; // 변수명 개선

        switch (achievement)
        {
            case Achievement.UnlockPotato:
                isAchieved = GameManager.instance.kill >= 10;
                break;
            case Achievement.UnlockBean:
                isAchieved = GameManager.instance.gameTime == GameManager.instance.maxGameTime;
                break;
        }

        if (isAchieved && PlayerPrefs.GetInt(achievement.ToString()) == 0)
        {
            PlayerPrefs.SetInt(achievement.ToString(), 1);

            for (int index = 0; index < uiNotice.transform.childCount; index++)
            {
                bool isActive = index == (int)achievement;
                uiNotice.transform.GetChild(index).gameObject.SetActive(isActive);
            }

            StartCoroutine(NoticeRoutine()); // 오타 수정
        }
    }

    IEnumerator NoticeRoutine() // 오타 수정
    {
        uiNotice.SetActive(true);
        AudioManager.instance.PlaySfx(AudioManager.Sfx.LevelUp);
        yield return wait;
        uiNotice.SetActive(false);
    }
}
