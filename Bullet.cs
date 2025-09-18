using UnityEngine;

public class Bullet : MonoBehaviour
{
    private const int OrbitPierceSentinel = -100;
    public float damage;
    public int pierce;
    private Rigidbody2D rb;

    void Awake() => rb = GetComponent<Rigidbody2D>();

    public void Init(float dmg, int p, Vector3 dir, float speed)
    {
        damage = dmg; pierce = p;
        rb.linearVelocity = (p >= 0) ? dir * speed : Vector2.zero;
    }

    public void SetDamage(float d) => damage = d;

    void OnTriggerEnter2D(Collider2D c)
    {
        if (!c.CompareTag("Enemy") || pierce == OrbitPierceSentinel) return;
        pierce--;
        if (pierce < 0) { rb.linearVelocity = Vector2.zero; gameObject.SetActive(false); }
    }

    void OnTriggerExit2D(Collider2D c)
    {
        if (!c.CompareTag("Area") || pierce == OrbitPierceSentinel) return;
        gameObject.SetActive(false);
    }
}
