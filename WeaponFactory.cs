using UnityEngine;
using System;

public static class WeaponFactory
{
    public static WeaponBase CreateWeapon(ItemData data, PoolManager pool, UtilityManager utility, Transform parent)
    {
        if (data == null || pool == null || parent == null)
        {
            Debug.LogError("WeaponFactory: 필수 매개변수가 null입니다.");
            return null;
        }

        GameObject go = new GameObject($"Weapon_{data.itemId}");
        go.transform.SetParent(parent, false);

        WeaponBase weapon = data.itemType switch
        {
            ItemData.ItemType.Melee => go.AddComponent<CombinedWeapon>(),
            ItemData.ItemType.Range => go.AddComponent<CombinedWeapon>(),
            _ => throw new ArgumentException("지원하지 않는 ItemType")
        };

        // CombinedWeapon인 경우 FireMode 설정
        if (weapon is CombinedWeapon combinedWeapon)
        {
            combinedWeapon.fireMode = data.itemType == ItemData.ItemType.Melee ?
                CombinedWeapon.FireMode.Orbit : CombinedWeapon.FireMode.Straight;
        }

        int prefabIndex = -1;
        if (data.projectile != null && pool.prefabs != null)
        {
            prefabIndex = Array.IndexOf(pool.prefabs, data.projectile);
        }

        if (prefabIndex == -1)
        {
            prefabIndex = 0; // 기본값 사용
        }

        weapon.Init(data, pool, prefabIndex, utility, data.baseDamage, data.baseCount);
        return weapon;
    }
}