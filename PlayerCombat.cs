// PlayerCombat.cs - 수정된 버전
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Scanner))]
public class PlayerCombat : MonoBehaviour
{
    [Header("References")]
    public Scanner scanner;
    public HandController hands;

    [Header("Combat Multipliers")]
    // 공격 관련 전역 배수(장비/캐릭터 프로필에서 적용)
    private float dmgMul = 1f;
    private float orbitSpeedMul = 1f; // 근접 회전 속도 배수
    private float fireIntervalMul = 1f; // 발사 간격 배수(작을수록 빨라짐)

    // 무기 관리
    private readonly List<WeaponBase> weapons = new List<WeaponBase>();

    void Awake()
    {
        scanner = GetComponent<Scanner>();
        if (hands == null) 
            hands = GetComponentInChildren<HandController>(true);
    }

    #region Multiplier Setters
    public void SetDamageMultiplier(float mul) => dmgMul = Mathf.Max(0f, mul);
    public void SetOrbitSpeedMultiplier(float mul) => orbitSpeedMul = Mathf.Max(0f, mul);
    public void SetFireIntervalMultiplier(float mul) => fireIntervalMul = Mathf.Max(0f, mul);

    public void IncreaseAttackSpeed(float rate)
    {
        // rate 0.2 => 발사 간격 0.8배, 회전 속도 1.2배
        fireIntervalMul = Mathf.Max(0.01f, fireIntervalMul * (1f - rate));
        orbitSpeedMul = orbitSpeedMul * (1f + rate);
        
        // 모든 기존 무기에 적용
        foreach (var weapon in weapons)
        {
            weapon.ApplyRateMultipliers(orbitSpeedMul, fireIntervalMul);
        }
    }
    #endregion

    #region Weapon Management
    /// <summary>
    /// WeaponFactory를 통해 무기를 추가하고 관리 리스트에 등록
    /// </summary>
    public WeaponBase AddWeapon(ItemData data, int extraCount = 0)
    {
        // WeaponFactory 사용
        WeaponBase weapon = WeaponFactory.CreateWeapon(
            data,
            GameManager.instance.pool,
            scanner,
            transform
        );

        // 배수 적용
        weapon.ApplyRateMultipliers(orbitSpeedMul, fireIntervalMul);
        
        // 관리 리스트에 추가
        weapons.Add(weapon);

        // 손 스프라이트 설정
        if (hands != null && data.handSprite != null)
        {
            hands.SetHandSprite((int)data.itemType, data.handSprite, true);
        }

        return weapon;
    }

    /// <summary>
    /// 기존 메서드 호환성 유지 (레거시 지원)
    /// </summary>
    public WeaponBase AddWeapon(ItemData data, PoolManager pool, int prefabId, int extraCount)
    {
        return AddWeapon(data, extraCount);
    }

    /// <summary>
    /// 무기 레벨업
    /// </summary>
    public void LevelUpWeapon(WeaponBase weapon, float nextDamage, int addCount)
    {
        if (weapon == null) return;
        weapon.LevelUp(nextDamage * dmgMul, addCount);
    }

    /// <summary>
    /// 특정 무기 제거
    /// </summary>
    public void RemoveWeapon(WeaponBase weapon)
    {
        if (weapon == null) return;
        
        weapons.Remove(weapon);
        Destroy(weapon.gameObject);
    }

    /// <summary>
    /// 모든 무기 제거
    /// </summary>
    public void ClearAllWeapons()
    {
        foreach (var weapon in weapons)
        {
            if (weapon != null)
                Destroy(weapon.gameObject);
        }
        weapons.Clear();
    }
    #endregion

    #region Gear Integration
    /// <summary>
    /// Gear 시스템에서 호출되는 메서드 (브로드캐스트 메시지)
    /// </summary>
    public void ApplyGear()
    {
        // 모든 무기에 현재 배수 재적용
        foreach (var weapon in weapons)
        {
            if (weapon != null)
                weapon.ApplyRateMultipliers(orbitSpeedMul, fireIntervalMul);
        }
    }
    #endregion

    #region Utility
    /// <summary>
    /// 현재 장착된 무기 수
    /// </summary>
    public int GetWeaponCount() => weapons.Count;

    /// <summary>
    /// 특정 타입의 무기 찾기
    /// </summary>
    public WeaponBase GetWeaponByType(ItemData.ItemType itemType)
    {
        foreach (var weapon in weapons)
        {
            // ItemData에서 타입 정보를 가져오려면 추가 구현 필요
            // 현재는 OrbitWeapon/ShooterWeapon으로만 구분 가능
            if (itemType == ItemData.ItemType.Melee && weapon is OrbitWeapon)
                return weapon;
            if (itemType == ItemData.ItemType.Range && weapon is ShooterWeapon)
                return weapon;
        }
        return null;
    }
    #endregion

    void OnDestroy()
    {
        ClearAllWeapons();
    }
}
