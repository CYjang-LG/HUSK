using UnityEngine;
using UnityEngine.UI;

public class Item : MonoBehaviour
{
    [Header("Item Data")]
    public ItemData data;
    [Header("Level")]
    public int level;

    [Header("References")]
    public WeaponBase weaponBase;
    public Gear gear;

    // UI 참조
    private Image icon;
    private Text textLevel;
    private Text textName;
    private Text textDesc;

    // PlayerSetup 참조
    private PlayerSetup playerSetup;

    void Awake()
    {
        // UI 컴포넌트 캐싱
        var images = GetComponentsInChildren<Image>(true);
        if (images.Length > 1)
            icon = images[1];
        if (icon != null)
            icon.sprite = data.itemIcon;

        var texts = GetComponentsInChildren<Text>(true);
        if (texts.Length >= 3)
        {
            textLevel = texts[0];
            textName = texts[1];
            textDesc = texts[2];
        }
        if (textName != null)
            textName.text = data.itemName;

        // PlayerSetup 참조
        playerSetup = GameManager.instance.player;
        if (playerSetup == null)
            Debug.LogError("Item: GameManager.instance.player가 할당되지 않았습니다!");
    }

    void OnEnable()
    {
        if (textLevel != null)
            textLevel.text = $"Lv.{level + 1}";

        if (textDesc != null)
        {
            switch (data.itemType)
            {
                case ItemData.ItemType.Melee:
                case ItemData.ItemType.Range:
                    float dmgPercent = data.damages[level] * 100f;
                    int cnt = data.counts[level];
                    textDesc.text = string.Format(data.itemDesc, dmgPercent, cnt);
                    break;

                case ItemData.ItemType.Glove:
                case ItemData.ItemType.Shoe:
                    float ratePercent = data.damages[level] * 100f;
                    textDesc.text = string.Format(data.itemDesc, ratePercent);
                    break;

                default:
                    textDesc.text = data.itemDesc;
                    break;
            }
        }
    }

    public void OnClick()
    {
        // 회복 아이템
        if (data.itemType == ItemData.ItemType.Heal)
        {
            GameManager.instance.health = GameManager.instance.maxHealth;
            return;
        }

        // Gear 아이템 (Glove, Shoe)
        if (data.itemType == ItemData.ItemType.Glove || data.itemType == ItemData.ItemType.Shoe)
        {
            if (level == 0)
            {
                gear = new GameObject($"Gear_{data.itemId}")
                    .AddComponent<Gear>();
                gear.Init(data);
            }
            else
            {
                gear.LevelUp(data.damages[level]);
            }
        }
        // Weapon 아이템 (Melee, Range 등)
        else
        {
            // Scanner를 PlayerSetup에서 가져오기
            Scanner scanner = playerSetup.GetComponent<Scanner>();
            if (level == 0)
            {
                weaponBase = WeaponFactory.CreateWeapon(
                    data,
                    GameManager.instance.pool,
                    scanner,
                    playerSetup.transform
                );
            }
            else if (weaponBase != null)
            {
                float nextDamage = data.baseDamage * data.damages[level];
                int nextCount = data.counts[level];
                weaponBase.LevelUp(nextDamage, nextCount);
            }
        }

        level++;
    }
}
