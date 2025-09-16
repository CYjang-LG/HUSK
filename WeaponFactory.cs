using UnityEngine;
using System;

public static class WeaponFactory
{
    public static WeaponBase CreateWeapon(ItemData data, PoolManager pool, Scanner scanner, Transform parent)
    {
        GameObject go = new GameObject($"Weapon_{data.itemId}");
        go.transform.SetParent(parent, false);

        WeaponBase weapon = data.itemType switch
        {
            ItemData.ItemType.Melee => go.AddComponent<OrbitWeapon>(),
            ItemData.ItemType.Range => go.AddComponent<ShooterWeapon>(),
            _ => throw new ArgumentException("지원하지 않는 ItemType")
        };

        int prefabIndex = Array.IndexOf(pool.prefabs, data.projectile);
        weapon.Init(data, pool, prefabIndex, scanner, data.baseDamage, data.baseCount);
        return weapon;
    }
}
