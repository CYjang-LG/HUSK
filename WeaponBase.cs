using UnityEngine;

public abstract class WeaponBase : MonoBehaviour
{
    public int Id { get; protected set; }
    protected int prefabId;
    protected float damage;
    protected int count;
    protected PoolManager pool;
    protected UtilityManager utilityManager;
    protected float orbitSpeed = 150f;
    protected float fireInterval = 0.5f;
    public abstract float Speed { get; set; }

    public virtual void Init(ItemData data, PoolManager pool, int prefabId, UtilityManager utility, float damageMul, int extraCount)
    {
        if (data != null)
        {
            name = $"Weapon{data.itemId}";
            Id = data.itemId;
            this.damage = data.baseDamage * damageMul;
            this.count = data.baseCount + extraCount;
        }

        transform.localPosition = Vector3.zero;
        this.pool = pool;
        this.prefabId = prefabId;
        this.utilityManager = utility;
    }

    public abstract void LevelUp(float nextDamage, int addCount);

    public virtual void ApplyRateMultipliers(float orbitSpeedMul, float fireIntervalMul)
    {
        orbitSpeed *= orbitSpeedMul;
        fireInterval = Mathf.Max(0.01f, fireInterval * fireIntervalMul);
    }
}