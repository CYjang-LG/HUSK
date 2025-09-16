// Health.cs
using UnityEngine;
using System;

public class Health : MonoBehaviour
{
    public event Action<float> OnHealthChanged;

    [Min(1f)] public float maxHealth = 100f;
    private float cur;

    void OnEnable() => cur = maxHealth;

    public void HealFull()
    {
        cur = maxHealth;
        OnHealthChanged?.Invoke(cur);
    }

    public void Damage(float amountPerSec)
    {
        if (!GameManager.instance.isLive) return;

        cur = Mathf.Max(0f, cur - amountPerSec * Time.deltaTime);
        OnHealthChanged?.Invoke(cur);

        if (cur <= 0f) GameManager.instance.GameOver();
    }

    public float Get() => cur;
}

