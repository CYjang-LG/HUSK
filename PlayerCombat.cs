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
    private float OrbitSpeedMultiplier = 1f;


    void Awake()
    {
        joystick = Object.FindFirstObjectByType<VirtualJoystick>();
    }

    void Update()
    {
        if (IsFirePressed() && Time.time - lastFireTime > baseFireRate/fireRateMultiplier)
        {
            Shoot();
            lastFireTime = Time.time;
        }
    }

    private bool IsFirePressed()
    {
#if UNITY_EDITOR
        try { return Input.GetButtonDown("Fire1"); }
        catch { return false; }
#else
        return joystick != null && joystick.IsFiring;
#endif
    }

    private void Shoot()
    {
        var b=Instantiate(bulletPrefab, firePoint.position, firePoint.rotation);
        if (b.TryGetComponent(out Bullet bs))
            bs.SetDamage(baseDamage * damageMultiplier);
    }


    public void SetDamageMultiplier(float m) => damageMultiplier = m;
    public void SetOrbitSpeedMultiplier(float m) => OrbitSpeedMultiplier = m;
    public void SetFireIntervalMultiplier(float m) => fireRateMultiplier = m;
}
