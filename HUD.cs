using UnityEngine;
using UnityEngine.UI;

public class HUD : MonoBehaviour
{
    public enum InfoType { Exp, Level, Kill, Time, Health }
    public InfoType type;

    private Text myText;
    private Slider mySlider;

    void Awake()
    {
        // InfoType에 따라 컴포넌트 할당
        if (type == InfoType.Exp || type == InfoType.Health)
        {
            mySlider = GetComponent<Slider>();
            if (mySlider == null)
                Debug.LogError($"HUD({type}): Slider 컴포넌트가 할당되지 않았습니다!");
        }
        else
        {
            myText = GetComponent<Text>();
            if (myText == null)
                Debug.LogError($"HUD({type}): Text 컴포넌트가 할당되지 않았습니다!");
        }
    }

    void LateUpdate()
    {
        // GameManager 인스턴스 유효성 검사
        if (GameManager.instance == null) return;

        switch (type)
        {
            case InfoType.Exp:
                if (mySlider == null) return;
                float curExp = GameManager.instance.exp;
                float maxExp = GameManager.instance.nextExp[
                    Mathf.Min(GameManager.instance.level,
                              GameManager.instance.nextExp.Length - 1)];
                mySlider.value = curExp / maxExp;
                break;

            case InfoType.Level:
                if (myText == null) return;
                myText.text = $"Lv.{GameManager.instance.level:F0}";
                break;

            case InfoType.Kill:
                if (myText == null) return;
                myText.text = $"{GameManager.instance.kill:F0}";
                break;

            case InfoType.Time:
                if (myText == null) return;
                float remainTime = GameManager.instance.maxGameTime
                                   - GameManager.instance.gameTime;
                int min = Mathf.FloorToInt(remainTime / 60);
                int sec = Mathf.FloorToInt(remainTime % 60);
                myText.text = $"{min:D2}:{sec:D2}";
                break;

            case InfoType.Health:
                if (mySlider == null) return;
                float curHealth = GameManager.instance.health;
                float maxHealth = GameManager.instance.maxHealth;
                mySlider.value = curHealth / maxHealth;
                break;
        }
    }
}
