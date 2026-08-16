using System;
using UnityEngine;

public class AttackChainTracker : MonoBehaviour
{
    [Header("Config")]
    [Tooltip("Chain'in reset olmasý için gereken süre (son hit'ten itibaren).")]
    [SerializeField] private float chainResetTime = 2f;
    [SerializeField] private ChainMultiplierProfileSO multiplierProfile;

    [Tooltip("Chain'in ulaþabileceði maksimum deðer. Aþarsa sýfýrlanýr (chain tamamlandý).")]
    [SerializeField] private int maxChainIndex = 3;

    [Header("Debug (runtime)")]
    [SerializeField] private int currentChainIndex = 0;
    [SerializeField] private float chainIndexTimer = 0f;
    [SerializeField] private bool chainActive = false;

    // References
    private PlayerAttack playerAttack;

    // Public API
    public int CurrentChainIndex => currentChainIndex;
    public bool IsInChain => chainActive;

    // Events
    public event Action<int> OnChainIndexChanged;
    public event Action OnChainReset;

    private void Awake()
    {
        playerAttack = GetComponent<PlayerAttack>();
    }

    private void OnEnable()
    {
        //playerAttack.OnHitLanded += HandleAttackCounter;
    }

    private void OnDisable()
    {
        //playerAttack.OnHitLanded -= HandleAttackCounter;
    }

    private void Update()
    {
        ChainTimer();
    }

    void ChainTimer()
    {
        if (!chainActive) return;

        chainIndexTimer += Time.deltaTime;
        if (chainIndexTimer > chainResetTime)
        {
            ResetChain();
        }

    }

    void ResetChain()
    {
        if (!chainActive) return;

        currentChainIndex = 0;
        chainIndexTimer = 0f;
        chainActive = false;
        OnChainReset?.Invoke();
    }
    
    void HandleAttackCounter(IDamageable damageable, HitInfo hitInfo)
    {
        AttackCounter();
    }

    public void AttackCounter()
    {
        currentChainIndex++;
        chainIndexTimer = 0f;
        chainActive = true;

        OnChainIndexChanged?.Invoke(currentChainIndex);

        if (currentChainIndex > maxChainIndex)
        {
            ResetChain();
        }
    }

    public void ForceReset()
    {
        ResetChain();
    }

    public float GetCurrentKnockbackMultiplier()
    {
        if (multiplierProfile == null) return 1f;
        return multiplierProfile.GetKnockbackMultiplier(currentChainIndex);
    }
   
    public bool IsCurrentAttackHeavyKnockback()
    {
        if (multiplierProfile == null) return false;
        return multiplierProfile.IsHeavyKnockback(currentChainIndex);
    }

    public EnemyKnockbackConfigSO GetCurrentKnockbackConfig()
    {
        if (multiplierProfile == null) return null;    
        return multiplierProfile.GetKnockbackConfig(currentChainIndex);
    }

    /// <summary>
    /// Burasý eger enemy dashteyse knockback olmasý iciin kullanýlacak
    /// </summary>

    public EnemyKnockbackConfigSO GetHeavyKnockbackConfig()
    {
        if (multiplierProfile == null) return null;
        return multiplierProfile.GetHeavyKnockbackConfig();
    }
}
