// Health.cs
using UnityEngine;
using System;

public class Health : MonoBehaviour
{
    public float maxHealth = 100f;
    public float currentHealth { get; private set; }

    void Awake() => currentHealth = maxHealth;

    public void SetMaxHealth(float m)
    {
        maxHealth = m;
        if(currentHealth > maxHealth)
            currentHealth = maxHealth;
    }
    public void ResetHealth() => currentHealth = maxHealth;

    public void TakeDamage(float d)
    {
        currentHealth = Mathf.Max(currentHealth-d, 0);

        if(GameManager.instance != null)
            GameManager.instance.health = currentHealth;

        if (currentHealth < 0)
            GameManager.instance.GameOver();
    }

    public void heal(float amout)
    {
        currentHealth = Mathf.Min(currentHealth +amout, maxHealth);

        if(GameManager.instance == null)
            GameManager.instance.health = currentHealth;
    }
}

