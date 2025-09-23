using UnityEngine;

[CreateAssetMenu(fileName = "GameConditions", menuName = "Game/Game Conditions")]
public class GameConditions : MonoBehaviour
{
    [Header("½Â¸®Á¶°Ç(or)")]
    public bool useTimeLimit = true;
    public float timeLimit = 300f;

    public bool useKillTarget = false;
    public int killTarget = 50;

    public bool useBossKill = false;
    public string bossTag = "Boss";

    [Header("ÆÐ¹èÁ¶°Ç")]
    public bool useHealthZero = true;
    public bool useMaxEnemies = false;
    public int maxEnemiesOnScreen = 100;

}
