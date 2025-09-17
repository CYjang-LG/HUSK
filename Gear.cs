using UnityEngine;

public class Gear : MonoBehaviour
{
    public ItemData.ItemType type;
    public float rate;

    public void Init(ItemData data)
    {
        name = "Gear" + data.itemId;
        transform.parent = GameManager.instance.player.transform;
        transform.localPosition = Vector3.zero;

        type = data.itemType;
        rate = data.damages[0];
        ApplyGear();
    }

    public void LevelUp(float newRate)
    {
        rate = newRate;
        ApplyGear();
    }

    /// <summary>
    /// 적용 대상 무기들에 변경된 속성 적용
    /// </summary>
    void ApplyGear()
    {
        var weapons = transform.parent.GetComponentsInChildren<WeaponBase>();

        switch (type)
        {
            case ItemData.ItemType.Glove:
                foreach (var weapon in weapons)
                {
                    // 회전형/발사형 모두 대응
                    if (weapon is OrbitWeapon) // 근접 = 회전형
                    {
                        weapon.Speed = 150f + (150f * rate);
                    }
                    else if (weapon is ShooterWeapon) // 원거리
                    {
                        weapon.Speed = 0.5f * (1f - rate);
                    }
                }
                break;
            case ItemData.ItemType.Shoe:
                // 플레이어 이동 속도 조정
                var playerComp = GameManager.instance.player;
                if (playerComp != null)
                {
                    playerComp.speed = 3f + 3f * rate;
                }
                break;
            default:
                break;
        }
    }
}
