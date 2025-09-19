// PlayerContactDamageReceiver.cs
using UnityEngine;

[RequireComponent(typeof(Health))]
public class PlayerContactDamageReceiver : MonoBehaviour
{
    [Min(0f)] public float dps = 10f; // 초당 피해

    private Health health;
    private float lastDamageTime;
    private float damageInterval = 0.5f;

    void Awake() => health = GetComponent<Health>();

    void OnCollisionStay2D(Collision2D collision)
    {
        if (!GameManager.instance.isLive || !collision.collider.CompareTag("Enemy"))
            return;
        if(Time.time - lastDamageTime >= damageInterval)
        {
            health.TakeDamage(dps);
            lastDamageTime = Time.time; 
        }
        
    }
}

