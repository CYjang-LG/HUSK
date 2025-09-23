// Bullet.cs (변경 없음)
using UnityEngine;

public class Bullet : MonoBehaviour
{
    public float damage;
    private Rigidbody2D rb;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    void OnEnable()
    {
        Invoke(nameof(Deactivate), 5f);
    }

    void OnDisable()
    {
        CancelInvoke();
    }

    private void Deactivate()
    {
        gameObject.SetActive(false);
    }

    void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Enemy"))
        {
            var enemy = collision.GetComponent<EnemyController>();
            if (enemy != null)
                enemy.TakeDamage(damage);
            gameObject.SetActive(false);
        }
        else if (!collision.CompareTag("Player"))
        {
            gameObject.SetActive(false);
        }
    }
}
