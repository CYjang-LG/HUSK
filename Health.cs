// Health.cs
using UnityEngine;
using System;

public class Health : MonoBehaviour
{
    public float maxHealth = 100f;
    public float currentHealth { get; private set; }

    void Awake() => currentHealth = maxHealth;

    public void SetMaxHealth(float m) => maxHealth = m;
    public void ResetHealth() => currentHealth = maxHealth;

    public void TakeDamage(float d)
    {
        currentHealth = Mathf.Max(currentHealth-d, d);
        if (currentHealth == 0)
            GameManager.instance.GameOver();
    }
}

