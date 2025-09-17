using UnityEngine;

public class PlayerCombat : MonoBehaviour
{
    public GameObject bulletPrefab;
    public Transform firePoint;
    private VirtualJoystick joystick;

    private float lastFireTime;
    private float baseFireRate = 0.5f;
    private float fireRateMultiplier = 1f;

    private float baseDamage = 10f;
    private float damageMultiplier = 1f;

    private float orbitBaseSpeed = 100f;
    private float orbitSpeedMultiplier = 1f;

    void Awake()
    {
        joystick = Object.FindFirstObjectByType<VirtualJoystick>();
    }

    void Update()
    {
        if (IsFirePressed() && Time.time - lastFireTime > GetFireRate())
        {
            Shoot();
            lastFireTime = Time.time;
        }
    }

    private bool IsFirePressed()
    {
#if UNITY_EDITOR
        try
        {
            return Input.GetButtonDown("Fire1");
        }
        catch (System.Exception)
        {
            return false; // Input System 충돌 시 임시 처리
        }
#else
    return joystick != null && joystick.IsFiring;
#endif
    }

    private void Shoot()
    {
        GameObject bullet = Instantiate(bulletPrefab, firePoint.position, firePoint.rotation);
        Bullet bulletScript = bullet.GetComponent<Bullet>();
        if (bulletScript != null)
        {
            bulletScript.SetDamage(baseDamage * damageMultiplier);
        }
    }

    public void IncreaseAttackSpeed(float multiplier)
    {
        fireRateMultiplier *= multiplier;
    }

    private float GetFireRate()
    {
        return baseFireRate / fireRateMultiplier;
    }

    // 외부에서 공격력 배수 설정
    public void SetDamageMultiplier(float multiplier)
    {
        damageMultiplier = multiplier;
    }

    // 외부에서 궤도 무기 속도 배수 설정
    public void SetOrbitSpeedMultiplier(float multiplier)
    {
        orbitSpeedMultiplier = multiplier;
        // orbitBaseSpeed에 곱해서 궤도 무기 동작에 반영하는 로직 필요
    }

    // 외부에서 발사 간격 배수 설정 (높을수록 빠름)
    public void SetFireIntervalMultiplier(float multiplier)
    {
        fireRateMultiplier = multiplier;
    }
}
