// ShooterWeapon.cs
using UnityEngine;

public class ShooterWeapon : WeaponBase
{
    private float timer;

    void Update()
    {
        if (!GameManager.instance.isLive) return;

        timer += Time.deltaTime;
        if (timer >= fireInterval)
        {
            timer = 0f;
            Fire();
        }
    }

    public override void Init(ItemData data, PoolManager pool, int prefabId, Scanner scanner, float damageMul, int extraCount)
    {
        base.Init(data, pool, prefabId, scanner, damageMul, extraCount);
        // fireInterval은 캐릭터/장비 배수에서 ApplyRateMultipliers로 조정
    }

    public override void LevelUp(float nextDamage, int addCount)
    {
        damage = nextDamage;
        count += addCount;
    }

    private void Fire()
    {
        if (!scanner || !scanner.nearestTarget) return;

        Vector3 targetPos = scanner.nearestTarget.position;
        Vector3 dir = (targetPos - transform.position).normalized;

        var bulletTf = pool.Get(prefabId)?.transform;
        if (bulletTf == null) return;

        bulletTf.position = transform.position;
        bulletTf.rotation = Quaternion.FromToRotation(Vector3.up, dir);
        var b = bulletTf.GetComponent<Bullet>();
        b.Init(damage, count, dir, 15f);

        AudioManager.instance.PlaySfx(AudioManager.Sfx.Range);
    }
}


