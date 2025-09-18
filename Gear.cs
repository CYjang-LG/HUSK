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
    /// ���� ��� ����鿡 ����� �Ӽ� ����
    /// </summary>
    void ApplyGear()
    {
        var weapons = transform.parent.GetComponentsInChildren<WeaponBase>();

        switch (type)
        {
            case ItemData.ItemType.Glove:
                foreach (var weapon in weapons)
                {
                    // ȸ����/�߻��� ��� ����
                    if (weapon is OrbitWeapon) // ���� = ȸ����
                    {
                        weapon.Speed = 150f + (150f * rate);
                    }
                    else if (weapon is ShooterWeapon) // ���Ÿ�
                    {
                        weapon.Speed = 0.5f * (1f - rate);
                    }
                }
                break;
            case ItemData.ItemType.Shoe:
                // �÷��̾� �̵� �ӵ� ����
                var playerSetup = GameManager.instance.player;
                if(playerSetup != null)
                {
                    var movement = playerSetup.GetComponent<PlayerMovement>();
                    if(movement != null)
                    {
                        float newSpeed = 3f + 3f * rate;
                        movement.SetBaseMoveSpeed(newSpeed);
                    }
                }
                break;
            default:
                break;
        }
    }
}
