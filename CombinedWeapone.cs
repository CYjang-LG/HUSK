using System.Collections;
using UnityEngine;

/// <summary>
/// 통합 무기 클래스
/// Shooter(직선 발사) 및 Orbit(궤도 회전) 모드를 지원
/// WeaponBase를 상속하여 Init, LevelUp, ApplyRateMultipliers 기능 재사용
/// </summary>
public class CombinedWeapon : WeaponBase
{
    public enum FireMode { Straight, Orbit }
    [Header("=== 공통 설정 ===")]
    public FireMode fireMode = FireMode.Straight;
    public GameObject projectilePrefab;
    public Transform firePoint;

    [Header("=== Straight 모드 설정 ===")]
    public float straightSpeed = 10f;

    [Header("=== Orbit 모드 설정 ===")]
    public float orbitRadius = 1.5f;
    private float orbitAngle = 0f;

    private Coroutine fireRoutine;

    void Awake()
    {
        if (firePoint == null)
        {
            GameObject firePointObj = new GameObject("FirePoint");
            firePointObj.transform.SetParent(transform);
            firePointObj.transform.localPosition = Vector3.right * 0.5f;
            firePoint = firePointObj.transform;
        }
    }

    public override float Speed
    {
        get => (fireMode == FireMode.Straight) ? straightSpeed : orbitSpeed;
        set
        {
            if (fireMode == FireMode.Straight)
                straightSpeed = value;
            else
                orbitSpeed = value;
        }
    }

    public override void Init(ItemData data, PoolManager pool, int prefabId, UtilityManager utility, float damageMul, int extraCount)
    {
        base.Init(data, pool, prefabId, utility, damageMul, extraCount);
        projectilePrefab = data.projectile;
        damage = data.baseDamage * damageMul;
        count = data.baseCount + extraCount;

        StartFiring();
    }

    public override void LevelUp(float nextDamage, int addCount)
    {
        damage = nextDamage;
        count += addCount;
    }

    public void StartFiring()
    {
        if (fireRoutine == null)
            fireRoutine = StartCoroutine(FireLoop());
    }

    public void StopFiring()
    {
        if (fireRoutine != null)
        {
            StopCoroutine(fireRoutine);
            fireRoutine = null;
        }
    }

    private IEnumerator FireLoop()
    {
        while (true)
        {
            if (GameManager.instance != null && GameManager.instance.isLive)
            {
                if (fireMode == FireMode.Straight)
                    FireStraight();
                else
                    FireOrbit();
            }

            yield return new WaitForSeconds(fireInterval);
        }
    }

    private void FireStraight()
    {
        if (pool == null || prefabId >= pool.prefabs.Length || firePoint == null) return;

        for (int i = 0; i < count; i++)
        {
            GameObject proj = pool.Get(prefabId);
            if (proj == null) continue;

            proj.transform.position = firePoint.position;
            proj.transform.rotation = firePoint.rotation;

            var rb = proj.GetComponent<Rigidbody2D>();
            if (rb != null)
                rb.linearVelocity = firePoint.right * straightSpeed;

            var bullet = proj.GetComponent<Bullet>();
            if (bullet != null)
                bullet.damage = damage;
        }
    }

    private void FireOrbit()
    {
        if (pool == null || prefabId >= pool.prefabs.Length || firePoint == null) return;

        orbitAngle += orbitSpeed * Time.deltaTime;
        for (int i = 0; i < count; i++)
        {
            float angle = orbitAngle + (360f / count) * i;
            Vector3 offset = new Vector3(
                Mathf.Cos(angle * Mathf.Deg2Rad),
                Mathf.Sin(angle * Mathf.Deg2Rad),
                0f) * orbitRadius;

            GameObject proj = pool.Get(prefabId);
            if (proj == null) continue;

            proj.transform.position = firePoint.position + offset;
            proj.transform.rotation = Quaternion.Euler(0, 0, angle);

            var bullet = proj.GetComponent<Bullet>();
            if (bullet != null)
                bullet.damage = damage;
        }
    }
}