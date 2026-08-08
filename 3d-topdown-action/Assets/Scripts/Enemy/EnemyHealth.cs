using System;
using UnityEngine;

public class EnemyHealth : MonoBehaviour, IDamageable
{
    float currentHealth = 100f;

    public float CurrentHealth => currentHealth;
    public bool IsAlive => false;

    public event Action<HitInfo> OnDamageTaken;
    public event Action OnDied;

    public void TakeDamage(HitInfo info)
    {
        //if (!isAlive) return;

        currentHealth = Mathf.Max(0, currentHealth - info.damage);
        Debug.Log($"Enemy took {info.damage} damage. Current health: {currentHealth}");
        OnDamageTaken?.Invoke(info);  // HitInfo'yu event ile forward et

        if (currentHealth <= 0)
        {
            OnDied?.Invoke();
        }
    }
}
