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

    private Player player;

    void Awake()
    {
        // UI 컴포넌트 캐싱
        var images = GetComponentsInChildren<Image>(true);
        icon = images.Length > 1 ? images[1] : null;
        icon.sprite = data.itemIcon;

        var texts = GetComponentsInChildren<Text>(true);
        if (texts.Length >= 3)
        {
            textLevel = texts[0];
            textName  = texts[1];
            textDesc  = texts[2];
        }
        textName.text = data.itemName;

        // 플레이어 참조
        player = GameManager.instance.player;
    }

    void OnEnable()
    {
        // 레벨 표시
        textLevel.text = $"Lv.{level + 1}";

        // 설명 업데이트
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
                textDesc.text = string.Format(data.itemDesc);
                break;
        }
    }

    public void OnClick()
    {
        // 이미 회복 아이템이라면 즉시 전체 체력 회복
        if (data.itemType == ItemData.ItemType.Heal)
        {
            GameManager.instance.health = GameManager.instance.maxHealth;
            return;
        }

        // Gear 또는 Weapon 생성 또는 레벨업
        if (data.itemType == ItemData.ItemType.Glove || data.itemType == ItemData.ItemType.Shoe)
        {
            if (level == 0)
            {
                // Gear 생성
                gear = new GameObject($"Gear_{data.itemId}")
                    .AddComponent<Gear>();
                gear.Init(data);
            }
            else
            {
                gear.LevelUp(data.damages[level]);
            }
        }
        else
        {
            if (level == 0)
            {
                // WeaponBase 생성 (팩토리 없이 직접 호출 예시)
                weaponBase = WeaponFactory.CreateWeapon(
                    data,
                    GameManager.instance.pool,
                    player.scanner,
                    player.transform
                );
            }
            else
            {
                float nextDamage = data.baseDamage * data.damages[level];
                int nextCount   = data.counts[level];
                weaponBase.LevelUp(nextDamage, nextCount);
            }
        }

        level++;
    }
}
