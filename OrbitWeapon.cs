using UnityEngine;

public class OrbitWeapon : WeaponBase
{
    // 회전 무기의 속도 구현
    private float _speed = 150f;
    public override float Speed
    {
        get => _speed;
        set => _speed = value;
    }

    private const int OrbitPierceSentinel = -100;

    void Start() => Batch();

    void Update()
    {
        if (!GameManager.instance.isLive) return;
        transform.Rotate(Vector3.back * _speed * Time.deltaTime);
    }

    public override void Init(ItemData data, PoolManager pool, int prefabId, Scanner scanner, float damageMul, int extraCount)
    {
        base.Init(data, pool, prefabId, scanner, damageMul, extraCount);
        _speed = 150f; // 기본값, 필요시 Gear 등에서 변경
    }

    public override void LevelUp(float nextDamage, int addCount)
    {
        damage = nextDamage;
        count += addCount;
        Batch();
    }

    private void Batch()
    {
        for (int i = 0; i < count; i++)
        {
            Transform bullet;
            if (i < transform.childCount)
            {
                bullet = transform.GetChild(i);
            }
            else
            {
                bullet = pool.Get(prefabId)?.transform;
                if (bullet == null) continue;
                bullet.SetParent(transform, false);
            }

            bullet.localPosition = Vector3.zero;
            bullet.localRotation = Quaternion.identity;

            Vector3 rotVec = Vector3.forward * (360f * i / count);
            bullet.Rotate(rotVec);
            bullet.Translate(bullet.up * 1.5f, Space.World);

            var b = bullet.GetComponent<Bullet>();
            b.Init(damage, OrbitPierceSentinel, Vector3.zero, 0f);
        }
    }

    public override void ApplyRateMultipliers(float orbitSpeedMul, float fireIntervalMul)
    {
        base.ApplyRateMultipliers(orbitSpeedMul, fireIntervalMul);
        _speed = 150f * orbitSpeedMul;
    }
}
