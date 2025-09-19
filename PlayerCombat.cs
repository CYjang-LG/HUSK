// ================ 수정된 PlayerCombat.cs (누락된 메서드들 추가) ================
using UnityEngine;

public class PlayerCombat : MonoBehaviour
{
    [Header("기본 공격 설정")]
    public GameObject bulletPrefab;
    public Transform firePoint;

    private VirtualJoystick joystick;
    private Scanner scanner;

    [Header("공격 스탯")]
    private float lastFireTime;
    private float baseFireRate = 0.5f;
    private float fireRateMultiplier = 1f;
    private float baseDamage = 10f;
    private float damageMultiplier = 1f;
    private float orbitSpeedMultiplier = 1f; // 🔥 누락된 필드 추가

    void Awake()
    {
        joystick = Object.FindFirstObjectByType<VirtualJoystick>();
        scanner = GetComponent<Scanner>();

        // firePoint가 없다면 생성
        if (firePoint == null)
        {
            GameObject firePointObj = new GameObject("FirePoint");
            firePointObj.transform.SetParent(transform);
            firePointObj.transform.localPosition = Vector3.right * 0.5f;
            firePoint = firePointObj.transform;
        }
    }

    void Update()
    {
        // 자동 공격 (가장 가까운 적 공격)
        if (scanner != null && scanner.nearestTarget != null)
        {
            if (Time.time - lastFireTime > baseFireRate / fireRateMultiplier)
            {
                AutoShoot();
                lastFireTime = Time.time;
            }
        }

        // 수동 공격 (조이스틱/마우스 입력)
        if (IsFirePressed() && Time.time - lastFireTime > baseFireRate / fireRateMultiplier)
        {
            ManualShoot();
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

    private void AutoShoot()
    {
        if (scanner.nearestTarget == null) return;

        Vector3 direction = (scanner.nearestTarget.position - firePoint.position).normalized;
        CreateBullet(direction);
    }

    private void ManualShoot()
    {
        Vector3 direction = Vector3.right; // 기본 오른쪽 방향

#if UNITY_EDITOR
        // 마우스 방향으로 발사
        Vector3 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        mousePos.z = 0;
        direction = (mousePos - firePoint.position).normalized;
#else
        // 조이스틱 방향으로 발사
        if (joystick != null && joystick.inputVector.magnitude > 0.1f)
        {
            direction = new Vector3(joystick.Horizontal, joystick.Vertical, 0).normalized;
        }
#endif

        CreateBullet(direction);
    }

    private void CreateBullet(Vector3 direction)
    {
        if (bulletPrefab != null)
        {
            GameObject bullet = Instantiate(bulletPrefab, firePoint.position, Quaternion.identity);

            // 총알 방향 설정
            bullet.transform.rotation = Quaternion.LookRotation(Vector3.forward, direction);

            // 총알 컴포넌트 초기화
            if (bullet.TryGetComponent(out Bullet bulletScript))
            {
                bulletScript.Init(baseDamage * damageMultiplier, 1, direction, 15f);
            }

            AudioManager.instance.PlaySfx(AudioManager.Sfx.Range);
        }
    }

    // 🔥 PlayerSetup에서 호출하는 메서드들 추가
    public void SetDamageMultiplier(float m) => damageMultiplier = m;
    public void SetFireRateMultiplier(float m) => fireRateMultiplier = m;
    public void SetOrbitSpeedMultiplier(float m) => orbitSpeedMultiplier = m;
    public void SetFireIntervalMultiplier(float m) => fireRateMultiplier = m;
}