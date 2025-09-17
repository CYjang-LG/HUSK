using UnityEngine;

/// <summary>
/// 모든 무기 공통 기반 클래스 (확장성과 캡슐화 강화)
/// </summary>
public abstract class WeaponBase : MonoBehaviour
{
    // 공용 일련번호, 외부에서 읽기만 가능
    public int Id { get; protected set; }

    protected int prefabId;
    protected float damage;
    protected int count;
    protected PoolManager pool;
    protected Scanner scanner;

    // 배수 (회전 속도, 발사 간격 - WeaponBase 기준값)
    protected float orbitSpeed = 150f;
    protected float fireInterval = 0.5f;

    // 무기 동작 속성: 속성으로 외부에 노출, 하위에서 구현 (get/set 가능)
    public abstract float Speed { get; set; }

    /// <summary>
    /// 초기화 - 하위 클래스에서 반드시 base.Init 호출
    /// </summary>
    public virtual void Init(ItemData data, PoolManager pool, int prefabId, Scanner scanner, float damageMul, int extraCount)
    {
        name = $"Weapon{data.itemId}";
        transform.localPosition = Vector3.zero;
        Id = data.itemId;
        this.pool = pool;
        this.prefabId = prefabId;
        this.scanner = scanner;

        damage = data.baseDamage * damageMul;
        count = data.baseCount + extraCount;
    }

    // 무기 레벨업 내용은 각 무기에서 구현
    public abstract void LevelUp(float nextDamage, int addCount);

    /// <summary>
    /// 배수 적용(회전 속도/발사간격 등) - 하위에서 오버라이드 권장
    /// </summary>
    public virtual void ApplyRateMultipliers(float orbitSpeedMul, float fireIntervalMul)
    {
        orbitSpeed *= orbitSpeedMul;
        fireInterval = Mathf.Max(0.01f, fireInterval * fireIntervalMul);
    }
}
