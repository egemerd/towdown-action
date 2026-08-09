using System;
using UnityEngine;

/// <summary>
/// Player'ýn canýný yönetir. IDamageable implement ederek Enemy attack'lerinden
/// gelen HitInfo mesajlarýný alýr. Invulnerability kontrolü yapar, event'ler fýrlatýr.
/// UI, feedback, death gibi sistemler bu component'in event'lerine subscribe olur —
/// PlayerHealth onlar hakkýnda hiçbir þey bilmez (loose coupling).
/// </summary>
[RequireComponent(typeof(PlayerInvulnerability))]
public class PlayerHealth : MonoBehaviour, IDamageable
{
    [Header("Config")]
    [SerializeField] private PlayerHealthConfigSO config;

    // Runtime state
    private float currentHealth;
    private PlayerInvulnerability invulnerability;

    // Public read-only accessors — UI ve diðer sistemler HP'yi okuyabilsin
    public float CurrentHealth => currentHealth;
    public float MaxHealth => config != null ? config.maxHealth : 0f;
    public float HealthPercent => MaxHealth > 0f ? currentHealth / MaxHealth : 0f;
    public bool IsAlive => currentHealth > 0f;

    // IDamageable event'leri
    public event Action<HitInfo> OnDamageTaken;
    public event Action OnDied;

    // Ekstra event — HP her deðiþtiðinde fýrlar, UI için kullanýþlý
    public event Action<float, float> OnHealthChanged; // (current, max)

    private void Awake()
    {
        if (config == null)
        {
            Debug.LogError("PlayerHealthConfigSO atanmamýþ!", this);
            return;
        }

        invulnerability = GetComponent<PlayerInvulnerability>();
        currentHealth = config.maxHealth;
    }

    public void TakeDamage(HitInfo info)
    {
        if (!IsAlive) return;

        // Guard 2: Invulnerability aktifse hasarý sessizce reddet
        // Event fýrlatmýyoruz çünkü "hasar aldý" mesajý yanlýþ olur
        if (invulnerability != null && invulnerability.IsActive) return;

        // Hasarý uygula
        currentHealth = Mathf.Max(0f, currentHealth - info.damage);

        Debug.Log($"Player took {info.damage} damage from . Current HP: {currentHealth}/{MaxHealth}");
        // Event'leri fýrlat — dinleyiciler bilgilendiriyoruz
        OnDamageTaken?.Invoke(info);
        OnHealthChanged?.Invoke(currentHealth, MaxHealth);

        // Invulnerability window baþlat
        if (invulnerability != null)
        {
            invulnerability.AddSource("damage", config.invulnerabilityDuration);
        }

        // Ölüm kontrolü — hasar sonrasý HP 0'a düþtüyse
        if (!IsAlive)
        {
            OnDied?.Invoke();
        }
    }

    // Þimdilik kullanmýyorsun ama iskeletin scaleability için hazýr — ilerde
    // health pickup'lar veya passive healing için buradan çaðrýlýr
    public void Heal(float amount)
    {
        if (!IsAlive || amount <= 0f) return;

        currentHealth = Mathf.Min(MaxHealth, currentHealth + amount);
        OnHealthChanged?.Invoke(currentHealth, MaxHealth);
    }
}