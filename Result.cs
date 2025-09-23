using UnityEngine;

public class Result : MonoBehaviour
{
    public GameObject[] titles;
    public GameObject stageCompletePanel; // �������� Ŭ���� ���� �г�

    public void Lose()
    {
        titles[0].SetActive(true);
    }

    public void Win()
    {
        titles[1].SetActive(true);

        // �������� �Ŵ����� �¸� �˸�
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
