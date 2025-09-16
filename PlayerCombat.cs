// PlayerCombat.cs
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Scanner))]
public class PlayerCombat : MonoBehaviour
{
    public Scanner scanner;
    public HandController hands;

    // 공격 관련 전역 배수(장비/캐릭터 프로필에서 적용)
    private float dmgMul = 1f;
    private float orbitSpeedMul = 1f;   // 근접 회전 속도 배수
    private float fireIntervalMul = 1f; // 발사 간격 배수(작을수록 빨라짐)

    private readonly List<WeaponBase> weapons = new List<WeaponBase>();

    void Awake()
    {
        scanner = GetComponent<Scanner>();
        if (hands == null) hands = GetComponentInChildren<HandController>(true);
    }

    public void SetDamageMultiplier(float mul) => dmgMul = Mathf.Max(0f, mul);
    public void SetOrbitSpeedMultiplier(float mul) => orbitSpeedMul = Mathf.Max(0f, mul);
    public void SetFireIntervalMultiplier(float mul) => fireIntervalMul = Mathf.Max(0f, mul);

    public void IncreaseAttackSpeed(float rate)
    {
        // rate 0.2 => 발사 간격 0.8배, 회전 속도 1.2배 등
        fireIntervalMul = Mathf.Max(0.01f, fireIntervalMul * (1f - rate));
        orbitSpeedMul = orbitSpeedMul * (1f + rate);
        foreach (var w in weapons)
            w.ApplyRateMultipliers(orbitSpeedMul, fireIntervalMul);
    }

    public WeaponBase AddWeapon(ItemData data, PoolManager pool, int prefabId, int extraCount)
    {
        var go = new GameObject($"Weapon_{data.itemType}_{data.itemId}");
        go.transform.SetParent(transform, false);

        WeaponBase weapon;
        if (data.itemType == ItemData.ItemType.Melee)
            weapon = go.AddComponent<OrbitWeapon>();
        else
            weapon = go.AddComponent<ShooterWeapon>();

        weapon.Init(data, pool, prefabId, scanner, dmgMul, extraCount);
        weapon.ApplyRateMultipliers(orbitSpeedMul, fireIntervalMul);
        weapons.Add(weapon);

        if (hands) hands.SetHandSprite((int)data.itemType, data.handSprite, true);
        return weapon;
    }

    public void LevelUpWeapon(WeaponBase weapon, float nextDamage, int addCount)
    {
        if (weapon == null) return;
        weapon.LevelUp(nextDamage * dmgMul, addCount);
    }
}
