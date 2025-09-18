// PlayerEquipment.cs
using UnityEngine;

[RequireComponent(typeof(PlayerMovement))]
[RequireComponent(typeof(PlayerCombat))]
public class PlayerEquipment : MonoBehaviour
{
    private PlayerMovement movement;
    private PlayerCombat combat;

    void Awake()
    {
        movement = GetComponent<PlayerMovement>();
        combat = GetComponent<PlayerCombat>();
    }

    public void ApplyGear(ItemData data, int level)
    {
        // level 인덱스에 맞는 rate/damage 등 적용
        float rate = data.damages[Mathf.Clamp(level, 0, data.damages.Length - 1)];

        switch (data.itemType)
        {
            case ItemData.ItemType.Shoe:
                movement.SetSpeedMultiplier(1f + rate);
                break;
            case ItemData.ItemType.Glove:
                combat.SetDamageMultiplier(rate);
                break;
        }
    }

    public void LevelUpGear(ItemData data, int level)
    {
        ApplyGear(data, level);
    }
}
