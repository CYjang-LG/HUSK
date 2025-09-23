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
        UnlockCharacter();
    }

    void UnlockCharacter()
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

    void CheckAchievement(Achievement achievement)
    {
        bool isAchieved = false;

        switch (achievement)
        {
            case Achievement.UnlockPotato:
                if (GameManager.instance != null)
                    isAchieved = GameManager.instance.kill >= 10;
                break;
            case Achievement.UnlockBean:
                if (GameManager.instance != null)
                    isAchieved = GameManager.instance.gameTime >= GameManager.instance.maxGameTime;
                break;
        }

        if (isAchieved && PlayerPrefs.GetInt(achievement.ToString()) == 0)
        {
            PlayerPrefs.SetInt(achievement.ToString(), 1);

            if (uiNotice != null)
            {
                for (int index = 0; index < uiNotice.transform.childCount; index++)
                {
                    bool isActive = index == (int)achievement;
                    uiNotice.transform.GetChild(index).gameObject.SetActive(isActive);
                }
            }

            StartCoroutine(NoticeRoutine());
        }
    }

    IEnumerator NoticeRoutine()
    {
        if (uiNotice != null)
            uiNotice.SetActive(true);

        if (AudioManager.instance != null)
            AudioManager.instance.PlaySfx(AudioManager.Sfx.LevelUp);

        yield return wait;

        if (uiNotice != null)
            uiNotice.SetActive(false);
    }
}