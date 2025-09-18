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
        if (type == InfoType.Exp || type == InfoType.Health)
        {
            mySlider = GetComponent<Slider>();
            if (mySlider == null)
                Debug.LogError($"HUD({type}): Slider 없음");
        }
        else
        {
            myText = GetComponent<Text>();
            if (myText == null)
                Debug.LogError($"HUD({type}): Text 없음");
        }
    }

    void LateUpdate()
    {
        if (GameManager.instance == null) return;

        switch (type)
        {
            case InfoType.Exp:
                if (mySlider == null) return;
                float ce = GameManager.instance.exp;
                float me = GameManager.instance.nextExp[
                    Mathf.Min(GameManager.instance.level,
                              GameManager.instance.nextExp.Length - 1)];
                mySlider.value = ce / me;
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
                float rt = GameManager.instance.maxGameTime - GameManager.instance.gameTime;
                int m = Mathf.FloorToInt(rt / 60);
                int s = Mathf.FloorToInt(rt % 60);
                myText.text = $"{m:D2}:{s:D2}";
                break;
            case InfoType.Health:
                if (mySlider == null) return;
                float ch = GameManager.instance.health;
                float mh = GameManager.instance.maxHealth;
                mySlider.value = ch / mh;
                break;
        }
    }
}
