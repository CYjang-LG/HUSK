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
            return false; // Input System �浹 �� �ӽ� ó��
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

    // �ܺο��� ���ݷ� ��� ����
    public void SetDamageMultiplier(float multiplier)
    {
        damageMultiplier = multiplier;
    }

    // �ܺο��� �˵� ���� �ӵ� ��� ����
    public void SetOrbitSpeedMultiplier(float multiplier)
    {
        orbitSpeedMultiplier = multiplier;
        // orbitBaseSpeed�� ���ؼ� �˵� ���� ���ۿ� �ݿ��ϴ� ���� �ʿ�
    }

    // �ܺο��� �߻� ���� ��� ���� (�������� ����)
    public void SetFireIntervalMultiplier(float multiplier)
    {
        fireRateMultiplier = multiplier;
    }
}
