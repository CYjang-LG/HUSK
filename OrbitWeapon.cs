// OrbitWeapon.cs
using UnityEngine;

public class OrbitWeapon : WeaponBase
{
    private const int OrbitPierceSentinel = -100; // 궤도형 탄환 구분 값

    void Start() => Batch();

    void Update()
    {
        if (!GameManager.instance.isLive) return;
        transform.Rotate(Vector3.back * orbitSpeed * Time.deltaTime);
    }

    public override void Init(ItemData data, PoolManager pool, int prefabId, Scanner scanner, float damageMul, int extraCount)
    {
        base.Init(data, pool, prefabId, scanner, damageMul, extraCount);
        // 초기 orbitSpeed는 캐릭터/장비 배수에서 ApplyRateMultipliers로 조정
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
}

