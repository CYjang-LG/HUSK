using UnityEngine;

public class Bullet : MonoBehaviour
{
    private const int OrbitPierceSentinel = -100;
    public float damage;
    public int pierce;
    private Rigidbody2D rb;

    void Awake() => rb = GetComponent<Rigidbody2D>();

    public void Init(float damage, int pierce, Vector3 dir, float speed)
    {
        this.damage = damage;
        this.pierce = pierce;
        if (pierce >= 0)
        {
            rb.linearVelocity = dir * speed;
        }
        else
        {
            rb.linearVelocity = Vector2.zero; // 궤도형
        }
    }

    // 새로 추가하는 데미지 설정 메서드
    public void SetDamage(float dmg)
    {
        damage = dmg;
    }

    void OnTriggerEnter2D(Collider2D collision)
    {
        // 원거리 탄만 소멸 규칙 적용
        if (!collision.CompareTag("Enemy") || pierce == OrbitPierceSentinel) return;
        pierce--;
        if (pierce < 0)
        {
            rb.linearVelocity = Vector2.zero;
            gameObject.SetActive(false);
        }
    }

    void OnTriggerExit2D(Collider2D collision)
    {
        if (!collision.CompareTag("Area") || pierce == OrbitPierceSentinel) return;
        gameObject.SetActive(false);
    }
}
