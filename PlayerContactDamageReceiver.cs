// PlayerContactDamageReceiver.cs
using UnityEngine;

[RequireComponent(typeof(Health))]
public class PlayerContactDamageReceiver : MonoBehaviour
{
    [Min(0f)] public float dps = 10f; // 초당 피해

    private Health health;

    void Awake() => health = GetComponent<Health>();

    void OnCollisionStay2D(Collision2D collision)
    {
        if (!GameManager.instance.isLive) return;
        health.TakeDamage(dps);
    }
}

