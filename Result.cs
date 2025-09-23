using UnityEngine;

public class Result : MonoBehaviour
{
    public GameObject[] titles;
    public GameObject stageCompletePanel; // 스테이지 클리어 전용 패널

    public void Lose()
    {
        titles[0].SetActive(true);
    }

    public void Win()
    {
        titles[1].SetActive(true);

        // 스테이지 매니저에 승리 알림
        if (StageManager.instance != null)
        {
            StageManager.instance.OnGameVictory();
        }
    }

    public void ShowStageComplete()
    {
        if (stageCompletePanel != null)
            stageCompletePanel.SetActive(true);
    }
}
