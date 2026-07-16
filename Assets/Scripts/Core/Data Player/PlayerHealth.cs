using System;
using UnityEngine;

public class PlayerHealth : MonoBehaviour
{
    [SerializeField] private int maxHealth = 100;
    [SerializeField] private int startHealth = 100;

    public int MaxHealth => Mathf.Max(1, maxHealth);
    public int CurrentHealth { get; private set; }
    public bool IsDead => CurrentHealth <= 0;

    public event Action<int, int> HealthChanged;
    public event Action Died;

    private void Awake()
    {
        CurrentHealth = Mathf.Clamp(startHealth, 0, MaxHealth);
        NotifyHealthChanged();
    }

    public void TakeDamage(int amount)
    {
        if (amount <= 0 || IsDead)
            return;

        CurrentHealth = Mathf.Max(0, CurrentHealth - amount);
        NotifyHealthChanged();

        if (CurrentHealth == 0)
            Died?.Invoke();
    }

    public void Heal(int amount)
    {
        if (amount <= 0 || IsDead)
            return;

        CurrentHealth = Mathf.Min(MaxHealth, CurrentHealth + amount);
        NotifyHealthChanged();
    }

    public void ResetHealth()
    {
        CurrentHealth = MaxHealth;
        NotifyHealthChanged();
    }

    private void NotifyHealthChanged()
    {
        HealthChanged?.Invoke(CurrentHealth, MaxHealth);
    }
}