// WeaponBase.cs
using UnityEngine;

public abstract class WeaponBase : MonoBehaviour
{
    protected int id;
    protected int prefabId;
    protected float damage;
    protected int count;
    protected PoolManager pool;
    protected Scanner scanner;

    protected float orbitSpeed = 150f;      // deg/sec (근접)
    protected float fireInterval = 0.5f;    // sec/shot (원거리)

    public virtual void Init(ItemData data, PoolManager pool, int prefabId, Scanner scanner, float damageMul, int extraCount)
    {
        name = $"Weapon{data.itemId}";
        transform.localPosition = Vector3.zero;

        id = data.itemId;
        this.pool = pool;
        this.prefabId = prefabId;
        this.scanner = scanner;

        damage = data.baseDamage * damageMul;
        count = data.baseCount + extraCount;
    }

    public abstract void LevelUp(float nextDamage, int addCount);

    public virtual void ApplyRateMultipliers(float orbitSpeedMul, float fireIntervalMul)
    {
        orbitSpeed *= orbitSpeedMul;
        fireInterval = Mathf.Max(0.01f, fireInterval * fireIntervalMul);
    }
}
